using System.Collections.Generic;
using System.Threading;
using QBRssEditor.LocalDb;

namespace QBRssEditor.Abstractions
{
    interface IResourceProvider
    {
        string Name { get; }

        IEnumerable<ResourceItem> GetNotExists(HashSet<string> existsKeys, CancellationToken cancellationToken);
    }
}
