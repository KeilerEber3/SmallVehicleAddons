using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.Sound;

namespace SmallVehicleAddons
{
	public class CompFastCargo : VehicleComp
    {
        public CargoType currentCargoType;
        public enum CargoType : byte
        {
            Default,
            WithChunks,
        }
        private static List<Thing> tmpThings = new List<Thing>();
        public CompProperties_CompFastCargo Props => props as CompProperties_CompFastCargo;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentCargoType, "currentCargoType");
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Vehicle.Spawned && Vehicle.inventory != null)
            {
                Command_FastCargoAction command_Action = new Command_FastCargoAction
                {
                    defaultLabel = "SmallVehicleAddonsLoadFastCargo".Translate(),
                    defaultDesc = "SmallVehicleAddonsLoadFastCargoDesc".Translate(),
                    icon = currentCargoType == CargoType.Default ? SmallVehicleAddons_Content.FastCargoTex : SmallVehicleAddons_Content.FastCargoWithChunksTex,
                    hotKey = KeyBindingDefOf.Designator_Cancel,
                    activateSound = Props?.activateSound,
                    onHover = delegate
                    {
                        GenDraw.DrawRadiusRing(Vehicle.Position, Props.thingsScanRadius);
                    },
                    actionLMB = delegate
                    {
                        AddItemsToTheVehicleTransferables();
                    },
                    actionRMB = delegate
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        FloatMenuOption floatMenuOption1 = new FloatMenuOption("SmallVehicleAddonsFastCargoMode".Translate("SmallVehicleAddonsFastCargoModeEnum0".Translate()), delegate
                        {
                            SwitchCargoMode(CargoType.Default);
                        });
                        list.Add(floatMenuOption1);
                        FloatMenuOption floatMenuOption2 = new FloatMenuOption("SmallVehicleAddonsFastCargoMode".Translate("SmallVehicleAddonsFastCargoModeEnum1".Translate()), delegate
                        {
                            SwitchCargoMode(CargoType.WithChunks);
                        });
                        list.Add(floatMenuOption2);
                        Find.WindowStack.Add(new FloatMenu(list));
                    },
                    action = delegate
                    {
                    },
                };
                yield return command_Action;
            }
        }
        public void SwitchCargoMode(CargoType currentCargoType)
        {
            this.currentCargoType = currentCargoType;
        }
        public void GetThingsNearBy()
        {
            tmpThings.Clear();
            tmpThings.AddRange(SmallVehicleAddons_Utility.GetNearbyThings(Vehicle.Map, Vehicle.Position, Props.thingsScanRadius, (Thing x) => CanThingBeFastCargo(x)));
        }
        public bool CanThingBeFastCargo(Thing t)
        {
            if (t?.def?.category != ThingCategory.Item)
            {
                return false;
            }
            if (t is Pawn)
            {
                return false;
            }
            if (currentCargoType == CargoType.Default && t.def.IsWithinCategory(ThingCategoryDefOf.Chunks))
            {
                return false;
            }
            return !t.IsForbidden(Faction.OfPlayer) && !tmpThings.Contains(t);
        }
        public void AddItemsToTheVehicleTransferables()
        {
            try
            {
                if (Vehicle.cargoToLoad != null && !Vehicle.cargoToLoad.Empty())
                {
                    MapComponentCache.GetCachedMapComponent<VehicleReservationManager>(Vehicle?.Map)?.RemoveLister(Vehicle, "LoadVehicle");
                    Vehicle.cargoToLoad.Clear();
                }
                SoundDefOf.Click.PlayOneShotOnCamera();
                GetThingsNearBy();
                if (!tmpThings.NullOrEmpty())
                {
                    AddItemsToTransferables(tmpThings, true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(SmallVehicleAddons_Utility.ExceptionMessage("AddItemsToTheVehicleTransferables", ex.Message + "\n" + ex.StackTrace));
            }
            tmpThings.Clear();
        }
        private void AddItemsToTransferables(List<Thing> things, bool setToTransferMax = false)
        {
            List<Thing> itemList = things;
            for (int i = 0; i < itemList.Count; i++)
            {
                AddToTransferables(itemList[i], setToTransferMax);
            }
        }
        private void AddToTransferables(Thing t, bool setToTransferMax = false)
        {
            List<TransferableOneWay> transferables = Vehicle.cargoToLoad;
            TransferableOneWay transferableOneWay = new TransferableOneWay();
            transferableOneWay.things.Add(t);
            if (setToTransferMax)
            {
                transferableOneWay.AdjustTo(transferableOneWay.CountToTransfer + t.stackCount);
            }
            else
            {
                transferableOneWay.AdjustTo(1);
            }
            if (transferables == null || transferables.Contains(transferableOneWay))
            {
                return;
            }
            Vehicle?.cargoToLoad?.Add(transferableOneWay);
            MapComponentCache.GetCachedMapComponent<VehicleReservationManager>(Vehicle?.Map)?.RegisterLister(Vehicle, "LoadVehicle");
        }
    }
}