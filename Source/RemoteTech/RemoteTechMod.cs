using HarmonyLib;
using System;
using UnityEngine.SceneManagement;
using Verse;
using static HugsLib.Utils.HugsLibUtility;

namespace RemoteTech
{
    // This is the new Mod entrypoint. It creates/loads ModSettings and
    // queues a LongEvent to call the old DefsLoaded logic after defs are available.
    public class RemoteTechMod : Mod
    {
        public readonly RemoteTechSettings settings;

        public RemoteTechMod(ModContentPack content) : base(content)
        {
            // load or create settings object
            settings = GetSettings<RemoteTechSettings>();

            new Harmony("Mlie.RemoteTech").PatchAll();

            InjectedDefHasher.PrepareReflection();

            // Queue the legacy DefsLoaded work to run as a long event (ensures defs are ready).
            LongEventHandler.QueueLongEvent(() =>
            {
                try
                {
                    // instantiate controller (previously ModBase instance) and call DefsLoaded
                    RemoteTechController.Initialize(settings);
                    RemoteTechController.Instance.DefsLoaded();
                }
                catch (Exception e)
                {
                    Log.Error($"RemoteTech: DefsLoaded initialization failed: {e}");
                }
            }, "RemoteTech: loading", false, null);

            // Hook sceneLoaded to approximate ModBase.SceneLoaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                // replicate previous SceneLoaded behaviour
                PlayerAvoidanceGrids.ClearAllMaps();
                RemoteTechController.Instance?.SceneLoaded(scene);
            }
            catch (Exception e)
            {
                Log.Error($"RemoteTech: sceneLoaded handler error: {e}");
            }
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            // Draw settings UI similar to HugsLib settings handles
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label((TaggedString)"RemoteTech Settings", -1f, null);
            listing.Gap();

            // draw toggles bound to settings object
            listing.CheckboxLabeled("Setting_autoArmCombat_label".Translate(), ref settings.autoArmCombat,
                "Setting_autoArmCombat_desc".Translate());
            listing.CheckboxLabeled("Setting_autoArmMining_label".Translate(), ref settings.autoArmMining,
                "Setting_autoArmMining_desc".Translate());
            listing.CheckboxLabeled("Setting_autoArmUtility_label".Translate(), ref settings.autoArmUtility,
                "Setting_autoArmUtility_desc".Translate());
            listing.CheckboxLabeled("Setting_miningChargesForbid_label".Translate(), ref settings.miningChargesForbid,
                "Setting_miningChargesForbid_desc".Translate());

            listing.Gap();

            // forbid replaced / timeout
            listing.CheckboxLabeled("Setting_forbidReplaced_label".Translate(), ref settings.forbidReplaced,
                "Setting_forbidReplaced_desc".Translate());

            if (settings.forbidReplaced)
            {
                listing.Label("Setting_forbidTimeout_label".Translate() + $": {settings.forbidTimeout}");
                // integer field
                int temp = settings.forbidTimeout;
                string s = listing.TextEntry(temp.ToString());
                if (int.TryParse(s, out var parsed)) settings.forbidTimeout = parsed;
            }

            // developer-only setting visibility
            if (Prefs.DevMode)
            {
                listing.Gap();
                listing.CheckboxLabeled("Setting_lowerStandingCap_label".Translate(), ref settings.lowerStandingCap,
                    "Setting_lowerStandingCap_desc".Translate());
            }

            listing.End();

            // Save changes
            WriteSettings();
        }

        public override string SettingsCategory() => "RemoteTech";

        public override void WriteSettings()
        {
            base.WriteSettings();
            // ensure the controller reads the latest settings
            RemoteTechController.Instance?.UpdateFromSettings(settings);
        }
    }
}
