using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using QBRssEditor.Model;
using QBRssEditor.Services.KeywordEmitter;

namespace QBRssEditor.Services
{
    class GroupingService
    {
        private static readonly SeriesKeywordEmitter KeywordEmitter = new SeriesKeywordEmitter();
        public readonly Dictionary<string, string> _groups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, List<RssItem>> GetGroups(IEnumerable<RssItem> source)
        {
            var ret = new Dictionary<string, List<RssItem>>(StringComparer.OrdinalIgnoreCase);
            var @new = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var miss = new List<RssItem>();

            void Add(string key, RssItem value)
            {
                if (!ret.TryGetValue(key, out var ls))
                {
                    ls = new List<RssItem>();
                    ret.Add(key, ls);
                }
                ls.Add(value);
            }

            lock (this._groups)
            {
                foreach (var item in source)
                {
                    if (this._groups.TryGetValue(item.Title, out var g))
                    {
                        Add(g, item);
                    }
                    else
                    {
                        miss.Add(item);
                    }
                }
            }

            foreach (var item in miss)
            {
                var g = GetGroupName(item.Title);
                Add(g, item);
                @new[item.Title] = g;
            }

            lock (this._groups)
            {
                foreach (var kvp in @new)
                {
                    this._groups[kvp.Key] = kvp.Value;
                }
            }

            return ret;
        }

        private static string GetGroupName(string title) 
            => KeywordEmitter.GetKeywords(title).FirstOrDefault() ?? string.Empty;
    }
}
