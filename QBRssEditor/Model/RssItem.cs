using System;
using System.Collections;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QBRssEditor.Abstractions;

namespace QBRssEditor.Model
{
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
