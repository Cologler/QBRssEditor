using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QBRssEditor.Model;

namespace QBRssEditor.EntityQuery
{
    interface IRuleSet<TPattern, TItem>
    {
        string Mode { get; }

        void Add(TPattern pattern);

        bool IsMatch(TItem item);
    }

    static class MatchModes
    {
        public new const string Equals = "equals";
        public const string EqualsIgnoreCase = "equals-ignore-case";
        public const string Include = "include";
        public const string IncludeIgnoreCase = "include-ignore-case";
        public const string Regexp = "regex";

        public static IRuleSet<string, string> CreateRules(string matchMode)
        {
            switch (matchMode)
            {
                case Equals:
                    return new EqualsRuleSet<string>(matchMode, StringComparer.Ordinal);

                case EqualsIgnoreCase:
                    return new EqualsRuleSet<string>(matchMode, StringComparer.OrdinalIgnoreCase);

                case Include:
                    return new EachRuleSet<string>(matchMode, new IncludeMatch());

                case IncludeIgnoreCase:
                    return new EachRuleSet<string>(matchMode, new IncludeIgnoreCaseMatch());

                default:
                    throw new NotImplementedException();
            }
        }

        interface IMatch<TValue>
        {
            bool IsMatch(TValue value);
        }

        interface IMatchMethod<TPattern, TValue>
        {
            bool IsMatch(TPattern pattern, TValue value);
        }

        class IncludeMatch : IMatchMethod<string, string>
        {
            public bool IsMatch(string pattern, string value) 
                => pattern != null && pattern.Contains(value);
        }

        class IncludeIgnoreCaseMatch : IMatchMethod<string, string>
        {
            public bool IsMatch(string pattern, string value) 
                => pattern != null && pattern.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        class EqualsRuleSet<T> : IRuleSet<T, T>
        {
            private readonly HashSet<T> _set = new HashSet<T>();

            public EqualsRuleSet(string mode, IEqualityComparer<T> comparer)
            {
                this.Mode = mode ?? throw new ArgumentNullException(nameof(mode));
                this._set = new HashSet<T>(comparer ?? throw new ArgumentNullException(nameof(comparer)));
            }

            public string Mode { get; }

            public void Add(T pattern) => this._set.Add(pattern);

            public bool IsMatch(T value) => this._set.Contains(value);
        }

        class EachRuleSet<T> : IRuleSet<T, T>
        {
            private readonly IMatchMethod<T, T> _match;
            private readonly List<T> _patterns = new List<T>();

            public EachRuleSet(string mode, IMatchMethod<T, T> match)
            {
                this.Mode = mode ?? throw new ArgumentNullException(nameof(mode));
                this._match = match ?? throw new ArgumentNullException(nameof(match));
            }

            public string Mode { get; }

            public void Add(T pattern) => this._patterns.Add(pattern);

            public bool IsMatch(T value) => this._patterns.Any(z => this._match.IsMatch(z, value));
        }

        abstract class ListRule<TPattern, TItem> : IRuleSet<TPattern, TItem>
        {
            private readonly List<IMatch<TItem>> _patterns = new List<IMatch<TItem>>();

            public ListRule(string mode)
            {
                this.Mode = mode ?? throw new ArgumentNullException(nameof(mode));
            }

            public string Mode { get; }

            protected abstract IMatch<TItem> CreateMatch(TPattern pattern);

            public void Add(TPattern pattern) => this._patterns.Add(this.CreateMatch(pattern));

            public bool IsMatch(TItem value) => this._patterns.Any(z => z.IsMatch(value));
        }

        class DelegateListRule<TPattern, TItem> : ListRule<TPattern, TItem>
        {
            private readonly Func<TPattern, IMatch<TItem>> _factory;

            public DelegateListRule(string mode, Func<TPattern, IMatch<TItem>> factory) : base(mode)
            {
                this._factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            protected override IMatch<TItem> CreateMatch(TPattern pattern) => this._factory(pattern);
        }

        class RegexListRule : ListRule<string, string>
        {
            public RegexListRule(string mode) : base(mode)
            {
            }

            protected override IMatch<string> CreateMatch(string pattern) => new RegexMatch(new Regex(pattern));

            class RegexMatch : IMatch<string>
            {
                private readonly Regex _regex;

                public RegexMatch(Regex regex)
                {
                    this._regex = regex ?? throw new ArgumentNullException(nameof(regex));
                }

                public bool IsMatch(string value) => this._regex.IsMatch(value);
            }
        }
    }

    enum EffectOn
    {
        Title = 0,
        Description = 1
    }

    class Data
    {
        public EffectOn Target { get; set; }

        public string MatchMode { get; set; }

        public string Value { get; set; }
    }

    abstract class TextFilterManager
    {
        private readonly Dictionary<string, IRuleSet<string, string>> _ruleSets = new Dictionary<string, IRuleSet<string, string>>();

        public abstract EffectOn Target { get; }

        protected abstract string GetText(RssItem item);

        public void Add(Data data)
        {
            IRuleSet<string, string> GetOrAddRules(string mode)
            {
                if (!this._ruleSets.TryGetValue(mode, out var rules))
                {
                    this._ruleSets.Add(mode, rules = MatchModes.CreateRules(mode));
                }

                return rules;
            }

            GetOrAddRules(data.MatchMode).Add(data.Value);
        }

        public bool IsMatch(RssItem item)
        {
            var text = this.GetText(item);
            return this._ruleSets.Any(x => x.Value.IsMatch(text));
        }
    }

    class TitleFilterManager : TextFilterManager
    {
        public override EffectOn Target => EffectOn.Title;

        protected override string GetText(RssItem item) => item.Title;
    }

    class DescriptionFilterManager : TextFilterManager
    {
        public override EffectOn Target => EffectOn.Description;

        protected override string GetText(RssItem item) => item.Description;
    }

    class FilterManager
    {
        private readonly List<TextFilterManager> _textFilterManagers = new List<TextFilterManager>();

        public FilterManager()
        {
            this._textFilterManagers.Add(new TitleFilterManager());
            this._textFilterManagers.Add(new DescriptionFilterManager());
        }

        public bool IsMatch(RssItem item) => this._textFilterManagers.Any(x => x.IsMatch(item));

        public void LoadState(IEnumerable<Data> datas)
        {
            TextFilterManager GetFilterManager(EffectOn on)
            {
                return this._textFilterManagers.Single(z => z.Target == on);
            }

            foreach (var data in datas)
            {
                GetFilterManager(data.Target).Add(data);
            }
        }

        public IEnumerable<Data> DumpState() 
        {
            return Enumerable.Empty<Data>();
        }
    }

    class Picker<T, TCategory>
    {
        TCategory Pick(T obj)
        {
            return default;
        }
    }
}
