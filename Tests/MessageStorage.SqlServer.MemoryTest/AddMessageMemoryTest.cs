using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.dotMemoryUnit;
using MessageStorage.SqlServer.MemoryTest.Fixtures;
using MessageStorage.SqlServer.MemoryTest.Fixtures.MessageHandlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MessageStorage.SqlServer.MemoryTest;

public class AddMessageMemoryTest
{
    private TestServerFixture _testServerFixture = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testServerFixture = new TestServerFixture();
    }

    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    [Test, Timeout(10000)]
    public void AddMessageAsync()
    {
        MemoryCheckPoint memoryCheckPoint = dotMemory.Check();

        Parallel.For(0,
                     100,
                     new ParallelOptions
                     {
                         MaxDegreeOfParallelism = 10
                     },
                     i =>
                     {
                         using IServiceScope scope = _testServerFixture.GetServiceScope();
                         using var messageStorageClient = scope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
                         var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestServer>>();
                         var basicMessage = new BasicMessage($"{i}---{Guid.NewGuid().ToString()}");
                         var stopWatch = Stopwatch.StartNew(); 
                         messageStorageClient.AddMessageAsync(basicMessage).GetAwaiter().GetResult();
                         stopWatch.Stop();
                         logger.LogInformation("{Index} | ElapsedTime : {ElapsedTime} ms", i, stopWatch.ElapsedMilliseconds);
                     });

        dotMemory.Check(memory =>
        {
            int messageStorageSurvivedObjectCount = memory.GetDifference(memoryCheckPoint)
                                                          .GetSurvivedObjects()
                                                          .GetObjects(obj => obj.Namespace.Like("MessageStorage."))
                                                          .ObjectsCount;
            Assert.That(messageStorageSurvivedObjectCount, Is.EqualTo(0));
        });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _testServerFixture.Dispose();
    }
}