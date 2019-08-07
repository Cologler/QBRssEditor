﻿using System.Collections.Generic;
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
        private readonly JsonSerializerSettings _settings;
        private readonly FileWriteWaiterService _writer;

        public JournalService(IQBitStatusService qBitStatus, JsonSerializerSettings settings, FileWriteWaiterService writer)
        {
            this._qBitStatus = qBitStatus;
            this._settings = settings;
            this._writer = writer;
            if (File.Exists(Path))
            {
                this._data = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(Path));
            } 
            else
            {
                this._data = new Dictionary<string, bool>();
            }
        }

        public int Count => this._data.Count;

        public Task SaveAsync() => 
            this._writer.WriteAsync(
                Task.Run(() => 
                    File.WriteAllText(Path, JsonConvert.SerializeObject(this._data, this._settings), new UTF8Encoding(false))));

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
