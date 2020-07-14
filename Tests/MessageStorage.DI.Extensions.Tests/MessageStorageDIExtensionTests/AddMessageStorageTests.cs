using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace MessageStorage.DI.Extensions.Tests.MessageStorageDIExtensionTests
{
    public class Tests
    {
        private Mock<IServiceCollection> _mockServiceCollection;

        [SetUp]
        public void Setup()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
        }

        [Test]
        public void WhenGivenActionIsNull_ExceptionShouldNotBeOccur()
        {
            _mockServiceCollection.Object.AddMessageStorage(null);
        }

        [Test]
        public void WhenGivenActionIsNotNull_ActionWillBeExecuted()
        {
            var functionExecuted = false;
            _mockServiceCollection.Object.AddMessageStorage(collection => { functionExecuted = true; });

            Assert.IsTrue(functionExecuted);
        }
    }
}