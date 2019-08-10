using System.Collections.Generic;

namespace QBRssEditor.Services.KeywordEmitter
{
    class OriginKeywordEmitter : IKeywordEmitter
    {
        public IEnumerable<string> GetKeywords(string title) => new[] { title };
    }
}
