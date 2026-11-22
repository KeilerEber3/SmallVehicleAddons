using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
    [StaticConstructorOnStartup]
    public static class SmallVehicleAddons_Content
    {
        public static readonly Texture2D FastCargoTex = ContentFinder<Texture2D>.Get("SmallVehicleAddons/UI/FastCargo/Texture");
        public static readonly Texture2D FastCargoWithChunksTex = ContentFinder<Texture2D>.Get("SmallVehicleAddons/UI/FastCargoWithChunks/Texture");
        public static readonly Texture2D HonkTex = ContentFinder<Texture2D>.Get("SmallVehicleAddons/UI/Honk/Texture");
        public static readonly Texture2D LightsUndeployTex = ContentFinder<Texture2D>.Get("SmallVehicleAddons/UI/LightsUndeploy/Texture");
        public static readonly Texture2D LightsDeployTex = ContentFinder<Texture2D>.Get("SmallVehicleAddons/UI/LightsDeploy/Texture");
        public static readonly Texture2D PasteColorTex = ContentFinder<Texture2D>.Get("UI/Commands/PasteColor");
    }
}