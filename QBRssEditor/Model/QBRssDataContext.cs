using System;
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
    class QBRssDataContext
    {
        static readonly string QBRssRootPath = Path.Combine(
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"),
            "qBittorrent",
            "rss",
            "articles");
        private readonly JsonService _jsonService;
        private readonly List<QBRssDbSet> _items = new List<QBRssDbSet>();

        public QBRssDataContext(IServiceProvider serviceProvider)
        {
            this._jsonService = serviceProvider.GetRequiredService<JsonService>();
            this.Load();
        }

        public IEnumerable<RssItem> Items => this._items.SelectMany(z => z);

        void Load()
        {
            if (Directory.Exists(QBRssRootPath))
            {
                foreach (var path in Directory.GetFiles(QBRssRootPath, "*.json"))
                {
                    var dbSet = new QBRssDbSet(this._jsonService, path);
                    dbSet.LoadFile();
                    this._items.Add(dbSet);
                }
            }
        }

        public void SaveChanges()
        {
            foreach (var item in this._items)
            {
                item.SaveFile();
            }
        }
    }
}
