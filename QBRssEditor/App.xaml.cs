using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Debug.Assert(ServiceProvider == null);
            ServiceProvider = ServicesBuilder.CreateServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider.GetRequiredService<FileSessionService>().Dispose();

            base.OnExit(e);
        }
    }
}
