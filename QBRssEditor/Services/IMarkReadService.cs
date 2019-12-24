using System.Collections.Generic;
using System.Threading.Tasks;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    interface IHideItemService
    {
        void Attach(IEnumerable<RssItem> items);

        void Hide(IEnumerable<ResourceItem> items);

        Task FlushAsync();
    }
}
