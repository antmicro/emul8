//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Bus;
using Emul8.Logging;
using Emul8.Core;

namespace Emul8.Peripherals.GPIOPort
{
    public class EFMGPIOPort : BaseGPIOPort, IDoubleWordPeripheral, IKnownSize
    {
        public EFMGPIOPort(Machine machine) : base(machine, 6*16)
        {

        }

        public long Size
        {
            get
            {
                return 0x140;
            }
        }

        public uint ReadDoubleWord(long offset)
        {
            this.LogUnhandledRead(offset);
            return 0;
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            var portNumber = (int)(offset / 0x24);
            if(portNumber <= 6)
            {
                offset %= 0x24;
                switch((Offset)offset)
                {
                case Offset.Set:
                    DoPinOperation(portNumber, Operation.Set, value);
                    break;
                case Offset.Clear:
                    DoPinOperation(portNumber, Operation.Clear, value);
                    break;
                case Offset.Toggle:
                    DoPinOperation(portNumber, Operation.Toggle, value);
                    break;
                default:
                    this.LogUnhandledWrite(offset, value);
                    break;
                }
            }
            else
            {
                this.LogUnhandledWrite(offset, value);
            }
        }

        private void DoPinOperation(int portNumber, Operation operation, uint value)
        {
            for(var i = 0; i < 15; i++)
            {
                var pinNumber = portNumber * 16 + i;
                if((value & 1) != 0)
                {
                    switch(operation)
                    {
                    case Operation.Set:
                        Connections[pinNumber].Set();
                        break;
                    case Operation.Clear:
                        Connections[pinNumber].Unset();
                        break;
                    case Operation.Toggle:
                        Connections[pinNumber].Toggle();
                        break;
                    }
                }
                value >>= 1;
            }
        }

        private enum Operation
        {
            Set,
            Clear,
            Toggle
        }

        private enum Offset : uint
        {
            Set = 0x10,
            Clear = 0x14,
            Toggle = 0x18
        }
    }
}

