using System;
using HugsLib.Utils;
using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     Changes a float value over time according to an interpolation curve. Used for animation.
/// </summary>
public class ValueInterpolator : IExposable
{
    public delegate void FinishedCallback(ValueInterpolator interpolator, float finalValue, float interpolationDuration,
        InterpolationCurves.Curve interpolationCurve);

    private FinishedCallback callback;
    private InterpolationCurves.Curve curve;
    private string curveName;
    private float duration;
    private float elapsedTime;
    public bool finished = true;
    private float initialValue;
    private bool respectTimeScale;
    private float targetValue;

    public float value;

    // deserialization constructor
    public ValueInterpolator()
    {
    }

    public ValueInterpolator(float value = 0f)
    {
        this.value = value;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref value, "value");
        Scribe_Values.Look(ref finished, "finished", true);
        Scribe_Values.Look(ref respectTimeScale, "respectTimeScale", true);
        Scribe_Values.Look(ref elapsedTime, "elapsedTime");
        Scribe_Values.Look(ref initialValue, "initialValue");
        Scribe_Values.Look(ref targetValue, "targetValue");
        Scribe_Values.Look(ref duration, "duration");
        Scribe_Values.Look(ref duration, "duration");
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            curveName = curve?.Method.Name;
        }

        Scribe_Values.Look(ref curveName, "curveName");
        if (Scribe.mode != LoadSaveMode.LoadingVars)
        {
            return;
        }

        if (curveName.NullOrEmpty())
        {
            return;
        }

        var curveMethod = typeof(InterpolationCurves).GetMethod(curveName, HugsLibUtility.AllBindingFlags);
        if (curveMethod == null)
        {
            RemoteTechController.Instance.Logger.Error($"Failed to load interpolation curve: {curveName}");
        }
        else
        {
            curve = (InterpolationCurves.Curve)Delegate.CreateDelegate(typeof(InterpolationCurves.Curve),
                curveMethod, true);
        }
    }

    public ValueInterpolator StartInterpolation(float finalValue, float interpolationDuration,
        InterpolationCurves.Curve curveDelegate)
    {
        initialValue = value;
        elapsedTime = 0;
        targetValue = finalValue;
        duration = interpolationDuration;
        curve = curveDelegate;
        finished = false;
        return this;
    }

    public void StartInterpolation(float finalValue, float interpolationDuration, CurveType curveType)
    {
        StartInterpolation(finalValue, interpolationDuration, InterpolationCurves.AllCurves[curveType]);
    }

    public void SetFinishedCallback(FinishedCallback finishedCallback)
    {
        callback = finishedCallback;
    }

    public void UpdateIfUnpaused()
    {
        if (Find.TickManager.Paused)
        {
            return;
        }

        Update();
    }

    public void Update()
    {
        if (finished)
        {
            return;
        }

        var deltaTime = Time.deltaTime;
        if (respectTimeScale)
        {
            deltaTime *= Find.TickManager.TickRateMultiplier;
        }

        elapsedTime += deltaTime;
        if (elapsedTime >= duration)
        {
            elapsedTime = duration;
            value = targetValue;
            finished = true;
            callback?.Invoke(this, value, duration, curve);
        }
        else
        {
            value = initialValue + (curve(elapsedTime / duration) * (targetValue - initialValue));
        }
    }

    public static implicit operator float(ValueInterpolator v)
    {
        return v.value;
    }
}