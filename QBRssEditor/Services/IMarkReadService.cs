using System.Collections.Generic;
using System.Threading.Tasks;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    interface IHideItemService
    {
        void Hide(IEnumerable<ResourceItem> items);
    }
}
