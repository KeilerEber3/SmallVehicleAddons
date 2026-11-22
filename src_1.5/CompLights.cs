using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace SmallVehicleAddons
{
    public class CompLights : VehicleComp
    {
        private static readonly IntVec3 NorthLightOffset = new IntVec3(0, 0, 1);
        private static readonly IntVec3 EastLightOffset = new IntVec3(1, 0, 0);
        private static readonly IntVec3 SouthLightOffset = new IntVec3(0, 0, -1);
        private static readonly IntVec3 WestLightOffset = new IntVec3(-1, 0, 0);
        private static readonly IntVec3 NorthEastLightOffset = new IntVec3(1, 0, 1);
        private static readonly IntVec3 SouthEastLightOffset = new IntVec3(1, 0, -1);
        private static readonly IntVec3 NorthWestLightOffset = new IntVec3(-1, 0, 1);
        private static readonly IntVec3 SouthWestLightOffset = new IntVec3(-1, 0, -1);
        public ColorInt? forcedColor = null;
        private bool lightsDeployed = true;
        public bool LightsDeployed => lightsDeployed;
        public CompFueledTravel CompFueledTravel => Vehicle.CompFueledTravel;
        public CompProperties_CompLights Props => props as CompProperties_CompLights;
        public bool HasOperator => SmallVehicleAddons_Utility.HasOperator(Vehicle);
        private readonly Dictionary<int, LitCellData> litCells = new Dictionary<int, LitCellData>(256);
        private Map CurrentMap => Vehicle?.Map;
        private GlowGrid CurrentGlowGrid => Vehicle?.Map?.glowGrid;
        private bool hasCachedConeState;
        private bool coneDirty = true;
        private LightsState cachedConeState;
        private bool hasCachedColor;
        private Color cachedColor;
        private bool hasCachedOperationalState;
        private bool cachedOperationalState;
        public bool Fueled
        {
            get
            {
                if (!Props.lightsNeedFuel)
                {
                    return true;
                }
                return CompFueledTravel != null && CompFueledTravel.FuelPercent > 0f;
            }
        }
        public bool LightIsOperational => lightsDeployed && Fueled;
        public IntVec3 LightsDirection
        {
            get
            {
                switch (Vehicle.FullRotation.AsInt)
                {
                    case 0:
                        return Vehicle.PositionHeld + NorthLightOffset * 8;
                    case 1:
                        switch (cachedConeState.rotation)
                        {
                            case 4:
                                return Vehicle.PositionHeld + NorthEastLightOffset * 7;
                            case 5:
                                return Vehicle.PositionHeld + SouthEastLightOffset * 7;
                            default:
                                break;
                        }
                        return Vehicle.PositionHeld + EastLightOffset * 8;
                    case 2:
                        return Vehicle.PositionHeld + SouthLightOffset * 8;
                    case 3:
                        switch (cachedConeState.rotation)
                        {
                            case 7:
                                return Vehicle.PositionHeld + NorthWestLightOffset * 7;
                            case 6:
                                return Vehicle.PositionHeld + SouthWestLightOffset * 7;
                            default:
                                break;
                        }
                        return Vehicle.PositionHeld + WestLightOffset * 8;
                    case 4:
                        return Vehicle.PositionHeld + NorthEastLightOffset * 7;
                    case 5:
                        return Vehicle.PositionHeld + SouthEastLightOffset * 7;
                    case 7:
                        return Vehicle.PositionHeld + NorthWestLightOffset * 7;
                    case 6:
                        return Vehicle.PositionHeld + SouthWestLightOffset * 7;
                    default:
                        return Vehicle.PositionHeld;
                }
            }
        }
        public Color LightColor
        {
            get
            {
                if (forcedColor.HasValue)
                {
                    return (Color)forcedColor.Value.ProjectToColor32;
                }
                return Props.color;
            }
        }
        public IntVec3 SourceCell
        {
            get
            {
                if (!Props.adjustSourceToHead)
                {
                    return Vehicle.PositionHeld;
                }
                return SmallVehicleAddons_Utility.GetHeadlightsCell(Vehicle, IntVec2.North);
            }
        }
        public float LightRange
        {
            get
            {
                return Mathf.Clamp(Props.lightsEffectiveDistance, 0.5f, 128f);
            }
        }
        public override void Initialize(CompProperties compProps)
        {
            base.Initialize(compProps);
            lightsDeployed = false;
            cachedColor = LightColor;
            hasCachedColor = true;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            hasCachedConeState = false;
            coneDirty = true;
            EnsureRegistered();
            RefreshConeIfNeeded();
        }
        public override void PostDeSpawn(Map map)
        {
            MarkCachedCellsDirty(map);
            UnregisterFromRegistry();
            litCells.Clear();
            hasCachedConeState = false;
            coneDirty = true;
            cachedConeState = default(LightsState);
            hasCachedColor = false;
            hasCachedOperationalState = false;
            base.PostDeSpawn(map);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref forcedColor, "forcedColor", defaultValue: null);
            Scribe_Values.Look(ref lightsDeployed, "lightsDeployed", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UnregisterFromRegistry();
                litCells.Clear();
                hasCachedConeState = false;
                coneDirty = true;
                hasCachedColor = false;
                hasCachedOperationalState = false;
                cachedConeState = default(LightsState);
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Vehicle == null || !Vehicle.Spawned || CurrentMap == null)
            {
                UnregisterAndClear();
                return;
            }
            EnsureRegistered();
            RefreshConeIfNeeded();
            RefreshCachedColorIfNeeded();
            RefreshOperationalStateIfNeeded();
            if (Props.lightsNeedFuel && lightsDeployed && Vehicle.IsHashIntervalTick(500))
            {
                EatFuel();
            }
        }
        /// <summary>
        /// Registers against the vehicle's current GlowGrid. The current map
        /// and grid are always fetched from Vehicle; the state snapshot's map
        /// is used only to detect a map transition.
        /// </summary>
        private void EnsureRegistered()
        {
            Map map = CurrentMap;
            if (map != null && CurrentGlowGrid != null)
            {
                if (hasCachedConeState && !ReferenceEquals(cachedConeState.map, map))
                {
                    MarkCachedCellsDirty(map);
                    SmallVehicleAddons_Utility.UnregisterLights(this, CurrentGlowGrid);
                    litCells.Clear();
                    hasCachedConeState = false;
                    coneDirty = true;
                    cachedConeState = default(LightsState);
                }
                SmallVehicleAddons_Utility.RegisterLights(this, CurrentGlowGrid);
            }
        }
        private void UnregisterFromRegistry()
        {
            SmallVehicleAddons_Utility.UnregisterLights(this, CurrentGlowGrid);
        }
        private void UnregisterAndClear()
        {
            MarkCachedCellsDirty(CurrentMap);
            UnregisterFromRegistry();
            litCells.Clear();
            hasCachedConeState = false;
            coneDirty = true;
            cachedConeState = default(LightsState);
            hasCachedColor = false;
            hasCachedOperationalState = false;
        }
        /// <summary>
        /// Checked every tick. The cone is rebuilt only when the compact state
        /// snapshot changes, or when NotifyBlockerChanged invalidates it.
        /// </summary>
        private void RefreshConeIfNeeded()
        {
            Map map = CurrentMap;
            if (map == null || CurrentGlowGrid == null || Vehicle == null || !Vehicle.Spawned)
            {
                return;
            }
            LightsState currentState = new LightsState(map, Vehicle.DrawPos, Vehicle.PositionHeld, Vehicle.FullRotation.AsInt);
            if (!coneDirty && hasCachedConeState && currentState == cachedConeState)
            {
                return;
            }
            IntVec3 sourceCell = SourceCell;
            RebuildCone(map, sourceCell);
            cachedConeState = currentState;
            hasCachedConeState = true;
            coneDirty = false;
        }
        private void RebuildCone(Map map, IntVec3 center)
        {
            MarkCachedCellsDirty(CurrentMap);
            litCells.Clear();
            if (map != null && center.InBounds(map))
            {
                float radius = LightRange;
                float halfAngle = Mathf.Clamp(Props.lightsAngle, 1f, 170f) * 0.5f;
                float direction = Vehicle.PositionHeld.GetConeDirection(LightsDirection);
                FloatRange coneAngle = new FloatRange(direction - halfAngle, direction + halfAngle);
                foreach (LitCellData data in GetCells_Cone(center, map, radius, coneAngle))
                {
                    litCells[data.index] = data;
                }
            }
            MarkCachedCellsDirty(map);
        }
        /// <summary>
        /// Scans the radial pattern and yields complete Harmony lookup records.
        /// Angle tolerance compensates for the square cell grid; edgeSoftness
        /// fades the intensity across the outer angular edge. A quadratic depth
        /// falloff also softens the end of the cone.
        /// </summary>
        private IEnumerable<LitCellData> GetCells_Cone(IntVec3 center, Map map, float radius, FloatRange coneAngle)
        {
            if (map == null || radius <= 0f)
            {
                yield break;
            }
            float coneCenter = (coneAngle.min + coneAngle.max) * 0.5f;
            float halfAngle = Mathf.Abs(coneAngle.max - coneAngle.min) * 0.5f;
            float safeRadius = Mathf.Max(radius, 0.001f);
            float softness = Mathf.Clamp01(Props.edgeSoftness);
            float intensity = Mathf.Max(0f, Props.intensity);
            Color tint = LightColor;
            float brightness = Mathf.Max(tint.r, Mathf.Max(tint.g, tint.b));
            int radialCount = Mathf.Min(GenRadial.NumCellsInRadius(radius), GenRadial.RadialPattern.Length);
            for (int i = 0; i < radialCount; i++)
            {
                IntVec3 cell = center + GenRadial.RadialPattern[i];
                if (!cell.InBounds(map))
                {
                    continue;
                }
                float distance = (cell - center).LengthHorizontal;
                if (distance <= 0.5f || distance > radius)
                {
                    continue;
                }
                if (Props.respectLOS && !GenSight.LineOfSight(center, cell, map, skipFirstCell: true))
                {
                    continue;
                }
                float cellDirection = center.GetConeDirection(cell);
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(coneCenter, cellDirection));
                float angularTolerance = 0.5f * (distance / safeRadius);
                float expandedHalfAngle = halfAngle + angularTolerance;
                if (angleDifference > expandedHalfAngle)
                {
                    continue;
                }
                float edgeStart = Mathf.Max(0f, halfAngle * (1f - softness));
                float edgeWidth = Mathf.Max(0.001f, expandedHalfAngle - edgeStart);
                float edgeT = Mathf.Clamp01((expandedHalfAngle - angleDifference) / edgeWidth);
                float edgeFade = Mathf.SmoothStep(0f, 1f, edgeT);
                float depthFade = Mathf.Clamp01(1f - distance / safeRadius);
                float depthFalloff = depthFade * depthFade;
                float cellIntensity = Mathf.Clamp01(intensity * depthFalloff * edgeFade);
                if (cellIntensity <= 0.0005f)
                {
                    continue;
                }
                int index = map.cellIndices.CellToIndex(cell);
                float groundGlow = cellIntensity * brightness;
                Color visualGlow = new Color(tint.r * cellIntensity, tint.g * cellIntensity, tint.b * cellIntensity, cellIntensity);
                yield return new LitCellData(cell, index, groundGlow, visualGlow);
            }
        }
        /// <summary>
        /// Updates color-derived values in-place. This does not rescan or
        /// recache the cone geometry.
        /// </summary>
        private void RefreshCachedColorIfNeeded()
        {
            Color color = LightColor;
            if (hasCachedColor && GenColor.IndistinguishableFrom(color, cachedColor))
            {
                return;
            }
            float brightness = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            if (litCells.Count > 0)
            {
                List<int> keys = new List<int>(litCells.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    int key = keys[i];
                    LitCellData data = litCells[key];
                    float cellIntensity = data.visualGlow.a;
                    data.groundGlow = cellIntensity * brightness;
                    data.visualGlow = new Color(color.r * cellIntensity, color.g * cellIntensity, color.b * cellIntensity, cellIntensity);
                    litCells[key] = data;
                }
                MarkCachedCellsDirty(CurrentMap);
            }
            cachedColor = color;
            hasCachedColor = true;
        }
        private void RefreshOperationalStateIfNeeded()
        {
            bool operational = LightIsOperational;
            if (hasCachedOperationalState && operational == cachedOperationalState)
            {
                return;
            }
            MarkCachedCellsDirty(CurrentMap);
            cachedOperationalState = operational;
            hasCachedOperationalState = true;
        }
        private void MarkCachedCellsDirty(Map map)
        {
            if (map == null || litCells.Count == 0)
            {
                return;
            }
            GlowGrid glowGrid = map.glowGrid;
            if (glowGrid == null)
            {
                return;
            }
            foreach (KeyValuePair<int, LitCellData> entry in litCells)
            {
                glowGrid.DirtyCache(entry.Value.cell);
                map.mapDrawer.MapMeshDirty(entry.Value.cell, MapMeshFlagDefOf.Roofs);
                map.mapDrawer.MapMeshDirty(entry.Value.cell, MapMeshFlagDefOf.GroundGlow);
                map.mapDrawer.MapMeshDirty(entry.Value.cell, MapMeshFlagDefOf.Things);
            }
        }
        public void RegistryWasCleared(GlowGrid oldGrid)
        {
            // This callback is issued only for components found in the grid's
            // registry list. Clear the runtime lookup and let CompTick register
            // against the current Vehicle.Map.glowGrid again.
            MarkCachedCellsDirty(CurrentMap);
            litCells.Clear();
            hasCachedConeState = false;
            coneDirty = true;
            cachedConeState = default(LightsState);
            hasCachedColor = false;
            hasCachedOperationalState = false;
        }
        public void NotifyBlockerChanged(IntVec3 cell)
        {
            if (!Props.respectLOS || Vehicle == null || !Vehicle.Spawned)
            {
                return;
            }
            Map map = CurrentMap;
            if (map == null || !cell.InBounds(map))
            {
                return;
            }
            IntVec3 sourceCell = SourceCell;
            float reach = LightRange + 1f;
            float dx = cell.x - sourceCell.x;
            float dz = cell.z - sourceCell.z;
            if (dx * dx + dz * dz > reach * reach)
            {
                return;
            }
            coneDirty = true;
        }
        public float GetCellGroundGlow(IntVec3 cell)
        {
            Map map = CurrentMap;
            if (!LightIsOperational || map == null || !cell.InBounds(map))
            {
                return 0f;
            }
            int index = map.cellIndices.CellToIndex(cell);
            return litCells.TryGetValue(index, out LitCellData data) ? data.groundGlow : 0f;
        }
        public bool TryGetCellVisualContribution(int cellIndex, out Color contribution)
        {
            if (LightIsOperational && litCells.TryGetValue(cellIndex, out LitCellData data))
            {
                contribution = data.visualGlow;
                return true;
            }
            contribution = Color.clear;
            return false;
        }
        public Vector2 GetSourceMapPosition()
        {
            if (Vehicle == null || !Vehicle.Spawned)
            {
                return Vector2.zero;
            }
            IntVec3 source = SourceCell;
            return new Vector2(source.x + 0.5f, source.z + 0.5f);
        }
        public Vector2 GetForwardDirection()
        {
            if (Vehicle == null)
            {
                return Vector2.right;
            }
            IntVec3 offset = LightsDirection - Vehicle.PositionHeld;
            Vector2 forward = new Vector2(offset.x, offset.z);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector2.right;
        }
        public bool TryGetCentralEndpoint(out Vector2 endpoint)
        {
            if (Vehicle == null || !Vehicle.Spawned || !LightIsOperational)
            {
                endpoint = Vector2.zero;
                return false;
            }
            Vector2 origin = GetSourceMapPosition();
            endpoint = origin + GetForwardDirection() * LightRange;
            return true;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Vehicle.Faction != Faction.OfPlayer)
            {
                yield break;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Flash Source Cell";
                command_Action.action = delegate
                {
                    Vehicle.Map.FlashCell(SourceCell);
                };
                yield return command_Action;
            }
            if (Props.doCopyPasteGizmos)
            {
                bool flag2 = true;
                float hue = 0f;
                float sat = 0f;
                if (CompGlower.ColorClipboard.HasValue)
                {
                    Color.RGBToHSV(CompGlower.ColorClipboard.Value.ProjectToColor32, out hue, out sat, out var _);
                    flag2 = false;
                }
                Command_ColorIcon command_ColorIcon3 = new Command_ColorIcon();
                command_ColorIcon3.icon = SmallVehicleAddons_Content.PasteColorTex;
                command_ColorIcon3.defaultLabel = "CommandPasteColorLabel".Translate();
                command_ColorIcon3.defaultDesc = "CommandPasteColorDesc".Translate();
                if (!flag2)
                {
                    command_ColorIcon3.color = Color.HSVToRGB(hue, sat, 1f);
                }
                command_ColorIcon3.Disabled = flag2;
                command_ColorIcon3.disabledReason = "ClipboardInvalidColor".Translate();
                command_ColorIcon3.action = delegate
                {
                    try
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        AssignColour(CompGlower.ColorClipboard.Value);
                        Messages.Message("ColorPastedSuccessfully".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
                    }
                    catch (Exception)
                    {
                        Messages.Message("ClipboardInvalidColor".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                };
                command_ColorIcon3.hotKey = KeyBindingDefOf.Misc5;
                yield return command_ColorIcon3;
            }
            if (HasOperator)
            {
                Command_Toggle command_Toggle = new Command_Toggle
                {
                    icon = (LightsDeployed ? SmallVehicleAddons_Content.LightsUndeployTex : SmallVehicleAddons_Content.LightsDeployTex),
                    defaultLabel = (LightsDeployed ? "SmallVehicleAddonsLightsUndeploy".Translate() : "SmallVehicleAddonsLightsDeploy".Translate()),
                    defaultDesc = "SmallVehicleAddonsLightsDeployDesc".Translate(),
                    turnOnSound = Props?.turnOnSoundDef,
                    turnOffSound = Props?.turnOffSoundDef,
                    hotKey = KeyBindingDefOf.Misc1,
                    toggleAction = delegate
                    {
                        ToggleDeployment();
                    },
                    isActive = () => LightsDeployed
                };
                yield return command_Toggle;
            }
        }
        public void ToggleDeployment()
        {
            lightsDeployed = !lightsDeployed;
            MarkCachedCellsDirty(CurrentMap);
            cachedOperationalState = LightIsOperational;
            hasCachedOperationalState = true;
            if (lightsDeployed)
            {
                coneDirty = true;
                RefreshConeIfNeeded();
                if (!Props.turnOnEvents.NullOrEmpty())
                {
                    foreach (VehicleEventDef vehicleEvent in Props.turnOnEvents)
                    {
                        SmallVehicleAddons_Utility.ExecuteVehicleEvent(Vehicle, vehicleEvent);
                    }
                }
            }
            else
            {
                if (!Props.turnOffEvents.NullOrEmpty())
                {
                    foreach (VehicleEventDef vehicleEvent in Props.turnOffEvents)
                    {
                        SmallVehicleAddons_Utility.ExecuteVehicleEvent(Vehicle, vehicleEvent);
                    }
                }
            }
        }
        public void EatFuel()
        {
            if (CompFueledTravel != null)
            {
                CompFueledTravel.ConsumeFuel(Props.lightsFuelSustainPrice);
            }
        }
        public void AssignColour(ColorInt? color = null)
        {
            forcedColor = color;
            hasCachedColor = false;
            RefreshCachedColorIfNeeded();
        }
        public void AssignColour(Color? color = null)
        {
            forcedColor = color.HasValue ? (ColorInt?)ColorInt.FromHdrColor(color.Value) : null;
            hasCachedColor = false;
            RefreshCachedColorIfNeeded();
        }
    }
}