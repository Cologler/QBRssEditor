using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Newtonsoft.Json;
using QBRssEditor.Model;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace QBRssEditor.Services
{
    class OriginMarkReadService : IMarkReadService
    {
        private readonly IQBitStatusService _qBitStatus;
        private readonly JsonSerializerSettings _settings;
        private readonly FileWriteWaiterService _waiter;
        private readonly IServiceProvider _serviceProvider;

        public OriginMarkReadService(IQBitStatusService qBitStatus, JsonSerializerSettings settings, FileWriteWaiterService waiter,
            IServiceProvider serviceProvider)
        {
            this._qBitStatus = qBitStatus;
            this._settings = settings;
            this._waiter = waiter;
            this._serviceProvider = serviceProvider;
        }

        public void Attach(IEnumerable<RssItem> items)
        {
            // do nothing
        }

        public async Task FlushAsync()
        {
            if (!this._qBitStatus.IsRunning)
            {
                var states = await this._serviceProvider.GetRequiredService<RssItemsService>()
                    .ListAsync();
                foreach (var state in states)
                {
                    await this._waiter.WriteAsync(Task.Run(() =>
                    {
                        File.WriteAllText(
                            state.File.FullName,
                            JsonConvert.SerializeObject(state.Items, this._settings),
                            Encoding.UTF8);
                    }));
                }
                var journal = this._serviceProvider.GetRequiredService<JournalService>();
                journal.Clear();
                await journal.SaveAsync();
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
