using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.LocalDb;
using QBRssEditor.Model;

namespace QBRssEditor.Services
{
    class ResourceItemsService
    {
        private readonly UpdateDbService _updateDbService;
        private readonly IEnumerable<IHideItemService> _hideItemServices;

        public ResourceItemsService(UpdateDbService updateDbService, IEnumerable<IHideItemService> markReadServices)
        {
            this._updateDbService = updateDbService;
            this._hideItemServices = markReadServices;
        }

        public async Task<List<ResourceItem>> ListAllAsync()
        {
            await this._updateDbService.UpdateDbAsync().ConfigureAwait(false);

            using (var scope = App.ServiceProvider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
                return await ctx.Resources.ToListAsync().ConfigureAwait(false);
            }
        }

        public void Hide(IEnumerable<ResourceItem> items)
        {
            foreach (var markReadService in this._hideItemServices)
            {
                markReadService.Hide(items);
            }
        }
    }
}
