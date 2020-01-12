using System.Collections.Generic;

namespace QBRssEditor.Services.KeywordEmitter
{
    interface IKeywordEmitter
    {
        IEnumerable<string> GetKeywords(string title);
    }
}
