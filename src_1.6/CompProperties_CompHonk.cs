using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace SmallVehicleAddons
{
	public class CompProperties_CompHonk : VehicleCompProperties
	{
		public SoundDef soundDef;
		public float pawnsScanRadius = 6.9f;
        public CompProperties_CompHonk()
		{
			compClass = typeof(CompHonk);
		}
	}
}