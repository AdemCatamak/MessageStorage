using System;
using MessageStorage.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace MessageStorage.AspNetCore.Tests
{
    public class JobProcessorHostedServiceExtensionTests
    {
        private IServiceCollection _serviceCollection;

        [SetUp]
        public void Setup()
        {
            _serviceCollection = new ServiceCollection();
        }

        [Test]
        public void WhenAddJobProcessorHostedServiceIsUsed_WithoutType__IJobProcessorIsRequired()
        {
            _serviceCollection.AddJobProcessorHostedService();

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<IHostedService>());
            }

            _serviceCollection.AddSingleton(new Mock<IJobProcessor>().Object);

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var hostedService = serviceProvider.GetService<IHostedService>();
                Assert.IsNotNull(hostedService);
            }
        }

        [Test]
        public void WhenAddJobProcessorHostedServiceIsUsed_WithType__TJobProcessorIsRequired()
        {
            _serviceCollection.AddJobProcessorHostedService<ICustomJobProcessor>();

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<IHostedService>());
            }

            _serviceCollection.AddSingleton(new Mock<IJobProcessor>().Object);

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<IHostedService>());
            }

            _serviceCollection.AddSingleton(new Mock<ICustomJobProcessor>().Object);

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var jobProcessorHostedService = serviceProvider.GetService<IHostedService>();
                Assert.NotNull(jobProcessorHostedService);
            }
        }


        public interface ICustomJobProcessor : IJobProcessor
        {
        }
    }
}