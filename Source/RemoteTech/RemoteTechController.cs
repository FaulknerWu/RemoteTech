﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using LudeonTK;
using RimWorld;
using UnityEngine;
using UnityEngine.SceneManagement;
using Verse;

namespace RemoteTech;

/// <summary>
///     The hub of the mod.
///     Injects trader stock generators, generates recipe copies for the workbench and injects comps.
/// </summary>
public class RemoteTechController : ModBase
{
    public const float ComponentReplacementWorkMultiplier = 2f;
    public const float SilverReplacementWorkMultiplier = 1.75f;
    private const float ComponentToSteelRatio = 20f;
    private const float SilverToSparkpowderRatio = 4f;
    private const int ForbiddenTimeoutSettingDefault = 30;
    private const int ForbiddenTimeoutSettingIncrement = 5;

    [TweakValue("Remote Tech")] private static readonly bool showDebugControls = false;

    private readonly MethodInfo objectCloneMethod =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

    public RemoteTechController()
    {
        Instance = this;
    }

    public static RemoteTechController Instance { get; private set; }

    public FieldInfo CompGlowerGlowOnField { get; private set; }
    public PropertyInfo CompGlowerShouldBeLitProperty { get; private set; }

    public SettingHandle<bool> SettingAutoArmCombat { get; private set; }
    public SettingHandle<bool> SettingAutoArmMining { get; private set; }
    public SettingHandle<bool> SettingAutoArmUtility { get; private set; }
    public SettingHandle<bool> SettingMiningChargesForbid { get; private set; }
    public SettingHandle<bool> SettingLowerStandingCap { get; set; }
    private SettingHandle<bool> SettingForbidReplaced { get; set; }
    private SettingHandle<int> SettingForbidTimeout { get; set; }

    public override string ModIdentifier => "RemoteTech";

    public new ModLogger Logger => base.Logger;

    public int BlueprintForbidDuration => SettingForbidReplaced ? SettingForbidTimeout : 0;

    public Dictionary<ThingDef, List<ThingDef>> MaterialToBuilding { get; } =
        new Dictionary<ThingDef, List<ThingDef>>();

    public override void DefsLoaded()
    {
        InjectTraderStocks();
        InjectRecipeVariants();
        InjectVanillaExplosivesComps();
        InjectUpgradeableStatParts();
        PrepareReverseBuildingMaterialLookup();
        GetSettingsHandles();
        PrepareReflection();
        RemoveFoamWallsFromMeteoritePool();
        Compat_DoorsExpanded.OnDefsLoaded();
    }

    public override void SceneLoaded(Scene scene)
    {
        PlayerAvoidanceGrids.ClearAllMaps();
    }

    public override void MapDiscarded(Map map)
    {
        PlayerAvoidanceGrids.DiscardMap(map);
    }

    public T CloneObject<T>(T obj)
    {
        return (T)objectCloneMethod.Invoke(obj, null);
    }

    private void RemoveFoamWallsFromMeteoritePool()
    {
        // foam walls are mineable, but should not appear in a meteorite drop
        ThingSetMaker_Meteorite.nonSmoothedMineables.Remove(Resources.Thing.rxFoamWall);
        ThingSetMaker_Meteorite.nonSmoothedMineables.Remove(Resources.Thing.rxFoamWallSmooth);
        ThingSetMaker_Meteorite.nonSmoothedMineables.Remove(Resources.Thing.rxFoamWallBricks);
        // same for our passable collapsed rock
        ThingSetMaker_Meteorite.nonSmoothedMineables.Remove(Resources.Thing.rxCollapsedRoofRocks);
    }

    private void PrepareReflection()
    {
        CompGlowerShouldBeLitProperty = AccessTools.Property(typeof(CompGlower), "ShouldBeLitNow");
        CompGlowerGlowOnField = AccessTools.Field(typeof(CompGlower), "glowOnInt");
        if (CompGlowerShouldBeLitProperty == null || CompGlowerShouldBeLitProperty.PropertyType != typeof(bool)
                                                  || CompGlowerGlowOnField == null ||
                                                  CompGlowerGlowOnField.FieldType != typeof(bool))
        {
            Logger.Error("Could not reflect required members");
        }
    }

    private void GetSettingsHandles()
    {
        SettingForbidReplaced = Settings.GetHandle("forbidReplaced", "Setting_forbidReplaced_label".Translate(),
            "Setting_forbidReplaced_desc".Translate(), true);
        SettingForbidTimeout = Settings.GetHandle("forbidTimeout", "Setting_forbidTimeout_label".Translate(),
            "Setting_forbidTimeout_desc".Translate(), ForbiddenTimeoutSettingDefault,
            Validators.IntRangeValidator(0, 100000000));
        SettingForbidTimeout.SpinnerIncrement = ForbiddenTimeoutSettingIncrement;
        SettingForbidTimeout.VisibilityPredicate = () => SettingForbidReplaced.Value;
        SettingAutoArmCombat = Settings.GetHandle("autoArmCombat", "Setting_autoArmCombat_label".Translate(),
            "Setting_autoArmCombat_desc".Translate(), true);
        SettingAutoArmMining = Settings.GetHandle("autoArmMining", "Setting_autoArmMining_label".Translate(),
            "Setting_autoArmMining_desc".Translate(), true);
        SettingAutoArmUtility = Settings.GetHandle("autoArmUtility", "Setting_autoArmUtility_label".Translate(),
            "Setting_autoArmUtility_desc".Translate(), true);
        SettingMiningChargesForbid = Settings.GetHandle("miningChargesForbid",
            "Setting_miningChargesForbid_label".Translate(), "Setting_miningChargesForbid_desc".Translate(), true);
        SettingLowerStandingCap = Settings.GetHandle("lowerStandingCap", "Setting_lowerStandingCap_label".Translate(),
            "Setting_lowerStandingCap_label".Translate(), true);
        SettingLowerStandingCap.VisibilityPredicate = () => Prefs.DevMode;
    }

    public override void OnGUI()
    {
        if (showDebugControls)
        {
            DrawDebugControls();
        }
    }

    /// <summary>
    ///     Injects StockGenerators into existing traders.
    /// </summary>
    private void InjectTraderStocks()
    {
        try
        {
            var allInjectors = DefDatabase<TraderStockInjectorDef>.AllDefs;
            var affectedTraders = new List<TraderKindDef>();
            foreach (var injectorDef in allInjectors)
            {
                if (injectorDef.traderDef == null || injectorDef.stockGenerators.Count == 0)
                {
                    continue;
                }

                affectedTraders.Add(injectorDef.traderDef);
                foreach (var stockGenerator in injectorDef.stockGenerators)
                {
                    injectorDef.traderDef.stockGenerators.Add(stockGenerator);
                }
            }

            if (affectedTraders.Count > 0)
            {
                Logger.Trace($"Injected stock generators for {affectedTraders.Count} traders");
            }

            // Unless all defs are reloaded, we no longer need the injector defs
            DefDatabase<TraderStockInjectorDef>.Clear();
        }
        catch (Exception e)
        {
            Logger.ReportException(e);
        }
    }

    /// <summary>
    ///     Injects copies of explosives recipes, substituting ingredients
    /// </summary>
    private void InjectRecipeVariants()
    {
        try
        {
            IEnumerable<RecipeDef> GetRecipesRequestingVariant(RecipeVariantType variant)
            {
                return DefDatabase<RecipeDef>.AllDefs.Where(r =>
                    r.GetModExtension<MakeRecipeVariants>() is { } v &&
                    v.CreateVariants.Contains(variant)).ToArray();
            }

            // components to steel variants
            var injectCount = 0;
            foreach (var explosiveRecipe in GetRecipesRequestingVariant(RecipeVariantType.Steel))
            {
                var variant = TryMakeRecipeVariant(explosiveRecipe, RecipeVariantType.Steel,
                    ThingDefOf.ComponentIndustrial, ThingDefOf.Steel, ComponentToSteelRatio,
                    ComponentReplacementWorkMultiplier);
                if (variant == null)
                {
                    continue;
                }

                DefDatabase<RecipeDef>.Add(variant);
                injectCount++;
            }

            // silver to sparkpowder variants
            foreach (var explosiveRecipe in GetRecipesRequestingVariant(RecipeVariantType.Sparkpowder))
            {
                var variant = TryMakeRecipeVariant(explosiveRecipe, RecipeVariantType.Sparkpowder, ThingDefOf.Silver,
                    Resources.Thing.rxSparkpowder, SilverToSparkpowderRatio, SilverReplacementWorkMultiplier);
                if (variant == null)
                {
                    continue;
                }

                DefDatabase<RecipeDef>.Add(variant);
                injectCount++;
            }

            if (injectCount > 0)
            {
                Logger.Trace($"Injected {injectCount} alternate explosives recipes.");
            }
        }
        catch (Exception e)
        {
            Logger.ReportException(e);
        }
    }

    /// <summary>
    ///     Generates a copy of a given recipe with the provided ingredient replaced with another, at the given ratio.
    ///     Will return null if recipe requires none of the original ingredient.
    /// </summary>
    private RecipeDef TryMakeRecipeVariant(RecipeDef recipeOriginal, RecipeVariantType variant,
        ThingDef originalIngredient, ThingDef replacementIngredient, float replacementRatio, float workAmountMultiplier)
    {
        // check original recipe for the replaced ingredient, copy other ingredients
        var resourceCountRequired = 0f;
        var newIngredientList = new List<IngredientCount>(recipeOriginal.ingredients);
        foreach (var ingredientCount in newIngredientList)
        {
            if (!ingredientCount.filter.Allows(originalIngredient))
            {
                continue;
            }

            resourceCountRequired = ingredientCount.GetBaseCount();
            newIngredientList.Remove(ingredientCount);
            break;
        }

        if (resourceCountRequired < float.Epsilon)
        {
            return null;
        }

        var recipeCopy = CloneObject(recipeOriginal);
        recipeCopy.defName = $"{recipeOriginal.defName}_{replacementIngredient.defName}";
        recipeCopy.shortHash = 0;
        InjectedDefHasher.GiveShortHashToDef(recipeCopy, typeof(RecipeDef));
        // clone our extension to avoid polluting the original def
        recipeCopy.modExtensions = recipeCopy.modExtensions
            ?.Select(e => e is ICloneable i ? (DefModExtension)i.Clone() : e).ToList();
        if (!recipeOriginal.HasModExtension<MakeRecipeVariants>())
        {
            // mark original as a variant, as well
            recipeOriginal.modExtensions = recipeOriginal.modExtensions ?? new List<DefModExtension>();
            recipeOriginal.modExtensions.Add(new MakeRecipeVariants());
        }

        // mark the copy as variant
        var variantExtension = recipeCopy.GetModExtension<MakeRecipeVariants>();
        if (variantExtension == null)
        {
            variantExtension = new MakeRecipeVariants();
            recipeCopy.modExtensions = recipeCopy.modExtensions ?? new List<DefModExtension>();
            recipeCopy.modExtensions.Add(variantExtension);
        }

        variantExtension.Variant |= variant;

        // copy non-replaced ingredients over to the new filter
        var newFixedFilter = new ThingFilter();
        foreach (var allowedThingDef in recipeOriginal.fixedIngredientFilter.AllowedThingDefs)
        {
            if (allowedThingDef != originalIngredient)
            {
                newFixedFilter.SetAllow(allowedThingDef, true);
            }
        }

        newFixedFilter.SetAllow(replacementIngredient, true);
        recipeCopy.fixedIngredientFilter = newFixedFilter;
        recipeCopy.defaultIngredientFilter = null;

        // add the replacement ingredient
        var replacementIngredientFilter = new ThingFilter();
        replacementIngredientFilter.SetAllow(replacementIngredient, true);
        var replacementCount = new IngredientCount { filter = replacementIngredientFilter };
        replacementCount.SetBaseCount(Mathf.Round(resourceCountRequired * replacementRatio));
        newIngredientList.Add(replacementCount);
        recipeCopy.ingredients = newIngredientList;

        // multiply work amount
        recipeCopy.workAmount = recipeOriginal.workAmount * workAmountMultiplier;

        recipeCopy.ResolveReferences();
        return recipeCopy;
    }

    /// <summary>
    ///     Add comps to vanilla IED's so that they can be triggered by the manual detonator
    /// </summary>
    private void InjectVanillaExplosivesComps()
    {
        try
        {
            var ieds = new List<ThingDef>
            {
                GetDefWithWarning("FirefoamPopper")
            };
            ieds.AddRange(DefDatabase<ThingDef>.AllDefs.Where(d =>
                d != null && d.thingClass == typeof(Building_TrapExplosive)));
            foreach (var thingDef in ieds)
            {
                if (thingDef == null)
                {
                    continue;
                }

                thingDef.comps.Add(new CompProperties_WiredDetonationReceiver());
                thingDef.comps.Add(new CompProperties_AutoReplaceable());
            }

            ThingDefOf.PassiveCooler.comps.Add(new CompProperties_AutoReplaceable { applyOnVanish = true });
        }
        catch (Exception e)
        {
            Logger.ReportException(e);
        }
    }

    /// <summary>
    ///     Add StatPart_Upgradeable to all stats that are used in any CompProperties_Upgrade
    /// </summary>
    private void InjectUpgradeableStatParts()
    {
        try
        {
            var relevantStats = new HashSet<StatDef>();
            var allThings = DefDatabase<ThingDef>.AllDefs.ToArray();
            foreach (var def in allThings)
            {
                if (def.comps.Count <= 0)
                {
                    continue;
                }

                foreach (var comp in def.comps)
                {
                    if (comp is not CompProperties_Upgrade upgradeProps)
                    {
                        continue;
                    }

                    foreach (var upgradeProp in upgradeProps.statModifiers)
                    {
                        relevantStats.Add(upgradeProp.stat);
                    }
                }
            }

            foreach (var stat in relevantStats)
            {
                var parts = stat.parts ?? (stat.parts = new List<StatPart>());
                parts.Add(new StatPart_Upgradeable { parentStat = stat });
            }
        }
        catch (Exception e)
        {
            Logger.ReportException(e);
        }
    }

    /// <summary>
    ///     Finds all buildings in which things with CompBuildGizmo are used as materials.
    /// </summary>
    /// <see cref="CompBuildGizmo" />
    /// <see cref="CompProperties_BuildGizmo" />
    private void PrepareReverseBuildingMaterialLookup()
    {
        try
        {
            // find all defs with our comp
            MaterialToBuilding.Clear();
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                foreach (var compProperties in def.comps)
                {
                    if (compProperties is not CompProperties_BuildGizmo)
                    {
                        continue;
                    }

                    MaterialToBuilding.Add(def, new List<ThingDef>());
                    break;
                }
            }

            // find all buildings with our material def in their costList
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.category != ThingCategory.Building || def.costList == null)
                {
                    continue;
                }

                foreach (var countClass in def.costList)
                {
                    var materialDef = countClass?.thingDef;
                    if (materialDef == null || !MaterialToBuilding.TryGetValue(materialDef, out var buildingDefs))
                    {
                        continue;
                    }

                    buildingDefs.Add(def);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Logger.ReportException(e);
        }
    }

    private ThingDef GetDefWithWarning(string defName)
    {
        var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (def == null)
        {
            Logger.Warning($"Could not get ThingDef for Comp injection: {defName}");
        }

        return def;
    }

    private void DrawDebugControls()
    {
        var map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }

        if (Widgets.ButtonText(new Rect(10, 10, 50, 20), "Cloud"))
        {
            DebugTools.curTool = new DebugTool("GasCloud placer", () =>
            {
                const float concentration = 10000000;
                var cell = UI.MouseCell();
                var cloud = map.thingGrid.ThingAt<GasCloud>(cell);
                if (cloud != null)
                {
                    cloud.ReceiveConcentration(concentration);
                }
                else
                {
                    cloud = (GasCloud)ThingMaker.MakeThing(Resources.Thing.rxGas_Sleeping);
                    cloud.ReceiveConcentration(concentration);
                    GenPlace.TryPlaceThing(cloud, cell, map, ThingPlaceMode.Direct);
                }
            });
        }

        if (Widgets.ButtonText(new Rect(10, 30, 50, 20), "Spark"))
        {
            DebugTools.curTool = new DebugTool("Spark",
                () =>
                {
                    Resources.Effecter.rxSparkweedIgnite.Spawn().Trigger(new TargetInfo(UI.MouseCell(), map), null);
                });
        }

        if (Widgets.ButtonText(new Rect(10, 50, 50, 20), "Failure"))
        {
            DebugTools.curTool = new DebugTool("Failure",
                () =>
                {
                    Resources.Effecter.rxDetWireFailure.Spawn().Trigger(new TargetInfo(UI.MouseCell(), map), null);
                });
        }
    }
}