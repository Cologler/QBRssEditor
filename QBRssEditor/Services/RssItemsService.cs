using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    class RssItemsService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IHideItemService> _markReadServices;
        private readonly IQBitStatusService _qBitStatus;

        public RssItemsService(IServiceProvider serviceProvider, 
            IEnumerable<IHideItemService> markReadServices, IQBitStatusService qBitStatus)
        {
            this._serviceProvider = serviceProvider;
            this._markReadServices = markReadServices;
            this._qBitStatus = qBitStatus;
        }

        public Task<List<RssItem>> ListAsync() => Task.Run(() => this.GetLoadedDataContext().Items.ToList());

        public QBRssDataContext GetLoadedDataContext()
        {
            var context = this._serviceProvider.GetRequiredService<QBRssDataContext>();
            foreach (var markReadService in this._markReadServices)
            {
                markReadService.Attach(context.Items);
            }
            return context;
        }

        public void MarkReaded(IEnumerable<ResourceItem> items)
        {
            foreach (var markReadService in this._markReadServices)
            {
                markReadService.Hide(items);
            }
        }

        public async Task FlushAsync()
        {
            await this._qBitStatus.UpdateStatusAsync();
            await Task.WhenAll(this._markReadServices.Select(z => z.FlushAsync()));
        }
    }
}
