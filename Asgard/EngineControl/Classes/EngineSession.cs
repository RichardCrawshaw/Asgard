using System;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;

namespace Asgard.EngineControl
{
    internal class EngineSession :
        IEngineSession
    {
        private readonly ICbusMessenger cbusMessenger;

        public ushort Address { get; }
        public byte Session { get; }
        public byte SpeedDir { get; private set; }

        public bool IsAvailable { get; private set; }

        public event EventHandler? SessionCancelled;

        public EngineSession(EngineReport report, ICbusMessenger cbusMessenger)
        {
            this.Address = report.Address;
            this.Session = report.Session;
            this.SpeedDir = report.SpeedDir;
            this.cbusMessenger = cbusMessenger;
            this.IsAvailable = true;
        }

        public async Task SetFunction(byte functionNo, bool on)
        {
            if (this.IsAvailable)
            {
                if (on)
                {
                    await cbusMessenger.SendMessage(
                        new SetEngineFunctionOn 
                        { 
                            Session = this.Session, 
                            FunctionNumber = functionNo,
                        });
                }
                else
                {
                    await cbusMessenger.SendMessage(
                        new SetEngineFunctionOff 
                        { 
                            Session = this.Session, 
                            FunctionNumber = functionNo,
                        });
                }
            }
        }

        public async Task SetSpeedAndDirection(byte speedDir)
        {
            if (this.SpeedDir != speedDir)
            {
                this.SpeedDir = speedDir;
                await cbusMessenger.SendMessage(
                    new SetEngineSpeedAndDirection
                    {
                        Session = this.Session,
                        SpeedDir = this.SpeedDir,
                    });
            }
        }

        public async Task SetCv(ushort cv, byte value)
        {
            await cbusMessenger.SendMessage(
                new WriteCvByteInOpsMode { Session = this.Session, CV = cv, Value = value });
        }

        internal void NotifyCancelled()
        {
            this.IsAvailable = false;
            SessionCancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}