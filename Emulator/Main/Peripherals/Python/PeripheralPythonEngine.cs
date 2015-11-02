//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Microsoft.Scripting.Hosting;
using Emul8.Peripherals.Bus;
using Antmicro.Migrant.Hooks;
using Antmicro.Migrant;
using System.Linq;
using Emul8.Core;

namespace Emul8.Peripherals.Python
{
    public class PeripheralPythonEngine : PythonEngine
    {
        private readonly static string[] Imports =
        {
            "from Emul8.Logging import Logger as logger",
            "from Emul8.Logging import LogLevel",
        };

        protected override string[] ReservedVariables
        {
            get { return base.ReservedVariables.Union(PeripheralPythonEngine.InnerReservedVariables).ToArray(); }
        }

        private readonly static string[] InnerReservedVariables =
        {
            "request",
            "self",
            "size",
            "logger",
            "LogLevel",
            "machine"
        };

        private void InitScope(ScriptSource script)
        {
            Request = new PythonRequest();

            Scope.SetVariable("request", Request);
            Scope.SetVariable("self", peripheral);
            Scope.SetVariable("size", peripheral.Size);

            source = script;
        }

        public PeripheralPythonEngine(PythonPeripheral peripheral)
        {
            this.peripheral = peripheral;
            InitScope(Engine.CreateScriptSourceFromString(Aggregate(Imports)));
        }

        public PeripheralPythonEngine(PythonPeripheral peripheral, Func<ScriptEngine, ScriptSource> sourceGenerator)
        {
            this.peripheral = peripheral;
            InitScope(sourceGenerator(Engine));
        }

        public string Code
        {
            get
            {
                return source.GetCode();
            }
        }

        public void ExecuteCode()
        {
            source.Execute(Scope);
        }

        public void SetSysbusAndMachine(SystemBus bus)
        {
            Scope.SetVariable("sysbus", bus);
            Scope.SetVariable("machine", bus.Machine);
        }

        [Transient]
        private ScriptSource source;

        protected override void Init()
        {
            base.Init();
            InitScope(Engine.CreateScriptSourceFromString(codeContent));
            codeContent = null;
        }

        #region Serialization

        [PreSerialization]
        protected void BeforeSerialization()
        {
            codeContent = Code;
        }

        [PostSerialization]
        private void AfterDeSerialization()
        {
            codeContent = null;
        }

        private string codeContent;

        #endregion

        [Transient]
        private PythonRequest request;

        public PythonRequest Request
        {
            get
            {
                return request;
            }
            private set
            {
                request = value;
            }
        }

        private readonly PythonPeripheral peripheral;

        // naming convention here is pythonic
        public class PythonRequest
        {
            public uint value { get; set; }
            public byte length { get; set; }
            public RequestType type { get; set; }
            public long offset { get; set; }
            public long absolute { get; set; }

            public bool isInit
            {
                get
                {
                    return type == RequestType.INIT;
                }
            }

            public bool isRead
            {
                get
                {
                    return type == RequestType.READ;
                }
            }

            public bool isWrite
            {
                get
                {
                    return type == RequestType.WRITE;
                }
            }

            public enum RequestType
            {
                READ,
                WRITE,
                INIT
            }
        }
    }
}
