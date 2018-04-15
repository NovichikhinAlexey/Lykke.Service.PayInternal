using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.
    [UsedImplicitly]
    public class ShutdownManager : IShutdownManager
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILog _log;

        public ShutdownManager(ILog log)
        {
            _log = log;
        }

        public async Task StopAsync()
        {
            await Task.CompletedTask;
        }
    }
}
