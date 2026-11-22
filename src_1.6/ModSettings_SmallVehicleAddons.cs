using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
    public class ModSettings_SmallVehicleAddons : ModSettings
    {
        public static readonly ModMetaData SmallVehicleAddonsMMD = ModLister.GetActiveModWithIdentifier("Inoshishi3.SmallVehicleAddons", ignorePostfix: true);
        public static ModSettings_SmallVehicleAddons Instance;
        public bool vehiclesMassAdjust = true;
        public bool addFastCargoGizmo = true;
        public bool addLightsGizmo = true;
        public bool addHonkGizmo = true;
        public bool addNeedsRefillment = true;
        public bool shouldLightSelf = false;
        private Listing_Standard ls = new Listing_Standard();
        public void DoWindowContents(Rect canvas)
        {
            Rect rect = canvas;
            ls.ColumnWidth = (rect.width - 20f) / 2f;
            ls.Begin(canvas);
            ls.Label(SmallVehicleAddonsMMD?.ModVersion ?? string.Empty);
            ls.GapLine(36f);
            Text.Font = GameFont.Small;
            Rect rect2 = ls.GetRect(30f);
            Widgets.CheckboxLabeled(rect2, "SmallVehicleAddonsSettingsMassAdjust".Translate(), ref vehiclesMassAdjust);
            TooltipHandler.TipRegion(rect2, "SmallVehicleAddonsSettingsMassAdjustDesc".Translate());
            Rect rect3 = ls.GetRect(30f);
            Widgets.CheckboxLabeled(rect3, "SmallVehicleAddonsSettingsAddFastCargoGizmo".Translate(), ref addFastCargoGizmo);
            TooltipHandler.TipRegion(rect3, "SmallVehicleAddonsSettingsAddFastCargoGizmoDesc".Translate());
            Rect rect4 = ls.GetRect(30f);
            Widgets.CheckboxLabeled(rect4, "SmallVehicleAddonsSettingsAddLightsGizmo".Translate(), ref addLightsGizmo);
            TooltipHandler.TipRegion(rect4, "SmallVehicleAddonsSettingsAddLightsGizmoDesc".Translate());
            Rect rect5 = ls.GetRect(30f);
            Widgets.CheckboxLabeled(rect5, "SmallVehicleAddonsSettingsAddHonkGizmo".Translate(), ref addHonkGizmo);
            TooltipHandler.TipRegion(rect5, "SmallVehicleAddonsSettingsAddHonkGizmoDesc".Translate());
            Rect rect7 = ls.GetRect(30f);
            Widgets.CheckboxLabeled(rect7, "SmallVehicleAddonsSettingsAddNeedsRefillment".Translate(), ref addNeedsRefillment);
            TooltipHandler.TipRegion(rect7, "SmallVehicleAddonsSettingsAddNeedsRefillmentDesc".Translate());
            //Rect rect6 = ls.GetRect(30f);
            //Widgets.CheckboxLabeled(rect6, "SmallVehicleAddonsSettingsShouldVehiclesLightSelf".Translate(), ref shouldLightSelf);
            //TooltipHandler.TipRegion(rect6, "SmallVehicleAddonsSettingsShouldVehiclesLightSelfDesc".Translate());
            ls.GapLine(36f);
            if (ls.ButtonText("Default"))
            {
                SetDefault();
            }
            ls.End();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref vehiclesMassAdjust, "vehiclesMassAdjust", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref addFastCargoGizmo, "addFastCargoGizmo", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref addLightsGizmo, "addLightsGizmo", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref addHonkGizmo, "addHonkGizmo", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref addNeedsRefillment, "addNeedsRefillment", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref shouldLightSelf, "shouldLightSelf", defaultValue: false, forceSave: true);
        }
        public void SetDefault()
        {
            vehiclesMassAdjust = true;
            addFastCargoGizmo = true;
            addLightsGizmo = true;
            addHonkGizmo = true;
            addNeedsRefillment = true;
            shouldLightSelf = false;
        }
    }
}