using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.Abstractions;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;
using QBRssEditor.Services;
using QBRssEditor.Services.KeywordEmitter;
using QBRssEditor.Services.Providers;

namespace QBRssEditor
{
    internal class ServicesBuilder
    {
        internal static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                })
                .AddSingleton<Encoding>(new UTF8Encoding(false))
                .AddSingleton<JsonService>()
                .AddSingleton<UpdateDbService>()
                .AddDbContext<LocalDbContext>(options => options.UseSqlite($"Data Source=localdb.sqlite3"))
                .AddSingleton<ResourceItemsService>()
                .AddSingleton<IHideItemService, LocalDbHideItemService>()
                .AddSingleton<FileSessionService>()
                .AddTransient<MainWindowViewModel>()
                .AddTransient<IResourceProvider, QBittorrentResourceProvider>()
                .AddSingleton<IKeywordEmitter, OriginKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, MovieKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, SeriesKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, FirstElementKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, PartsKeywordEmitter>()
                .AddSingleton<GroupingService>()
                .AddSingleton<AppSettings>()
                .BuildServiceProvider();
        }
    }
}
