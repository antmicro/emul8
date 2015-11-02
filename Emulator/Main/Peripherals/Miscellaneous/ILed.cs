//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals;

namespace Emul8.Peripherals.Miscellaneous
{
    public interface ILed : IPeripheral
    {
        bool State { get; }
        event Action<ILed, bool> StateChanged;
    }
}
