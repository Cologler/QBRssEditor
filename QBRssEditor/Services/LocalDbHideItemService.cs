using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    class LocalDbHideItemService : IHideItemService
    {
        private readonly IServiceProvider _serviceProvider;

        public LocalDbHideItemService(IServiceProvider serviceProvider) 
        {
            this._serviceProvider = serviceProvider;
        }

        public void Hide(IEnumerable<ResourceItem> items)
        {
            using (var scope = this._serviceProvider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
                foreach (var item in items)
                {
                    item.IsHided = true;
                    var entity = ctx.Resources.Find(item.Id);
                    if (entity != null)
                    {
                        entity.IsHided = true;
                    }
                }
                ctx.SaveChanges();
            }
        }
    }
}
