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

        private readonly ConcurrentDictionary<int, EngineSession> sessions;
        private readonly Timer sessionRefreshTimer;
        private bool disposedValue;

        

        public EngineManager(ICbusMessenger cbusMessenger)
        {
            sessions = new ConcurrentDictionary<int, EngineSession>();
            sessionRefreshTimer = new Timer(OnTimer, null, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4));
            this.cbusMessenger = cbusMessenger;
            this.messageManager = new MessageManager(cbusMessenger);
            this.cbusMessenger.MessageReceived += OnCbusMessage;
        }

        private void OnCbusMessage(object sender, CbusMessageEventArgs e) {
            if (e.Message.GetOpCode() is CommandStationErrorReport errorReport)
            {
                if (errorReport.DccErrorCode == DccErrorCodeEnum.SessionCancelled)
                {
                    //Data1 contains the command station session
                    if (sessions.TryRemove(errorReport.Data1, out var session))
                    {
                        session.NotifyCancelled();
                    }
                }
            }
        }

        private async void OnTimer(object state) {
            //TODO: proper exception handling and logging to prevent exceptions leaving async void event handler
            foreach(var session in sessions.Values)
            {
                await cbusMessenger.SendMessage(new SessionKeepAlive { Session = session.Session });
            }
        }

        public async Task<IEngineSession> RequestEngineSession(ushort locoDccAddress)
        {
            if (sessions.TryGetValue(locoDccAddress, out var session))
            {
                return session;
            }
            else
            {
                var msg = await messageManager.SendMessageWaitForReply(new RequestEngineSession
                {
                    Address = locoDccAddress,
                });

                switch (msg) {
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
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cbusMessenger.MessageReceived -= OnCbusMessage;
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
