using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.Services;

namespace QBRssEditor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Debug.Assert(ServiceProvider == null);
            ServiceProvider = new ServiceCollection()
                .AddSingleton(new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                })
                .AddSingleton<IQBitStatusService, QBitStatusService>()
                .AddSingleton<RssItemsService>()
                .AddSingleton<JournalService>()
                .AddSingleton<IMarkReadService>(ioc => ioc.GetRequiredService<JournalService>())
                .AddSingleton<OriginMarkReadService>()
                .AddSingleton<IMarkReadService>(ioc => ioc.GetRequiredService<OriginMarkReadService>())
                .AddSingleton<FileWriteWaiterService>()
                .BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider.GetRequiredService<FileWriteWaiterService>().WaitAll();

            base.OnExit(e);
        }
    }
}
