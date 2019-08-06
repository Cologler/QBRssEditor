using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace QBRssEditor.Services
{
    class FileWriteWaiterService
    {
        private readonly List<WriteSession> _sessions = new List<WriteSession>();

        public IDisposable Opening()
        {
            return new WriteSession(this._sessions);
        }

        public void Write(Action action)
        {
            using (this.Opening())
            {
                action();
            }
        }

        public async Task WriteAsync(Task action)
        {
            using (this.Opening())
            {
                await action;
            }
        }

        public void WaitAll()
        {
            SpinWait.SpinUntil(() =>
            {
                lock (this._sessions)
                {
                    return this._sessions.Count == 0;
                }
            });
        }

        private class WriteSession : IDisposable
        {
            private readonly List<WriteSession> _sessions;

            public WriteSession(List<WriteSession> sessions)
            {
                this._sessions = sessions;
                lock (this._sessions)
                {
                    this._sessions.Add(this);
                }
            }

            public void Dispose()
            {
                lock (this._sessions)
                {
                    this._sessions.Remove(this);
                }
            }
        }
    }
}
