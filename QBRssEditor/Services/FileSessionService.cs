using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace QBRssEditor.Services
{
    class FileSessionService : IDisposable
    {
        private readonly object _syncRoot = new object();
        private bool _canAdd = true;
        private readonly List<Session> _sessions = new List<Session>();

        public Session OpenSession(string path) => new Session(this);

        public void Dispose()
        {
            lock (this._syncRoot)
            {
                this._canAdd = false;
            }

            SpinWait.SpinUntil(() =>
            {
                lock (this._syncRoot)
                {
                    return this._sessions.Count == 0;
                }
            });
        }

        public class Session : IDisposable
        {
            private readonly FileSessionService _service;

            public Session(FileSessionService service)
            {
                this._service = service;
                lock (service._syncRoot)
                {
                    this.IsOpened = service._canAdd;
                    if (this.IsOpened) service._sessions.Add(this);
                }
            }

            public bool IsOpened { get; }

            public void Dispose()
            {
                lock (this._service._syncRoot)
                {
                    if (this.IsOpened) this._service._sessions.Remove(this);
                }
            }
        }
    }
}
