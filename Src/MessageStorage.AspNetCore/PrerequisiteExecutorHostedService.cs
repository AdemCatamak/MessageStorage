using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.AspNetCore
{
    public class PrerequisiteExecutorHostedService : IHostedService
    {
        private readonly IEnumerable<IPrerequisite> _prerequisites;

        public PrerequisiteExecutorHostedService(IEnumerable<IPrerequisite> prerequisites)
        {
            _prerequisites = prerequisites;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (IPrerequisite prerequisite in _prerequisites)
            {
                prerequisite.Execute();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}