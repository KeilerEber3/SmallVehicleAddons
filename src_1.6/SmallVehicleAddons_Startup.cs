using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SmallVehicleAddons
{
    [StaticConstructorOnStartup]
    public static class SmallVehicleAddons_Startup
    {
        static SmallVehicleAddons_Startup()
        {
            SmallVehicleAddons_Utility.VehiclesMassAdjust();
            SmallVehicleAddons_Utility.AddFastCargoGizmo();
            SmallVehicleAddons_Utility.AddLightsGizmo();
            SmallVehicleAddons_Utility.AddHonkGizmo();
            SmallVehicleAddons_Utility.AddNeedsRefillment();
        }
    }
}