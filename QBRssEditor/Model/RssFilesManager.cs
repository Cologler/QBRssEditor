using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Newtonsoft.Json;

namespace QBRssEditor.Model
{
    class RssFilesManager
    {
        private readonly object _syncRoot = new object();
        private readonly DirectoryInfo _rootDir;
        private Task<List<RssItem>> _task;

        RssFilesManager(string rootDir)
        {
            this._rootDir = new DirectoryInfo(rootDir);
        }

        public Task<List<RssItem>> ListAsync()
        {
            lock (this._syncRoot)
            {
                if (this._task == null)
                {
                    this._task = Task.Run(this.List);
                }

                return this._task;
            }
        }

        private List<RssItem> List()
        {
            if (!this._rootDir.Exists)
            {
                return Enumerable.Empty<RssItem>().ToList();
            }

            return this._rootDir.GetFiles("*.json").SelectMany(z =>
            {
                using (var text = z.OpenText())
                {
                    return JsonConvert.DeserializeObject<List<RssItem>>(text.ReadToEnd());
                }
            }).ToList();
        }

        public static RssFilesManager Installed { get; }

        static RssFilesManager()
        {
            var path = Path.Combine(
                Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"),
                "qBittorrent",
                "rss",
                "articles");

            Installed = new RssFilesManager(path);
        }
    }
}
