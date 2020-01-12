using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.Model.Configuration;
using QBRssEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBRssEditor.Model
{
    class AppSettings
    {
        private static readonly string Path = "appsettings.json";
        private readonly IServiceProvider _serviceProvider;
        private readonly AppConfig _appConfig;

        public AppSettings(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;

            if (File.Exists(Path))
            {
                this._appConfig = serviceProvider.GetRequiredService<JsonService>().Load<AppConfig>(Path);
            }
            else
            {
                this._appConfig = new AppConfig();
            }
        }

        public void Save() => this._serviceProvider.GetRequiredService<JsonService>().Dump(Path, this._appConfig);

        public ProviderSettings GetProviderSettings(string providerName) => new ProviderSettings(this, providerName);

        public class ProviderSettings
        {
            private readonly AppSettings _appSettings;
            private readonly string _providerName;

            public ProviderSettings(AppSettings appSettings, string providerName)
            {
                this._appSettings = appSettings;
                this._providerName = providerName;
            }

            private ProviderConfig GetConfig() =>
                this._appSettings._appConfig.ProviderSettings.TryGetValue(this._providerName, out var c) ? c : default;

            private ProviderConfig GetOrAddConfig()
            {
                if (!this._appSettings._appConfig.ProviderSettings.TryGetValue(this._providerName, out var c))
                {
                    c = new ProviderConfig();
                    this._appSettings._appConfig.ProviderSettings.Add(this._providerName, c);
                }

                return c;
            }

            public bool IsEnabled
            {
                get => this.GetConfig()?.IsEnabled ?? false;
                set
                {
                    var conf = this.GetOrAddConfig();
                    if (conf.IsEnabled != value)
                    {
                        conf.IsEnabled = value;
                        this._appSettings.Save();
                    }
                }
            }
        }
    }
}
