using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QBRssEditor.Services.KeywordEmitter
{
    class MovieKeywordEmitter : IKeywordEmitter
    {
        private static readonly Regex Regex1 = new Regex("^(.*)\\.\\d{4}\\.(.*)$"); // *.2019.*

        public IEnumerable<string> GetKeywords(string title)
        {
            var match = Regex1.Match(title);
            if (match.Success)
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
