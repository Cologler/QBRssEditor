using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QBRssEditor.Services
{
    class JsonService
    {
        private readonly FileSessionService _sessionService;
        private readonly JsonSerializerSettings _settings;
        private readonly Encoding _encoding;

        public JsonService(FileSessionService sessionService, JsonSerializerSettings settings, Encoding encoding)
        {
            this._sessionService = sessionService;
            this._settings = settings;
            this._encoding = encoding;
        }

        public T Load<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        public bool Dump(string path, object obj)
        {
            using (var session = this._sessionService.OpenSession(path))
            {
                if (session.IsOpened)
                {
                    var json = JsonConvert.SerializeObject(obj, this._settings);
                    File.WriteAllText(path, json, this._encoding);
                    return true;
                }

                return false;
            }
        }
    }
}
