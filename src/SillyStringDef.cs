using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SmallVehicleAddons
{
    public class SillyStringDef : Def
    {
        [NoTranslate]
        public List<string> stringList = new List<string>();
        public static SillyStringDef Named(string defName)
        {
            return DefDatabase<SillyStringDef>.GetNamed(defName);
        }
	}
}