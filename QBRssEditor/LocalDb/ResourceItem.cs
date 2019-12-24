using QBRssEditor.Abstractions;

namespace QBRssEditor.LocalDb
{
    class ResourceItem : IGroupable
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public bool IsHided { get; set; }
    }
}
