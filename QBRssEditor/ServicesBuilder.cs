﻿using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.Abstractions;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;
using QBRssEditor.Services;
using QBRssEditor.Services.KeywordEmitter;

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
                .AddSingleton<IQBitStatusService, QBitStatusService>()
                .AddSingleton<UpdateDbService>()
                .AddDbContext<LocalDbContext>(options => options.UseSqlite($"Data Source=localdb.sqlite3"))
                .AddSingleton<RssItemsService>()
                .AddSingleton<JournalService>()
                .AddSingleton<IHideItemService>(ioc => ioc.GetRequiredService<JournalService>())
                .AddSingleton<OriginMarkReadService>()
                .AddSingleton<IHideItemService>(ioc => ioc.GetRequiredService<OriginMarkReadService>())
                .AddSingleton<IHideItemService, LocalDbHideItemService>()
                .AddSingleton<FileSessionService>()
                .AddTransient<MainWindowViewModel>()
                .AddTransient<QBRssDataContext>()
                .AddTransient<IResourceProvider, QBRssDataContext>()
                .AddSingleton<IKeywordEmitter, OriginKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, MovieKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, SeriesKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, FirstElementKeywordEmitter>()
                .AddSingleton<IKeywordEmitter, PartsKeywordEmitter>()
                .AddSingleton<GroupingService>()
                .BuildServiceProvider();
        }
    }
}