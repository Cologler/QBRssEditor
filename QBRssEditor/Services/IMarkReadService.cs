using System.Collections.Generic;
using QBRssEditor.LocalDb;

namespace QBRssEditor.Services
{
    interface IHideItemService
    {
        void Hide(IEnumerable<ResourceItem> items);
    }
}
