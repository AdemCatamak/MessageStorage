using System;
using MessageStorage.Db;
using Xunit;

namespace MessageStorage.DbUnitTests
{
    public class MessageStorageDbConfigurationFactory_CreateTests
    {
        [Fact]
        public void WhenConnectionStrIsNull__ArgumentNullExceptionOccurs()
        {
            Assert.Throws<ArgumentNullException>(() => MessageStorageDbConfigurationFactory.Create(connectionStr: null));
        }

        [Fact]
        public void WhenConnectionStrIsNotNull__SchemaDefaultValueShouldBeMessageStorage()
        {
            const string connectionStr = "connectionStr";
            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);

            Assert.NotNull(messageStorageDbConfiguration);
            Assert.Equal(connectionStr, messageStorageDbConfiguration.ConnectionStr);
            Assert.NotNull(messageStorageDbConfiguration.Schema);
            Assert.Equal("MessageStorage", messageStorageDbConfiguration.Schema);
        }

        [Fact]
        public void WhenConnectionStrIsNotNull_and_SchemaIsGiven__SchemaShouldBeEqualToGiven()
        {
            const string connectionStr = "connectionStr";
            const string schema = "schema";
            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr, schema);

            Assert.NotNull(messageStorageDbConfiguration);
            Assert.Equal(connectionStr, messageStorageDbConfiguration.ConnectionStr);
            Assert.NotNull(messageStorageDbConfiguration.Schema);
            Assert.Equal(schema, messageStorageDbConfiguration.Schema);
        }

        [Fact]
        public void WhenConnectionStrIsNotNull_and_SchemaIsGivenThatStartWithSpaceOrEndWithSpace__SchemaShouldBeEqualToGiven()
        {
            const string connectionStr = "connectionStr";
            const string givenSchema = " schema ";
            const string expectedSchema = "schema";
            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr, givenSchema);

            Assert.NotNull(messageStorageDbConfiguration);
            Assert.Equal(connectionStr, messageStorageDbConfiguration.ConnectionStr);
            Assert.NotNull(messageStorageDbConfiguration.Schema);
            Assert.Equal(expectedSchema, messageStorageDbConfiguration.Schema);
        }
    }
}