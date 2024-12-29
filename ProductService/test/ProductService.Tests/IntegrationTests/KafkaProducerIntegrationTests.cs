using Confluent.Kafka;
using ProductService.API.Producer;
using ProductService.API.Producer.Enums;

namespace ProductService.Tests.IntegrationTests
{
    public class KafkaProducerIntegrationTests
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "category-events";

        [Fact]
        public async Task ProduceProductEventAsync_ShouldSendMessageToKafkaCluster()
        {
            // Arrange
            var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };
            var producer = new KafkaProducer(producerConfig);

            // Act
            await producer.ProduceProductEventAsync(123, EventType.Created, "Integration Test");

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
