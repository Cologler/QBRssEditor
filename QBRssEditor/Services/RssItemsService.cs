using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    class RssItemsService
    {
        private readonly IEnumerable<IMarkReadService> _markReadServices;
        private readonly IQBitStatusService _qBitStatus;

        public RssItemsService(IEnumerable<IMarkReadService> markReadServices, IQBitStatusService qBitStatus)
        {
            this._markReadServices = markReadServices;
            this._qBitStatus = qBitStatus;
        }

        public async Task<List<RssItem>> ListAsync()
        {
            var items = await RssFilesManager.Installed.ListAsync();
            foreach (var markReadService in this._markReadServices)
            {
                markReadService.Attach(items);
            }
            return items;
        }

        public void MarkReaded(IEnumerable<RssItem> items)
        {
            foreach (var markReadService in this._markReadServices)
            {
                markReadService.MarkReaded(items);
            }
        }

        public async Task FlushAsync()
        {
            await this._qBitStatus.UpdateStatusAsync();
            await Task.WhenAll(this._markReadServices.Select(z => z.FlushAsync()));
        }
    }
}
