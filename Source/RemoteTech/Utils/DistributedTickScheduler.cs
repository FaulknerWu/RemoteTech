using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RemoteTech;

public class DistributedTickScheduler
{
	public class TickableEntry
	{
		public readonly Action callback;

		public readonly int interval;

		public readonly Thing owner;

		public TickableEntry(Action callback, int interval, Thing owner)
		{
			this.callback = callback;
			this.interval = interval;
			this.owner = owner;
		}
	}

	private class ListTicker
	{
		public readonly int tickInterval;

		private readonly DistributedTickScheduler scheduler;

		private readonly List<TickableEntry> tickList = new List<TickableEntry>();

		private int currentIndex;

		private float listProgress;

		private int nextCycleStart;

		private int numCalls;

		private bool tickInProgress;

		public int NumCallsLastTick => numCalls;

		public int EntryCount => tickList.Count;

		public ListTicker(int tickInterval, DistributedTickScheduler scheduler)
		{
			this.tickInterval = tickInterval;
			this.scheduler = scheduler;
		}

		public void Tick(int currentTick)
		{
			tickInProgress = true;
			numCalls = 0;
			if (nextCycleStart <= currentTick)
			{
				currentIndex = 0;
				listProgress = 0f;
				nextCycleStart = currentTick + tickInterval;
			}
			listProgress += (float)tickList.Count / (float)tickInterval;
			int num = Mathf.Min(tickList.Count, Mathf.CeilToInt(listProgress));
			while (currentIndex < num)
			{
				TickableEntry tickableEntry = tickList[currentIndex];
				if (tickableEntry.owner.Spawned)
				{
					try
					{
						tickableEntry.callback();
						numCalls++;
					}
					catch (Exception ex)
					{
						Log.Error($"DistributedTickScheduler caught an exception! {ex}");
					}
				}
				else
				{
					scheduler.UnregisterAtEndOfTick(tickableEntry.owner);
				}
				currentIndex++;
			}
			tickInProgress = false;
		}

		public void Register(TickableEntry entry)
		{
			AssertNotTicking();
			tickList.Add(entry);
		}

		public void Unregister(TickableEntry entry)
		{
			AssertNotTicking();
			tickList.Remove(entry);
		}

		private void AssertNotTicking()
		{
			if (tickInProgress)
			{
				throw new Exception("Cannot register or unregister a callback while a tick is in progress");
			}
		}
	}

	private readonly Dictionary<Thing, TickableEntry> entries = new Dictionary<Thing, TickableEntry>();

	private readonly List<ListTicker> tickers = new List<ListTicker>();

	private readonly Queue<Thing> unregisterQueue = new Queue<Thing>();

	private int lastProcessedTick = -1;

	internal DistributedTickScheduler()
	{
	}

	public void RegisterTickability(Action callback, int tickInterval, Thing owner)
	{
		if (lastProcessedTick < 0)
		{
			throw new Exception("Adding callback to not initialized DistributedTickScheduler");
		}
		if (owner == null || owner.Destroyed)
		{
			throw new Exception("A non-null, not destroyed owner Thing is required to register for tickability");
		}
		if (tickInterval < 1)
		{
			throw new Exception("Invalid tick interval: " + tickInterval);
		}
		if (entries.ContainsKey(owner))
		{
			Log.Warning("DistributedTickScheduler tickability already registered for: " + owner);
			return;
		}
		TickableEntry tickableEntry = new TickableEntry(callback, tickInterval, owner);
		GetTicker(tickInterval).Register(tickableEntry);
		entries.Add(owner, tickableEntry);
	}

	public void UnregisterTickability(Thing owner)
	{
		if (!IsRegistered(owner))
		{
			throw new ArgumentException("Cannot unregister non-registered owner: " + owner);
		}
		TickableEntry tickableEntry = entries[owner];
		ListTicker ticker = GetTicker(tickableEntry.interval);
		ticker.Unregister(tickableEntry);
		if (ticker.EntryCount == 0)
		{
			tickers.Remove(ticker);
		}
		entries.Remove(owner);
	}

	public bool IsRegistered(Thing owner)
	{
		return entries.ContainsKey(owner);
	}

	public IEnumerable<TickableEntry> DebugGetAllEntries()
	{
		return entries.Values;
	}

	public int DebugCountLastTickCalls()
	{
		return tickers.Sum((ListTicker t) => t.NumCallsLastTick);
	}

	public int DebugGetNumTickers()
	{
		return tickers.Count;
	}

	internal void Initialize(int currentTick)
	{
		entries.Clear();
		tickers.Clear();
		lastProcessedTick = currentTick;
	}

	internal void Tick(int currentTick)
	{
		if (lastProcessedTick < 0)
		{
			throw new Exception("Ticking not initialized DistributedTickScheduler");
		}
		lastProcessedTick = currentTick;
		for (int i = 0; i < tickers.Count; i++)
		{
			tickers[i].Tick(currentTick);
		}
		UnregisterQueuedOwners();
	}

	private void UnregisterAtEndOfTick(Thing owner)
	{
		unregisterQueue.Enqueue(owner);
	}

	private void UnregisterQueuedOwners()
	{
		while (unregisterQueue.Count > 0)
		{
			Thing owner = unregisterQueue.Dequeue();
			if (IsRegistered(owner))
			{
				UnregisterTickability(owner);
			}
		}
	}

	private ListTicker GetTicker(int interval)
	{
		for (int i = 0; i < tickers.Count; i++)
		{
			if (tickers[i].tickInterval == interval)
			{
				return tickers[i];
			}
		}
		ListTicker listTicker = new ListTicker(interval, this);
		tickers.Add(listTicker);
		return listTicker;
	}
}
