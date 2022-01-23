using System;
using Asgard.Communications;
using Asgard.Data;

namespace Asgard.EngineControl
{
    public class EngineSession
    {
        private readonly ICbusMessenger cbusMessenger;
        private byte speedDir;

        public ushort Address { get; }
        public byte Session { get; }
        public byte SpeedDir
        {
            get => speedDir; set
            {
                if (speedDir != value)
                {
                    speedDir = value;
                    cbusMessenger?.SendMessage(new SetEngineSpeedAndDirection
                    {
                        Session = Session,
                        SpeedDir = speedDir
                    });
                }
                
            }
        }

        public EngineSession(EngineReport report, ICbusMessenger cbusMessenger)
        {
            this.Address = report.Address;
            this.Session = report.Session;
            this.speedDir = report.SpeedDir;
            this.cbusMessenger = cbusMessenger;
        }

        public void SetFunction(byte functionNo, bool on) {
            if (on)
            {
                cbusMessenger?.SendMessage(new SetEngineFunctionOn { Session = Session, FunctionNumber = functionNo });
            }
            else
            {
                cbusMessenger?.SendMessage(new SetEngineFunctionOff { Session = Session, FunctionNumber = functionNo });
            }
        }
    }
}