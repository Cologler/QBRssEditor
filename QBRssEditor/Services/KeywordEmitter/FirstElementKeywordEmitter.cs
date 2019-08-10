using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QBRssEditor.Services.KeywordEmitter
{
    class FirstElementKeywordEmitter : IKeywordEmitter
    {
        private static readonly Regex DotRegex = new Regex(
            "^([^.]*).", RegexOptions.IgnoreCase); // *.*

        public IEnumerable<string> GetKeywords(string title)
        {
            var match = DotRegex.Match(title);
            if (match.Success)
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
