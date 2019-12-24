using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;
using QBRssEditor.Services;
using QBRssEditor.Services.KeywordEmitter;

namespace QBRssEditor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private const string MutexName = "QBRssEditor";

        private Mutex _mutex;

        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, MutexName, out var isNew);
            if (!isNew)
            {
                this.Shutdown();
            }

            base.OnStartup(e);

            Debug.Assert(ServiceProvider == null);
            ServiceProvider = ServicesBuilder.CreateServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<LocalDbContext>().Database.EnsureCreated();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider.GetRequiredService<FileSessionService>().Dispose();

            base.OnExit(e);
        }
    }
}
