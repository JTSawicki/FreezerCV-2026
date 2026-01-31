using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabServices.GpibHardware
{
    public readonly struct GpibHardwareInitData
    {
        public readonly int KithleyAddress { get; init; }
        public readonly int LakeShoreAddress { get; init; }

        public readonly bool KithleyConnected { get; init; }
        public readonly bool LakeShoreConnected { get; init; }

        public GpibHardwareInitData(int kithleyAddress, int lakeShoreAddress, bool kithleyConnected, bool lakeShoreConnected)
        {
            KithleyAddress = kithleyAddress;
            LakeShoreAddress = lakeShoreAddress;
            KithleyConnected = kithleyConnected;
            LakeShoreConnected = lakeShoreConnected;
        }
    }
}
