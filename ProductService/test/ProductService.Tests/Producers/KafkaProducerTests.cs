using Confluent.Kafka;
using Moq;
using Newtonsoft.Json;
using ProductService.API.Producer;
using ProductService.API.Producer.Entities;
using ProductService.API.Producer.Enums;
using Microsoft.OpenApi.Extensions;

namespace ProductService.Tests.Producers
{
    public class KafkaProducerTests
    {
        [Fact]
        public async Task ProduceProductEventAsync_ShouldSendMessageToKafka()
        {
            // Arrange
            var mockProducer = new Mock<IProducer<Null, string>>();
            var producer = new KafkaProducer();

            var productEvent = new ProductEvent
            {
                ProductId = 2,
                Type = EventType.Created.GetDisplayName(),
                Timestamp = DateTime.UtcNow,
                Changes = "Unit test"
            };

            var expectedMessage = JsonConvert.SerializeObject(productEvent);

            mockProducer
                .Setup(p => p.ProduceAsync(
                    It.IsAny<string>(),
                    It.Is<Message<Null, string>>(m => m.Value == expectedMessage),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<Null, string>());

            // Act
            await producer.ProduceProductEventAsync(productEvent);

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
