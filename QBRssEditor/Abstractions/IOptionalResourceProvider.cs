namespace QBRssEditor.Abstractions
{
    interface IOptionalResourceProvider : IResourceProvider
    {
        bool? IsEnable { get; set; }
    }
}
