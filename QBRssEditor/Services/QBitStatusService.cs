using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace QBRssEditor.Services
{
    class QBitStatusService : IQBitStatusService
    {
        public bool IsRunning { get; private set; }

        private Process TryGetProcess()
        {
            var processes = Process.GetProcessesByName("qbittorrent");
            return processes.FirstOrDefault();
        }

        public Task WaitForExitAsync()
        {
            return Task.Run(() =>
            {
                var proc = this.TryGetProcess();
                if (proc != null && !proc.HasExited)
                {
                    proc.WaitForExit();
                }
            });
        }

        public async Task UpdateStatusAsync()
        {
            this.IsRunning = !await Task.Run(() =>
            {
                var proc = this.TryGetProcess();
                return proc == null || proc.HasExited;
            });
        }
    }
}
