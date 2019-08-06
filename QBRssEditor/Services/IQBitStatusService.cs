using System.Threading.Tasks;

namespace QBRssEditor.Services
{
    interface IQBitStatusService
    {
        Task WaitForExitAsync();

        Task UpdateStatusAsync();

        bool IsRunning { get; }
    }
}
