using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;

namespace Asgard.Control
{
    public class EngineManager:IDisposable
    {
        private readonly ICbusMessenger cbusMessenger;
        private readonly MessageManager messageManager;

        private readonly ConcurrentDictionary<int, EngineSession> sessions;
        private Timer sessionRefreshTimer;
        private bool disposedValue;

        

        public EngineManager(ICbusMessenger cbusMessenger)
        {
            sessions = new ConcurrentDictionary<int, EngineSession>();
            sessionRefreshTimer = new Timer(RefreshSessions, null, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4));
            this.cbusMessenger = cbusMessenger;
            this.messageManager = new MessageManager(cbusMessenger);

            //TODO: hook into cbusmessenger to watch for DCC error events
        }

        private async void RefreshSessions(object state) {
            //TODO: proper exception handling and logging to prevent exceptions leaving async void event handler
            var c = sessions.Values;
            foreach(var s in c)
            {
                await cbusMessenger.SendMessage(new SessionKeepAlive { Session = s.Session });
            }
        }

        public async Task<EngineSession> RequestEngineSession(ushort locoDccAddress)
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
