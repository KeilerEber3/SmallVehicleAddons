using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vehicles;
using Verse;

namespace SmallVehicleAddons
{
	public class CompProperties_CompLights : VehicleCompProperties
	{
        public bool farLightAllowed;

        public float intensity = 1.0f;
        public Color color = Color.white;
        public bool doCopyPasteGizmos = true;
        public bool respectLOS = true;
		public bool adjustSourceToHead = true;
        public float edgeSoftness = 0.12f;
        public float lightsAngle = 78f;
		public float lightsEffectiveDistance = 16f;

        public bool lightsNeedFuel;
        public float lightsFuelSustainPrice;
		public SoundDef turnOnSoundDef;
		public SoundDef turnOffSoundDef;
		public ThingDef selfLightThingDef;
		public ThingDef nearbyLightThingDef;
		public List<VehicleEventDef> turnOnEvents;
        public List<VehicleEventDef> turnOffEvents;
        public CompProperties_CompLights()
		{
			compClass = typeof(CompLights);
		}
	}
}