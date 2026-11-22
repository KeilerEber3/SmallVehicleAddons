using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
	public class Mod_SmallVehicleAddons : Mod
	{
		public static Mod_SmallVehicleAddons Instance;
		public Mod_SmallVehicleAddons(ModContentPack content)
			: base(content)
		{
			Instance = this;
			ModSettings_SmallVehicleAddons.Instance = GetSettings<ModSettings_SmallVehicleAddons>();
		}
		public override string SettingsCategory()
		{
			return "SmallVehicleAddons".Translate();
		}
		public override void DoSettingsWindowContents(Rect canvas)
		{
			ModSettings_SmallVehicleAddons.Instance.DoWindowContents(canvas);
		}
	}
}