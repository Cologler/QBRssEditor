using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    class JournalService : IMarkReadService
    {
        private static readonly string Path = "journal.json";
        private readonly Dictionary<string, bool> _data;
        private readonly IQBitStatusService _qBitStatus;
        private readonly JsonService _jsonService;

        public JournalService(IQBitStatusService qBitStatus, JsonService jsonService)
        {
            this._qBitStatus = qBitStatus;
            if (File.Exists(Path))
            {
                this._data = jsonService.Load<Dictionary<string, bool>>(Path);
            } 
            else
            {
                this._data = new Dictionary<string, bool>();
            }
            this._jsonService = jsonService;
        }

        public int Count => this._data.Count;

        public Task SaveAsync() => Task.Run(() => this._jsonService.Dump(Path, this._data));

        public void MarkReaded(IEnumerable<RssItem> items)
        {
            foreach (var item in items)
            {
                this._data[item.Id] = true;
            }
        }

        public async Task FlushAsync()
        {
            if (this._qBitStatus.IsRunning)
            {
                await this.SaveAsync();
            }
        }

        public void Clear() => this._data.Clear();

        public void Attach(IEnumerable<RssItem> items)
        {
            foreach (var item in items)
            {
                if (!item.IsRead)
                {
                    item.IsRead = this._data.TryGetValue(item.Id, out var v) && v;
                }
            }
        }
    }
}
