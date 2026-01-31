using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    /// <summary>
    /// Struktura reprezentująca wartość nastawy PID
    /// </summary>
    public readonly struct LakeShorePidValue
    {
        public ushort ParamP { get; init; }
        public ushort ParamI { get; init; }
        public ushort ParamD { get; init; }

        public LakeShorePidValue(ushort paramP, ushort paramI, ushort paramD)
        {
            if (paramP > LakeShore.MaxPParameter || paramI > LakeShore.MaxIParameter || paramD > LakeShore.MaxDParameter)
            {
                Log.Error($"LakeShorePidValue-TriedToSetToBigValue:{{{paramP},{paramI},{paramD}}}");
                paramP = paramP <= LakeShore.MaxPParameter ? paramP : LakeShore.MaxPParameter;
                paramI = paramI <= LakeShore.MaxIParameter ? paramI : LakeShore.MaxIParameter;
                paramD = paramD <= LakeShore.MaxDParameter ? paramD : LakeShore.MaxDParameter;
            }
            ParamP = paramP;
            ParamI = paramI;
            ParamD = paramD;
        }

        public override string ToString() =>
            $"LumelPidValue{{{ParamP},{ParamI},{ParamD}}}";

        public override int GetHashCode()
        {
            return HashCode.Combine(ParamP, ParamI, ParamD);
        }
    }
}
