using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
    public struct LightsState : IEquatable<LightsState>
    {
        public readonly Map map;
        public readonly Vector3 position;
        public readonly IntVec3 cell;
        public readonly int rotation;
        public LightsState(Map map, Vector3 position, IntVec3 cell, int rotation)
        {
            this.map = map;
            this.position = position;
            this.cell = cell;
            this.rotation = rotation;
        }
        public bool Equals(LightsState other)
        {
            return ReferenceEquals(map, other.map) &&
                cell == other.cell &&
                rotation == other.rotation &&
                (position - other.position).sqrMagnitude <= 0.000001f;
        }
        public override int GetHashCode()
        {
            int hash = map != null ? map.GetHashCode() : 0;
            hash = (hash * 397) ^ cell.GetHashCode();
            hash = (hash * 397) ^ rotation;
            return hash;
        }
        public override bool Equals(object obj)
        {
            return obj is LightsState other && Equals(other);
        }
        public static bool operator ==(LightsState left, LightsState right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(LightsState left, LightsState right)
        {
            return !left.Equals(right);
        }
    }
}