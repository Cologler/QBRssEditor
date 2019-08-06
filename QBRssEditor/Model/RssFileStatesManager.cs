using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QBRssEditor.Model
{
    class RssFileStatesManager
    {
        private readonly object _syncRoot = new object();
        private readonly DirectoryInfo _rootDir;
        private Task<List<RssItem>> _task;
        private List<RssFileState> _states;

        RssFileStatesManager(string rootDir)
        {
            this._rootDir = new DirectoryInfo(rootDir);
        }

        public async Task LoadStatesAsync()
        {
            this._states = await Task.Run(this.GetRssFileStates);
        }

        public async Task<List<RssFileState>> GetStatesAsync()
        {
            await this.LoadStatesAsync();
            return this._states;
        }

        public async Task<List<RssItem>> ListAsync()
        {
            await this.LoadStatesAsync();
            return this._states.SelectMany(z => z.Items).ToList();
        }

        private List<RssFileState> GetRssFileStates()
        {
            return this._rootDir.GetFiles("*.json").Select(z =>
            {
                using (var text = z.OpenText())
                {
                    return new RssFileState(z, JsonConvert.DeserializeObject<List<RssItem>>(text.ReadToEnd()));
                }
            }).ToList();
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

        public static RssFileStatesManager Installed { get; }

        static RssFileStatesManager()
        {
            var path = Path.Combine(
                Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"),
                "qBittorrent",
                "rss",
                "articles");

            Installed = new RssFileStatesManager(path);
        }
    }
}
