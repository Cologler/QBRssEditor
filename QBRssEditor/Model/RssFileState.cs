using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Documents;

namespace QBRssEditor.Model
{
    class RssFileState
    {
        public RssFileState(FileInfo file, List<RssItem> items)
        {
            this.File = file;
            this.Items = items;
        }

        public FileInfo File { get; }

        public List<RssItem> Items { get; }
    }
}
