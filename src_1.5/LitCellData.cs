using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
    public struct LitCellData
    {
        public IntVec3 cell;
        public int index;
        public float groundGlow;
        public Color visualGlow;
        public LitCellData(IntVec3 cell, int index, float groundGlow, Color visualGlow)
        {
            this.cell = cell;
            this.index = index;
            this.groundGlow = groundGlow;
            this.visualGlow = visualGlow;
        }
    }
}