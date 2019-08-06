using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Newtonsoft.Json;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    class OriginMarkReadService : IMarkReadService
    {
        private readonly IQBitStatusService _qBitStatus;
        private readonly JsonSerializerSettings _settings;
        private readonly FileWriteWaiterService _writer;

        public OriginMarkReadService(IQBitStatusService qBitStatus, JsonSerializerSettings settings, FileWriteWaiterService writer)
        {
            this._qBitStatus = qBitStatus;
            this._settings = settings;
            this._writer = writer;
        }

        public void Attach(IEnumerable<RssItem> items)
        {
            // do nothing
        }

        public async Task FlushAsync()
        {
            if (!this._qBitStatus.IsRunning)
            {
                
            }
        }

        public void MarkReaded(IEnumerable<RssItem> items)
        {
            foreach (var item in items)
            {
                item.IsRead = true;
            }
        }
    }
}
