using Moq;
using Confluent.Kafka;
using Newtonsoft.Json;
using CategoryService.API.Producer;
using CategoryService.API.Producer.Entities;
using CategoryService.API.Producer.Enums;
using Microsoft.OpenApi.Extensions;

namespace CategoryService.Tests.Producers
{
    public class KafkaProducerTests
    {
        [Fact]
        public async Task ProduceProductEventAsync_ShouldSendMessageToKafka()
        {
            // Arrange
            var mockProducer = new Mock<IProducer<Null, string>>();
            var producer = new KafkaProducer();

            var categoryEvent = new CategoryEvent
            {
                CategoryId = 2,
                Type = EventType.Created.GetDisplayName(),
                Timestamp = DateTime.UtcNow,
                Changes = "Unit test"
            };

            var expectedMessage = JsonConvert.SerializeObject(categoryEvent);

            mockProducer
                .Setup(p => p.ProduceAsync(
                    It.IsAny<string>(),
                    It.Is<Message<Null, string>>(m => m.Value == expectedMessage),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<Null, string>());

            // Act
            await producer.ProduceCategoryEventAsync(categoryEvent);

            // Assert
            mockProducer.Verify(
                p => p.ProduceAsync(
                    "category-events",
                    It.Is<Message<Null, string>>(m => m.Value == expectedMessage),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
