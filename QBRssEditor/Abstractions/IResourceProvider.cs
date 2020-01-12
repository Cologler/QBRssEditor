using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QBRssEditor.LocalDb;

namespace QBRssEditor.Abstractions
{
    interface IResourceProvider
    {
        IEnumerable<ResourceItem> GetNotExists(HashSet<string> existsKeys, CancellationToken cancellationToken);
    }
}
