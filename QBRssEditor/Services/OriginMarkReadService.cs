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
using QBRssEditor.LocalDb;

namespace QBRssEditor.Services
{
    class OriginMarkReadService : IHideItemService
    {
        private readonly IQBitStatusService _qBitStatus;
        private readonly JsonService _jsonService;
        private readonly IServiceProvider _serviceProvider;

        public OriginMarkReadService(IQBitStatusService qBitStatus, JsonService jsonService,
            IServiceProvider serviceProvider)
        {
            this._qBitStatus = qBitStatus;
            this._jsonService = jsonService;
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
                await Task.Run(async () =>
                {
                    var context = this._serviceProvider.GetRequiredService<RssItemsService>().GetLoadedDataContext();
                    context.SaveChanges();
                    var journal = this._serviceProvider.GetRequiredService<JournalService>();
                    journal.Clear();
                    await journal.SaveAsync();
                });
            }
        }

        public void Hide(IEnumerable<ResourceItem> _)
        {
        }
    }
}
