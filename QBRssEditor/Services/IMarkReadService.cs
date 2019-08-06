using System.Collections.Generic;
using System.Threading.Tasks;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    interface IMarkReadService
    {
        void Attach(IEnumerable<RssItem> items);

        void MarkReaded(IEnumerable<RssItem> items);

        Task FlushAsync();
    }
}
