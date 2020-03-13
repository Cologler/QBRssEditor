using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.LocalDb;
using QBRssEditor.Services;

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
                MessageBox.Show("another instance is opened, shutdown this now.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
