using Newtonsoft.Json;
using ProductService.API.Producer.Entities;
using ProductService.API.Producer.Enums;
using Confluent.Kafka;
using Microsoft.OpenApi.Extensions;

namespace ProductService.API.Producer
{
    public class KafkaProducer
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "product-events";  // Kafka topic for product events
        private readonly ProducerConfig _config;

        public KafkaProducer()
        {
            _config = new ProducerConfig()
            {
                BootstrapServers = _bootstrapServers
            };
        }

        public KafkaProducer(ProducerConfig producerConfig)
        {
            _config = producerConfig;
            _bootstrapServers = producerConfig.BootstrapServers;
        }

        public async Task ProduceProductEventAsync(int productId, EventType type, string changes = "")
        {
            var productEvent = new ProductEvent
            {
                ProductId = productId,
                Type = type.GetDisplayName(),
                Timestamp = DateTime.UtcNow,
                Changes = changes
            };

            await ProduceProductEventAsync(productEvent);
        }

        public async Task ProduceProductEventAsync(ProductEvent productEvent)
        {
            using (var producer = new ProducerBuilder<Null, string>(_config).Build())
            {
                try
                {
                    var messageValue = JsonConvert.SerializeObject(productEvent); // Serialize event object

                    var result = await producer.ProduceAsync(_topic, new Message<Null, string> { Value = messageValue });
                    Console.WriteLine($"Message sent to Kafka: {result.Value}");
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Error sending message: {e.Error.Reason}");
                }
            }
        }
    }
}
