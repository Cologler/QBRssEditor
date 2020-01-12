using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBRssEditor.Model.Configuration
{
    class AppConfig
    {
        public Dictionary<string, ProviderConfig> ProviderSettings { get; set; } = new Dictionary<string, ProviderConfig>();
    }
}
