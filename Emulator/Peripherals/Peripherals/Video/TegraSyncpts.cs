//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Logging;

namespace Emul8.Peripherals.Video
{
    public class TegraSyncpts : IDoubleWordPeripheral, IKnownSize
    {
        public TegraSyncpts(Machine machine)
        {
  //          this.machine = machine;

//            sync = new object();

            sync_pts = new uint[23];
            for (int i = 0; i < sync_pts.Length; i++) sync_pts[i] = 0;
        }

        public long Size
        {
            get
            {
                return 0x4000;
            }
        }

        public void WriteDoubleWord(long address, uint value)
        {
            this.Log(LogLevel.Warning, "Write to unknown offset {0:X}, value {1:X}",address,value);
        }

        public uint ReadDoubleWord(long offset)
        {
            if ((offset >= 0x3400) && (offset <= 0x3458)) {
                       uint sync_id = (uint)((offset - 0x3400) / 4);
                       sync_pts[sync_id] += 1;
                       return sync_pts[sync_id];
            }
            switch (offset) {
               case 0x3040: // HOST1X_SYNC_SYNCPT_THRESH_CPU0_INT_STATUS_0
                       this.Log(LogLevel.Warning, "Read from CPU0_INT_STATUS");
                       return (1<<22) | (1<<13);
               default:
                       this.Log(LogLevel.Warning, "Read from unknown offset {0:X}, returning 0",offset);
                       break;
            }
            return 0;
        }

        public void Reset() {
        }

        uint[] sync_pts;

//        private object sync;

//        private readonly Machine machine;
    }
}

