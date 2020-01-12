using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.Abstractions;
using QBRssEditor.LocalDb;

namespace QBRssEditor.Services
{
    class UpdateDbService
    {
        private readonly object _syncRoot = new object();
        private readonly IServiceProvider _serviceProvider;
        private Task _runningUpdateDbJob;
        private DateTime? LastUpdateTime;

        public UpdateDbService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        private bool IsNeedUpdate => this.LastUpdateTime is null || DateTime.UtcNow - this.LastUpdateTime> TimeSpan.FromMinutes(10);

        public Task UpdateDbAsync()
        {
            if (this.IsNeedUpdate)
            {
                lock (this._syncRoot)
                {
                    if (this._runningUpdateDbJob is null)
                    {
                        if (this.IsNeedUpdate)
                        {
                            return this._runningUpdateDbJob = Task.Run(() =>
                            {
                                using (var scope = this._serviceProvider.CreateScope())
                                {
                                    this.InternalUpdateDb(scope.ServiceProvider, CancellationToken.None);
                                }

                                lock (this._syncRoot)
                                {
                                    this.LastUpdateTime = DateTime.UtcNow;
                                    this._runningUpdateDbJob = null;
                                }
                            });
                        }
                    }
                    else
                    {
                        return this._runningUpdateDbJob;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private void InternalUpdateDb(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var ctx = serviceProvider.GetRequiredService<LocalDbContext>();
            var rps = serviceProvider.GetServices<IResourceProvider>()
                .Where(z => (z as IOptionalResourceProvider)?.IsEnable ?? true)
                .ToArray();

            var exists = ctx.Resources.Select(z => z.Id).ToHashSet();
            var newItems = rps.SelectMany(z => z.GetNotExists(exists, cancellationToken)).ToDictionary(z => z.Id);

            cancellationToken.ThrowIfCancellationRequested();
            ctx.Resources.AddRange(newItems.Values.ToArray());
            ctx.SaveChanges();
        }
    }
}
