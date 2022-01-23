using System;
using System.Threading.Tasks;

namespace Asgard.EngineControl
{
    public interface IEngineSession
    {
        ushort Address { get; }
        byte Session { get; }
        byte SpeedDir { get; }

        event EventHandler SessionCancelled;

        Task SetFunction(byte functionNo, bool on);
        Task SetSpeedAndDirection(byte speedDir);
    }
}