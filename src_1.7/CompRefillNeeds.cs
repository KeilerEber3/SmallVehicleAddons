using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace SmallVehicleAddons
{
	public class CompRefillNeeds : VehicleComp
	{
		public CompProperties_CompRefillNeeds Props => props as CompProperties_CompRefillNeeds;
		public override void CompTick()
		{
			base.CompTick();
			if (!parent.Spawned)
			{
				return;
            }
			if (Vehicle.IsHashIntervalTick(2500))
			{
                SmallVehicleAddons_Utility.TrySatisfyPassengersNeeds(Vehicle, Props.operatorKey);
			}
		}
    }
}