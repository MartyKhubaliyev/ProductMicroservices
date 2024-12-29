using Confluent.Kafka;
using CategoryService.API.Producer;
using CategoryService.API.Producer.Enums;

namespace CategoryService.Tests.IntegrationTests
{
    public class KafkaProducerIntegrationTests
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "category-events";

        [Fact]
        public async Task ProduceCategoryEventAsync_ShouldSendMessageToKafkaCluster()
        {
            // Arrange
            var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };
            var producer = new KafkaProducer(producerConfig);

            // Act
            await producer.ProduceCategoryEventAsync(123, EventType.Created, "Integration Test");

            // Assert: Consume the message to verify
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "test-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe(_topic);

            var consumeResult = consumer.Consume();
            Assert.Contains("123", consumeResult.Message.Value);
            Assert.Contains("Created", consumeResult.Message.Value);
        }
    }
}
