using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.Abstractions;
using QBRssEditor.LocalDb;
using QBRssEditor.Services;

namespace QBRssEditor.Model
{
    class QBRssDataContext : IOptionalResourceProvider
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

        public bool? IsEnable { get; set; }

        public string Name => "qBittorrent rss";

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

        public IEnumerable<ResourceItem> GetNotExists(HashSet<string> existsKeys, CancellationToken cancellationToken)
        {
            IEnumerable<ResourceItem> SelectNotExists(string path)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var name = Path.GetFileNameWithoutExtension(path);
                foreach (var item in this._jsonService.Load<List<RssItem>>(path))
                {
                    var id = $"{name}::{item.Id}";
                    if (!existsKeys.Contains(id))
                    {
                        yield return new ResourceItem
                        {
                            Id = id,
                            Title = item.Title,
                            Description = item.Description,
                            Url = item.TorrentUrl,
                            IsHided = item.IsRead
                        };
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (Directory.Exists(QBRssRootPath))
            {
                return Directory.GetFiles(QBRssRootPath, "*.json")
                    .SelectMany(SelectNotExists);
            }

            return Enumerable.Empty<ResourceItem>();
        }
    }
}
