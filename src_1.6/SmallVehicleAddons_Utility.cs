using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vehicles;
using Verse;

namespace SmallVehicleAddons
{
	public static class SmallVehicleAddons_Utility
    {
        public const byte IntNorth = 0;
        public const byte IntSouth = 2;
        public const byte IntEast = 1;
        public const byte IntWest = 3;
        public const byte IntNorthEast = 4;
        public const byte IntSouthEast = 5;
        public const byte IntSouthWest = 6;
        public const byte IntNorthWest = 7;
        private const float scanRadius = 6.9f;
        private const float defaultLightsEffectiveDistance = 16f;
        public static readonly int PlaceNearMiddleRadialCells = GenRadial.NumCellsInRadius(3f);
        private static Dictionary<GlowGrid, List<CompLights>> lightsByGrid = new Dictionary<GlowGrid, List<CompLights>>();
        private const string ExceptionMessageStart = "[Small Vehicle Addons]: Something went wrong during ";
        public static string ExceptionMessage(string reason, string message)
        {
            return ExceptionMessageStart + reason + ". Exception message: " + message;
        }
        public static void VehiclesMassAdjust()
        {
            if (ModSettings_SmallVehicleAddons.Instance.vehiclesMassAdjust)
            {
                try
                {
                    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading;
                    if (list.NullOrEmpty())
                    {
                        return;
                    }
                    foreach (VehicleDef item in list)
                    {
                        try
                        {
                            VehicleStatModifier vehicleMass = item?.vehicleStats?.Find((VehicleStatModifier x) => x.statDef == VehicleStatDefOf.Mass);
                            if (vehicleMass == null)
                            {
                                continue;
                            }
                            if (vehicleMass.value <= 0)
                            {
                                continue;
                            }
                            item.SetStatBaseValue(StatDefOf.Mass, vehicleMass.value);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ExceptionMessage($"giving vanilla mass to vehicle of def: {item}", ex.Message + "\n" + ex.StackTrace));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ExceptionMessage("giving vanilla mass to vehicles", ex.Message + "\n" + ex.StackTrace));
                }
            }
        }
        public static void AddFastCargoGizmo()
        {
            if (ModSettings_SmallVehicleAddons.Instance.addFastCargoGizmo)
            {
                try
                {
                    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading;
                    if (list.NullOrEmpty())
                    {
                        return;
                    }
                    foreach (VehicleDef item in list)
                    {
                        try
                        {
                            if (!item.HasComp<CompFastCargo>())
                            {
                                item.comps?.Add(new CompProperties_CompFastCargo
                                {
                                    activateSound = SoundDef.Named("SMA_Sound_Fast_Cargo"),
                                    thingsScanRadius = scanRadius + ((item.size.x * item.size.z) * 0.05f)
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ExceptionMessage($"adding fast cargo comp to vehicle of def: {item}", ex.Message + "\n" + ex.StackTrace));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ExceptionMessage("adding fast cargo comp to vehicles", ex.Message + "\n" + ex.StackTrace));
                }
            }
        }
        public static void AddLightsGizmo()
        {
            if (ModSettings_SmallVehicleAddons.Instance.addLightsGizmo)
            {
                try
                {
                    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading;
                    if (list.NullOrEmpty())
                    {
                        return;
                    }
                    HashSet<string> whitelist = SmallVehicleAddonsDefOf.Headlights_Whitelist?.stringList != null ? SmallVehicleAddonsDefOf.Headlights_Whitelist.stringList.ToHashSet() : new HashSet<string>();
                    foreach (VehicleDef item in list)
                    {
                        try
                        {
                            if (((SmallVehicleAddons_Utility.VehicleDefShouldHaveExtraGizmo(item) && SmallVehicleAddons_Utility.VehicleMovementUnlocked(item)) || whitelist.Contains(item.defName)) && !item.HasComp<CompLights>())
                            {
                                if (item.Size.Area > 1)
                                {
                                    item.comps?.Add(new CompProperties_CompLights
                                    {
                                        turnOnSoundDef = SoundDef.Named("SMA_Sound_Lights_On"),
                                        turnOffSoundDef = SoundDef.Named("SMA_Sound_Lights_Off"),
                                        nearbyLightThingDef = ThingDef.Named("SMA_Vehicle_Light"),
                                        lightsEffectiveDistance = defaultLightsEffectiveDistance + (item.Size.Area / 2),
                                        //selfLightThingDef = ModSettings_SmallVehicleAddons.Instance.shouldLightSelf ? ThingDef.Named("SMA_Vehicle_Light_Self") : null,
                                        //farLightAllowed = true
                                    });
                                }
                                else if (item.Size.Area == 1)
                                {
                                    item.comps?.Add(new CompProperties_CompLights
                                    {
                                        turnOnSoundDef = SoundDef.Named("SMA_Sound_Lights_On"),
                                        turnOffSoundDef = SoundDef.Named("SMA_Sound_Lights_Off"),
                                        //nearbyLightThingDef = ThingDef.Named("SMA_Vehicle_Light"),
                                        //selfLightThingDef = ModSettings_SmallVehicleAddons.Instance.shouldLightSelf ? ThingDef.Named("SMA_Vehicle_Light_Self") : null,
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ExceptionMessage($"adding lights comp to vehicle of def: {item}", ex.Message + "\n" + ex.StackTrace));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ExceptionMessage("adding lights comp to vehicles", ex.Message + "\n" + ex.StackTrace));
                }
            }
        }
        public static void AddHonkGizmo()
        {
            if (ModSettings_SmallVehicleAddons.Instance.addHonkGizmo)
            {
                try
                {
                    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading;
                    if (list.NullOrEmpty())
                    {
                        return;
                    }
                    HashSet<string> whitelist = SmallVehicleAddonsDefOf.Honk_Whitelist?.stringList != null ? SmallVehicleAddonsDefOf.Honk_Whitelist.stringList.ToHashSet() : new HashSet<string>();
                    foreach (VehicleDef item in list)
                    {
                        try
                        {
                            if (((SmallVehicleAddons_Utility.VehicleDefShouldHaveExtraGizmo(item) && item.type >= VehicleType.Land) || whitelist.Contains(item.defName)) && !item.HasComp<CompHonk>())
                            {
                                if (item.Size.Area == 1 || (SmallVehicleAddonsDefOf.SillyString_MotorcycleDefs != null && !SmallVehicleAddonsDefOf.SillyString_MotorcycleDefs.stringList.NullOrEmpty() && SmallVehicleAddonsDefOf.SillyString_MotorcycleDefs.stringList.Contains(item.defName)))
                                {
                                    item.comps?.Add(new CompProperties_CompHonk
                                    {
                                        soundDef = SoundDef.Named("SMA_Sound_Horn_Motorcycle"),
                                    });
                                }
                                else if (item.Size.Area <= 8 || (SmallVehicleAddonsDefOf.SillyString_CarDefs != null && !SmallVehicleAddonsDefOf.SillyString_CarDefs.stringList.NullOrEmpty() && SmallVehicleAddonsDefOf.SillyString_CarDefs.stringList.Contains(item.defName)))
                                {
                                    item.comps?.Add(new CompProperties_CompHonk
                                    {
                                        soundDef = SoundDef.Named("SMA_Sound_Horn_Car"),
                                    });
                                }
                                else if (item.Size.Area > 8 || (SmallVehicleAddonsDefOf.SillyString_TruckDefs != null && !SmallVehicleAddonsDefOf.SillyString_TruckDefs.stringList.NullOrEmpty() && SmallVehicleAddonsDefOf.SillyString_TruckDefs.stringList.Contains(item.defName)))
                                {
                                    item.comps?.Add(new CompProperties_CompHonk
                                    {
                                        soundDef = SoundDef.Named("SMA_Sound_Horn_Truck"),
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ExceptionMessage($"adding honk comp to vehicle of def: {item}", ex.Message + "\n" + ex.StackTrace));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ExceptionMessage("adding honk comp to vehicles", ex.Message + "\n" + ex.StackTrace));
                }
            }
        }
        public static void AddNeedsRefillment()
        {
            if (ModSettings_SmallVehicleAddons.Instance.addNeedsRefillment)
            {
                try
                {
                    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading;
                    if (list.NullOrEmpty())
                    {
                        return;
                    }
                    foreach (VehicleDef item in list)
                    {
                        try
                        {
                            if (!item.HasComp<CompRefillNeeds>())
                            {
                                item.comps?.Add(new CompProperties_CompRefillNeeds
                                {
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ExceptionMessage($"adding needs refillment comp to vehicle of def: {item}", ex.Message + "\n" + ex.StackTrace));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ExceptionMessage("adding needs refillment comp to vehicles", ex.Message + "\n" + ex.StackTrace));
                }
            }
        }
        private const string DBHThirst = "DBHThirst";
        private const string WaterExt = "WaterExt";
        private const string SeekForThirst = "SeekForThirst";
        public const byte LMBInt = 0;
        public const byte RMBInt = 1;
        public static bool ActionOnLMB(Event ev, Action a)
        {
            if (ev?.button == LMBInt)
            {
                a?.Invoke();
                return true;
            }
            return false;
        }
        public static bool ActionOnRMB(Event ev, Action a)
        {
            if (ev?.button == RMBInt)
            {
                a?.Invoke();
                return true;
            }
            return false;
        }
        public static ref F GetFieldRefOfInstance<F>(object i, string fieldName)
        {
            if (i == null)
            {
                throw new ArgumentNullException("i");
            }
            AccessTools.FieldRef<object, F> f = AccessTools.FieldRefAccess<F>(i.GetType(), fieldName);
            return ref f(i);
        }
        public static bool DubsBadHygiene()
        {
            return ModsConfig.IsActive("Dubwise.DubsBadHygiene");
        }
        public static List<T> GetNearbyThings<T>(Map map, IntVec3 center, float scanRadius, Predicate<T> validator) where T : Thing
        {
            List<T> tmpThings = new List<T>();
            try
            {
                int scanCellCount = GenRadial.NumCellsInRadius(scanRadius);
                for (int i = 0; i < scanCellCount; i++)
                {
                    IntVec3 c = center + GenRadial.RadialPattern[i];
                    if (!c.InBounds(map))
                    {
                        continue;
                    }
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (!(thingList[j] is T thing))
                        {
                            continue;
                        }
                        else if (validator(thing))
                        {
                            tmpThings.Add(thing);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ExceptionMessage("GetNearbyThings", ex.Message + "\n" + ex.StackTrace));
            }
            return tmpThings;
        }
        public static bool CanPawnBeScaredByHonk(Pawn pawn, Map map, IntVec3 vehiclePos)
        {
            return pawn != null && pawn.Spawned && (pawn.Faction == null || (pawn.Faction != null && (!pawn.Faction.HostileTo(Faction.OfPlayer) || (int)pawn.Faction.def.techLevel <= 3))) && !(pawn is VehiclePawn) && !pawn.DeadOrDowned && !pawn.InAggroMentalState && (((pawn.health?.capacities != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing) && (pawn.def.race?.FleshType != FleshTypeDefOf.EntityMechanical || pawn.def.race?.FleshType != FleshTypeDefOf.Mechanoid) && pawn.def.race.IsFlesh)) && GenSight.LineOfSight(vehiclePos, pawn.Position, map, skipFirstCell: true));
        }
        public static bool HasOperator(VehiclePawn v)
        {
            if (v.ignition.Drafted)
            {
                return true;
            }
            foreach (VehicleRoleHandler handler in v.handlers)
            {
                if (handler.role.HandlingTypes == HandlingType.Movement && handler.RoleFulfilled)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasOperator(VehiclePawn v, string key, out List<Pawn> operators)
        {
            operators = null;
            if (v == null)
            {
                return false;
            }
            operators = new List<Pawn>();
            bool flag = false;
            foreach (VehicleRoleHandler handler in v.handlers)
            {
                if (key.NullOrEmpty())
                {
                    if (handler.role.HandlingTypes == HandlingType.None)
                    {
                        foreach (Pawn p in handler.thingOwner)
                        {
                            operators.Add(p);
                        }
                        flag = true;
                    }
                }
                else
                {
                    if (handler.role.key == key)
                    {
                        foreach (Pawn p in handler.thingOwner)
                        {
                            operators.Add(p);
                        }
                        return true;
                    }
                }
            }
            return flag;
        }
        public static bool VehicleMovementUnlocked(this VehicleDef vehicleDef)
        {
            return vehicleDef?.MovementPermissions > VehiclePermissions.None;
        }
        public static bool VehicleDefShouldHaveExtraGizmo(VehicleDef vehicleDef)
		{
			return vehicleDef?.techLevel > TechLevel.Medieval;
		}
        public static void ExecuteVehicleEvent(VehiclePawn vehicle, VehicleEventDef vehicleEventDef)
        {
            if (vehicle == null || vehicleEventDef == null)
            {
                return;
            }
            vehicle.EventRegistry[vehicleEventDef]?.ExecuteEvents();
        }
        public static Need TryGetNeed(this Pawn pawn, string defName)
        {
            if (pawn.needs == null)
            {
                return null;
            }
            List<Need> needs = pawn.needs.AllNeeds;
            if (needs.NullOrEmpty())
            {
                return null;
            }
            for (int i = 0; i < needs.Count; i++)
            {
                if (needs[i].def.defName == defName)
                {
                    return needs[i];
                }
            }
            return null;
        }
        public static void TrySatisfyPassengersNeeds(VehiclePawn vp, string key)
        {
            try
            {
                if (HasOperator(vp, key, out var operators))
                {
                    for (int i = 0; i < operators.Count; i++)
                    {
                        Pawn pawn = operators[i];
                        if (TryRefillFood(pawn, pawn.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                        if (TryRefillFood(pawn, vp.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                    }
                    for (int i = 0; i < operators.Count; i++)
                    {
                        Pawn pawn = operators[i];
                        if (TryRefillChemicalNeeds(pawn, pawn.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                        if (TryRefillChemicalNeeds(pawn, vp.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                    }
                    for (int i = 0; i < operators.Count; i++)
                    {
                        Pawn pawn = operators[i];
                        if (TryRefillChemicalDependencies(pawn, pawn.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                        if (TryRefillChemicalDependencies(pawn, vp.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                    }
                    if (DubsBadHygiene())
                    {
                        for (int i = 0; i < operators.Count; i++)
                        {
                            Pawn pawn = operators[i];
                            if (TryRefillBladder(pawn))
                            {
                                AddWastes();
                            }
                        }
                    }
                }
                if (DubsBadHygiene())
                {
                    for (int i = 0; i < vp.Passengers.Count; i++)
                    {
                        Pawn pawn = vp.Passengers[i];
                        if (TryRefillThrist(pawn, pawn.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                        if (TryRefillThrist(pawn, vp.inventory.innerContainer.InnerListForReading))
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ExceptionMessage("TrySatisfyPassengersNeeds", ex.Message + "\n" + ex.StackTrace));
            }
            bool TryRefillBladder(Pawn pawn)
            {
                Need bladder = TryGetNeed(pawn, "Bladder");
                if (bladder != null && bladder.CurLevelPercentage <= 0.3f)
                {
                    bladder.CurLevelPercentage += 0.7f;
                    return true;
                }
                return false;
            }
            void AddWastes()
            {
                ThingDef def = ThingDef.Named("BedPan");
                if (def != null)
                {
                    vp.inventory?.TryAddAndUnforbid(ThingMaker.MakeThing(def));
                }
            }
        }
        public static bool TryRefillFood(Pawn pawn, List<Thing> innerContainer)
        {
            Need_Food food = pawn?.needs?.TryGetNeed<Need_Food>();
            if (food != null && food.CurCategory >= HungerCategory.Hungry)
            {
                if (TryGetBestFood(innerContainer, pawn, out var f))
                {
                    food.CurLevel += f.Ingested(pawn, food.NutritionWanted);
                    return true;
                }
            }
            return false;
        }
        public static bool TryGetBestFood(List<Thing> sourceList, Pawn forPawn, out Thing food)
        {
            food = null;
            Thing thing = null;
            float num = 0f;
            if (sourceList.NullOrEmpty())
            {
                return false;
            }
            for (int i = 0; i < sourceList.Count; i++)
            {
                Thing thing2 = sourceList[i];
                if (thing2.def.IsIngestible && CaravanPawnsNeedsUtility.CanEatForNutritionNow(thing2, forPawn))
                {
                    float foodScore = CaravanPawnsNeedsUtility.GetFoodScore(thing2, forPawn);
                    if (thing == null || foodScore > num)
                    {
                        thing = thing2;
                        num = foodScore;
                    }
                }
            }
            if (thing != null)
            {
                food = thing;
                return true;
            }
            return false;
        }
        public static bool TryRefillChemicalNeeds(Pawn pawn, List<Thing> innerContainer)
        {
            List<Need> allNeeds = pawn?.needs?.AllNeeds;
            if (allNeeds.NullOrEmpty())
            {
                return false;
            }
            bool flag = false;
            for (int i = 0; i < allNeeds.Count; i++)
            {
                if (allNeeds[i] is Need_Chemical chemical && (int)chemical.CurCategory < 2)
                {
                    TrySatisfyChemicalNeed(chemical);
                }
            }
            return flag;
            void TrySatisfyChemicalNeed(Need_Chemical chemical)
            {
                if (TryGetBestDrug(innerContainer, pawn, chemical.AddictionHediff, out var drug))
                {
                    float num = drug.Ingested(pawn, 0f);
                    Need_Food food = pawn.needs.food;
                    if (food != null)
                    {
                        food.CurLevel += num;
                    }
                    flag = true;
                }
            }
        }
        public static bool TryRefillChemicalDependencies(Pawn pawn, List<Thing> innerContainer)
        {
            List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;
            if (hediffs.NullOrEmpty())
            {
                return false;
            }
            bool flag = false;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_ChemicalDependency hediff_ChemicalDependency && hediff_ChemicalDependency.ShouldSatify)
                {
                    TrySatisfyChemicalDependency(hediff_ChemicalDependency);
                }
            }
            return flag;
            void TrySatisfyChemicalDependency(Hediff_ChemicalDependency chemical)
            {
                if (TryGetBestDrug(innerContainer, pawn, chemical, out var drug))
                {
                    float num = drug.Ingested(pawn, 0f);
                    Need_Food food = pawn.needs.food;
                    if (food != null)
                    {
                        food.CurLevel += num;
                    }
                    flag = true;
                }
            }
        }
        public static bool TryGetBestDrug(List<Thing> sourceList, Pawn forPawn, Hediff hediff, out Thing drug)
        {
            drug = null;
            if (hediff == null || sourceList.NullOrEmpty())
            {
                return false;
            }
            Thing thing = null;
            for (int i = 0; i < sourceList.Count; i++)
            {
                Thing thing2 = sourceList[i];
                if (!thing2.def.IsIngestible || !thing2.def.IsDrug)
                {
                    continue;
                }
                CompDrug compDrug = thing2.TryGetComp<CompDrug>();
                ChemicalDef chemical = compDrug?.Props?.chemical;
                if (chemical != null && (!(hediff is Hediff_ChemicalDependency hediff_ChemicalDependency) || chemical == hediff_ChemicalDependency.chemical) && (!(hediff is Hediff_Addiction hediff_Addiction) || chemical.addictionHediff == hediff_Addiction.def))
                {
                    DrugPolicy drugPolicy = forPawn.drugs?.CurrentPolicy;
                    if (drugPolicy == null || drugPolicy[thing2.def].allowedForAddiction || forPawn.story?.traits?.DegreeOfTrait(TraitDefOf.DrugDesire) > 0)
                    {
                        thing = thing2;
                        break;
                    }
                }
            }
            if (thing != null)
            {
                drug = thing;
                return true;
            }
            return false;
        }
        public static bool TryRefillThrist(Pawn pawn, List<Thing> innerContainer)
        {
            Need thirst = TryGetNeed(pawn, DBHThirst);
            if (thirst != null && thirst.CurLevelPercentage <= 0.3f)
            {
                Thing t = innerContainer?.FirstOrDefault((Thing x) => x.def.IsIngestible && !x.def.IsDrug && WaterExt_SeekForThirst(x.def));
                if (t != null)
                {
                    t.Ingested(pawn, 0f);
                    return true;
                }
            }
            return false;
            bool WaterExt_SeekForThirst(ThingDef def)
            {
                if (def.modExtensions == null)
                {
                    return false;
                }
                for (int i = 0; i < def.modExtensions.Count; i++)
                {
                    if (def.modExtensions[i].GetType().Name == WaterExt)
                    {
                        ref bool flag = ref GetFieldRefOfInstance<bool>(def.modExtensions[i], SeekForThirst);
                        return flag;
                    }
                }
                return false;
            }
        }
        public static void FlashCell(this Map map, IntVec3 cell)
        {
            if (DebugSettings.ShowDevGizmos && map != null && cell.InBounds(map))
            {
                map.debugDrawer.FlashCell(cell, 0.95f, null, 10);
            }
        }
        public static bool IsEven(this int num)
        {
            return (num & 1) == 0;
        }
        public static float GetConeDirection(this IntVec3 origin, IntVec3 destination)
        {
            return Mathf.Atan2(-(destination.z - origin.z), destination.x - origin.x) * 57.29578f;
        }
        public static int DirectionAsInt(this IntVec2 direction)
        {
            int result = 0;
            switch ((direction.x, direction.z))
            {
                case (0, 1):
                    result = IntNorth;
                    break;
                case (0, -1):
                    result = IntSouth;
                    break;
                case (1, 0):
                    result = IntEast;
                    break;
                case (-1, 0):
                    result = IntWest;
                    break;
                case (1, 1):
                    result = IntNorthEast;
                    break;
                case (1, -1):
                    result = IntSouthEast;
                    break;
                case (-1, 1):
                    result = IntNorthWest;
                    break;
                case (-1, -1):
                    result = IntSouthWest;
                    break;
                default:
                    break;
            }
            return result;
        }
        public static IntVec3 GetHeadlightsCell(VehiclePawn target, IntVec2 direction)
        {
            if (target.def.Size.Area == 1)
            {
                return target.PositionHeld;
            }
            int dir = direction.DirectionAsInt();
            CellRect rect = target.OccupiedRect();
            float centerX = (rect.minX + rect.maxX) * 0.5f;
            float centerZ = (rect.minZ + rect.maxZ) * 0.5f;
            float angle = target.FullRotation.AsInt switch
            {
                IntNorth => 0f,
                IntEast => 90f,
                IntSouth => 180f,
                IntWest => 270f,
                IntNorthEast => 45f,
                IntSouthEast => 135f,
                IntSouthWest => 225f,
                IntNorthWest => 315f,
                _ => 0f
            };
            float radians = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            // Preserve the original anchor cell selection.
            // Convert its displacement into vehicle-local coordinates.
            float dx = target.Position.x - centerX;
            float dz = target.Position.z - centerZ;
            float anchorLocalX = dx * cos - dz * sin;
            float anchorLocalZ = dx * sin + dz * cos;
            float localX = 0f;
            float localZ = 0f;
            // Distance to the outermost cell centers.
            float halfWidth = (target.def.Size.x - 1) * 0.5f;
            float halfLength = (target.def.Size.z - 1) * 0.5f;
            switch (dir)
            {
                case IntNorth:
                    localX = anchorLocalX;
                    localZ = halfLength;
                    break;
                case IntEast:
                    localX = -halfWidth;
                    localZ = anchorLocalZ;
                    break;
                case IntSouth:
                    localX = anchorLocalX;
                    localZ = -halfLength;
                    break;
                case IntWest:
                    localX = halfWidth;
                    localZ = anchorLocalZ;
                    break;
                default:
                    return target.Position;
            }
            // Rotate the local point into world coordinates.
            float worldX = centerX + localX * cos + localZ * sin;
            float worldZ = centerZ - localX * sin + localZ * cos;
            // Find the nearest occupied cell to the calculated midpoint.
            IntVec3 result = target.Position;
            float bestDistance = float.MaxValue;
            foreach (IntVec3 cell in rect.Cells)
            {
                float dxCell = cell.x - worldX;
                float dzCell = cell.z - worldZ;
                float distance = dxCell * dxCell + dzCell * dzCell;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    result = cell;
                }
            }
            // Post-process to fix 3x3/5x5 misalignment.
            IntVec2 size = target.def.Size;
            if (size.x != size.z && !size.x.IsEven())
            {
                switch (target.FullRotation.AsInt)
                {
                    case IntNorthWest:
                        result += IntVec3.East;
                        break;
                    case IntSouthWest:
                        result += IntVec3.East;
                        break;
                }
            }
            result.y = target.Position.y;
            return result;
        }
        public static void RegisterLights(CompLights comp, GlowGrid grid)
        {
            if (comp == null || grid == null)
            {
                return;
            }
            if (!lightsByGrid.TryGetValue(grid, out List<CompLights> list))
            {
                list = new List<CompLights>(4);
                lightsByGrid.Add(grid, list);
            }
            if (!list.Contains(comp))
            {
                list.Add(comp);
            }
        }
        public static void UnregisterLights(CompLights comp, GlowGrid grid)
        {
            if (comp == null || grid == null || !lightsByGrid.TryGetValue(grid, out List<CompLights> list))
            {
                return;
            }
            list.Remove(comp);
            if (list.Count == 0)
            {
                lightsByGrid.Remove(grid);
            }
        }
        public static bool TryGetLights(GlowGrid grid, out List<CompLights> lights)
        {
            if (grid != null && lightsByGrid.TryGetValue(grid, out lights))
            {
                return true;
            }
            lights = null;
            return false;
        }
        public static void ClearLights()
        {
            if (lightsByGrid.Count > 0)
            {
                foreach (KeyValuePair<GlowGrid, List<CompLights>> entry in lightsByGrid)
                {
                    List<CompLights> list = entry.Value;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null)
                        {
                            list[i].RegistryWasCleared(entry.Key);
                        }
                    }
                }
                lightsByGrid.Clear();
            }
        }
        public static void NotifyBlockerChanged(GlowGrid grid, IntVec3 cell)
        {
            if (!TryGetLights(grid, out List<CompLights> list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                CompLights comp = list[i];
                if (comp != null)
                {
                    comp.NotifyBlockerChanged(cell);
                }
            }
        }
    }
}