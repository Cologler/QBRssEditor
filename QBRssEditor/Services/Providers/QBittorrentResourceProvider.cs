using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.Abstractions;
using QBRssEditor.LocalDb;

namespace QBRssEditor.Services.Providers
{
    class QBittorrentResourceProvider : IOptionalResourceProvider
    {
        static readonly string QBRssRootPath = Path.Combine(
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"),
            "qBittorrent",
            "rss",
            "articles");
        private readonly JsonService _jsonService;

        public QBittorrentResourceProvider(IServiceProvider serviceProvider)
        {
            this._jsonService = serviceProvider.GetRequiredService<JsonService>();
        }

        public bool? IsEnable { get; set; }

        public string Name => "qBittorrent rss";

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

            cancellationToken.ThrowIfCancellationRequested();

            if (Directory.Exists(QBRssRootPath))
                return Directory.GetFiles(QBRssRootPath, "*.json")
                    .SelectMany(SelectNotExists);

            return Enumerable.Empty<ResourceItem>();
        }

        class RssItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("isRead")]
            public bool IsRead { get; set; }

            [JsonProperty("torrentURL")]
            public string TorrentUrl { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }
        }
    }
}
