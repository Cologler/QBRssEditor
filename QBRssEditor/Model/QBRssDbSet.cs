using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QBRssEditor.Services;

namespace QBRssEditor.Model
{
    class QBRssDbSet : IEnumerable<RssItem>
    {
        private readonly JsonService _jsonService;
        private readonly string _path;
        private List<RssItem> _data;

        public QBRssDbSet(JsonService jsonService, string path)
        {
            this._jsonService = jsonService;
            this._path = path;
        }

        public IEnumerator<RssItem> GetEnumerator() => (this._data ?? Enumerable.Empty<RssItem>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void LoadFile() => this._data = this._jsonService.Load<List<RssItem>>(this._path);

        public void SaveFile()
        {
            if (this._data != null)
            {
                this._jsonService.Dump(this._path, this._data);
            }
        }
    }
}
