﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.LocalDb;

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

        public async Task<List<ResourceItem>> ListAllAsync(bool includeHided)
        {
            await this._updateDbService.UpdateDbAsync().ConfigureAwait(false);

            using (var scope = App.ServiceProvider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
                var query = includeHided
                    ? ctx.Resources
                    : ctx.Resources.Where(z => !z.IsHided);
                return await query.ToListAsync().ConfigureAwait(false);
            }
        }

        public Task HideAsync(IEnumerable<ResourceItem> items)
        {
            return Task.Run(() =>
            {
                foreach (var markReadService in this._hideItemServices)
                {
                    markReadService.Hide(items);
                }
            });
        }
    }
}
