using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QBRssEditor.Services.KeywordEmitter
{
    class SeriesKeywordEmitter : IKeywordEmitter
    {
        private static readonly Regex TVShowRegex = new Regex(
            "^(.*)\\.(ep?\\d{1,3}|s\\d{1,3}e\\d{1,3})\\.(.*)$", RegexOptions.IgnoreCase); // *.ep1.* | *.s01e01.*
        private static readonly Regex JPAnimeRegex1 = new Regex(
            "^(.*)\\[\\d{1,3}\\](.*)$", RegexOptions.IgnoreCase); // *[01]*
        private static readonly Regex JPAnimeRegex2 = new Regex(
            "^(.*)【\\d{1,3}】(.*)$", RegexOptions.IgnoreCase); // *【01】*

        public IEnumerable<string> GetKeywords(string title)
        {
            var match = TVShowRegex.Match(title);
            if (match.Success)
            {
                yield return match.Groups[1].Value;
            }

            match = JPAnimeRegex1.Match(title);
            if (match.Success)
            {
                yield return match.Groups[1].Value;
            }

            match = JPAnimeRegex2.Match(title);
            if (match.Success)
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
