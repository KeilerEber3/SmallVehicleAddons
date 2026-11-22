using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
    public class Command_FastCargoAction : Command_Action
    {
        public Action actionLMB;
        public Action actionRMB;
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SmallVehicleAddons_Utility.ActionOnLMB(ev, actionLMB);
            SmallVehicleAddons_Utility.ActionOnRMB(ev, actionRMB);
        }
    }
}