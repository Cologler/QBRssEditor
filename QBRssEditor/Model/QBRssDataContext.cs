using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.Services;

namespace QBRssEditor.Model
{
    class QBRssDbSet : IEnumerable<RssItem>
    {
        private readonly Dictionary<string, List<RssItem>> _states = new Dictionary<string, List<RssItem>>();
        private readonly JsonService _jsonService;

        public QBRssDbSet(JsonService jsonService)
        {
            this._jsonService = jsonService;
        }

        public IEnumerator<RssItem> GetEnumerator() => this._states.SelectMany(z => z.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void LoadFile(string file) => 
            this._states[file] = this._jsonService.Load<List<RssItem>>(file);

        public void SaveFile()
        {
            foreach (var item in this._states)
            {
                this._jsonService.Dump(item.Key, item.Value);
            }            
        }
    }

    class QBRssDataContext
    {
        static readonly string QBRssRootPath = Path.Combine(
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"),
            "qBittorrent",
            "rss",
            "articles");

        public QBRssDataContext(IServiceProvider serviceProvider)
        {
            this.Items = new QBRssDbSet(serviceProvider.GetRequiredService<JsonService>());
        }

        public QBRssDbSet Items { get; }

        public void Load()
        {
            if (Directory.Exists(QBRssRootPath))
            {
                foreach (var path in Directory.GetFiles(QBRssRootPath, "*.json"))
                {
                    this.Items.LoadFile(path);
                }
            }
        }

        public void SaveChanges() => this.Items.SaveFile();
    }
}
