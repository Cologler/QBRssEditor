using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QBRssEditor.Services.KeywordEmitter
{
    class PartsKeywordEmitter : IKeywordEmitter
    {
        private static readonly Regex Scope1Regex = new Regex("【([^】]+)】");
        private static readonly Regex Scope2Regex = new Regex("\\[([^\\]]+)\\]");

        public IEnumerable<string> GetKeywords(string title)
        {
            foreach (var match in new[] { Scope1Regex, Scope2Regex }.SelectMany(z => z.Matches(title).Cast<Match>()))
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
