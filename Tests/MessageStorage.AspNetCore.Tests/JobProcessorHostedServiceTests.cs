using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Moq;
using NUnit.Framework;

namespace MessageStorage.AspNetCore.Tests
{
    public class Tests
    {
        private Mock<IJobProcessor> _mockJobProcessor;
        private JobProcessorHostedService<IJobProcessor> _sut;

        [SetUp]
        public void Setup()
        {
            _mockJobProcessor = new Mock<IJobProcessor>();
            _mockJobProcessor.Setup(processor => processor.StartAsync(It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _mockJobProcessor.Setup(processor => processor.StopAsync(It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            
            _sut = new JobProcessorHostedService<IJobProcessor>(_mockJobProcessor.Object);
        }

        [Test]
        public async Task WhenJobProcessorHostedServiceStart__JobProcessorWillBeStart()
        {
            await _sut.StartAsync(CancellationToken.None);
            
            _mockJobProcessor.Verify(processor => processor.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Test]
        public async Task WhenJobProcessorHostedServiceStop__JobProcessorWillBeStop()
        {
            await _sut.StopAsync(CancellationToken.None);
            
            _mockJobProcessor.Verify(processor => processor.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}