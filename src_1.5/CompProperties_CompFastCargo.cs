using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace SmallVehicleAddons
{
    public class CompProperties_CompFastCargo : VehicleCompProperties
    {
        public SoundDef activateSound;
        public float thingsScanRadius;
        public CompProperties_CompFastCargo()
        {
            compClass = typeof(CompFastCargo);
        }
    }
}