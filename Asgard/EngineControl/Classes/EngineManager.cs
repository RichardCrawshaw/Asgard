using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;

namespace Asgard.EngineControl
{
    public class EngineManager:IDisposable
    {
        private readonly ICbusMessenger cbusMessenger;
        private readonly MessageManager messageManager;
        private readonly ResponseManager responseManager;
        private readonly ConcurrentDictionary<int, EngineSession> sessions;
        private readonly Timer sessionRefreshTimer;
        private bool disposedValue;

        

        public EngineManager(ICbusMessenger cbusMessenger)
        {
            sessions = new ConcurrentDictionary<int, EngineSession>();
            sessionRefreshTimer = new Timer(OnTimer, null, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4));
            this.cbusMessenger = cbusMessenger;
            this.messageManager = new MessageManager(cbusMessenger);
            this.responseManager = new ResponseManager(cbusMessenger);
            this.responseManager.Register<CommandStationErrorReport>(OnSessionCancelled, er => er.DccErrorCode == DccErrorCodeEnum.SessionCancelled);
        }

        private Task OnSessionCancelled(ICbusMessenger messenger, ICbusMessage message, CommandStationErrorReport sessionCancelled)
        {
            //Data1 contains the command station session
            if (sessions.TryRemove(sessionCancelled.Data1, out var session))
            {
                session.NotifyCancelled();
            }
            return Task.CompletedTask;
        }

        private async void OnTimer(object? state)
        {
            //TODO: proper exception handling and logging to prevent exceptions leaving async void event handler
            foreach(var session in sessions.Values)
            {
                await cbusMessenger.SendMessage(new SessionKeepAlive { Session = session.Session });
            }
        }

        public async Task<IEngineSession> RequestEngineSession(ushort locoDccAddress, bool share = false, bool steal = false)
        {
            if (share && steal)
            {
                throw new ArgumentException("steal and share cannot be set to true at the same time", nameof(steal));
            }
            var flags = SessionFlagsEnum.Request;
            if (share)
            {
                flags = SessionFlagsEnum.Share;
            }
            else if (steal)
            {
                flags = SessionFlagsEnum.Steal;
            }
            var msg = await messageManager.SendMessageWaitForReply(new GetEngineSession
            {
                Address = locoDccAddress,
                SessionFlags = flags
            });

            switch (msg)
            {
                case EngineReport report:
                    var es = new EngineSession(report, cbusMessenger);
                    sessions.TryAdd(locoDccAddress, es);
                    return es;
                case CommandStationErrorReport error:
                    throw new Exception("TODO: create better exception");
                default:
                    throw new Exception("TODO: create unexpected message exception");
            }
        }

        public async Task ReleaseEngineSession(IEngineSession session)
        {
            await cbusMessenger.SendMessage(new ReleaseEngine { Session = session.Session });
            if (sessions.TryRemove(session.Session, out _))
            {
                (session as EngineSession)?.NotifyCancelled();
            }
        }

        public async Task<SessionStatusEnum> ServiceModeWrite(IEngineSession session, ushort Cv, ServiceModeEnum serviceMode, byte value)
        {
            var status = await this.messageManager.SendMessageWaitForReply<ServiceModeStatus>(new WriteCvInServiceMode { 
                Session = session.Session,
                CV = Cv,
                ServiceMode = serviceMode,
                Value = value
            });
            return status.SessionStatus;
        }

        public async Task<byte> ServiceModeRead(IEngineSession session, ushort Cv, ServiceModeEnum serviceMode)
        {
            var reply = await this.messageManager.SendMessageWaitForReply(new ReadCv
            {
                Session = session.Session,
                ServiceMode = serviceMode,
                CV = Cv
            });

            return reply switch
            {
                ReportCv report => report.Value,
                ServiceModeStatus status => throw new ServiceModeReadException(status.SessionStatus),
                _ => throw new Exception($"Unknown reply reading CV: {reply}")
            };

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    responseManager.Deregister<CommandStationErrorReport>(OnSessionCancelled);
                    sessionRefreshTimer.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
