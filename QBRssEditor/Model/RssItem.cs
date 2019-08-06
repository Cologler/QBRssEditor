using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

    class RssFile
    {
        public RssFile(string path)
        {
            this.File = new FileInfo(path);
        }

        public FileInfo File { get; }
    }
}
