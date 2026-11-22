using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallVehicleAddons
{
    [DefOf]
    public static class SmallVehicleAddonsDefOf
    {
        public static SillyStringDef SillyString_MotorcycleDefs;
        public static SillyStringDef SillyString_CarDefs;
        public static SillyStringDef SillyString_TruckDefs;
        public static SillyStringDef Headlights_Whitelist;
        public static SillyStringDef Honk_Whitelist;
        static SmallVehicleAddonsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SmallVehicleAddonsDefOf));
        }
    }
}