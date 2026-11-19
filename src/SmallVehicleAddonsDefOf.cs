using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace SmallVehicleAddons
{
    [DefOf]
    public static class SmallVehicleAddonsDefOf
    {
        public static SillyStringDef SillyString_MotorcycleDefs;
        public static SillyStringDef SillyString_CarDefs;
        public static SillyStringDef SillyString_TruckDefs;
        static SmallVehicleAddonsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SmallVehicleAddonsDefOf));
        }
    }
}