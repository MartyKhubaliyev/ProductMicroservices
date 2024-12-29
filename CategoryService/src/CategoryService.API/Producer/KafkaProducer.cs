using Newtonsoft.Json;
using CategoryService.API.Producer.Entities;
using CategoryService.API.Producer.Enums;
using Confluent.Kafka;
using Microsoft.OpenApi.Extensions;

namespace CategoryService.API.Producer
{
    public class KafkaProducer
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "category-events";  // Kafka topic for product events
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

        public async Task ProduceCategoryEventAsync(int categoryId, EventType type, string changes = "")//ProduceCategoryEventAsync
        {
            var categoryEvent = new CategoryEvent
            {
                CategoryId = categoryId,
                Type = type.GetDisplayName(),
                Timestamp = DateTime.UtcNow,
                Changes = changes
            };

            await ProduceCategoryEventAsync(categoryEvent);
        }

        public async Task ProduceCategoryEventAsync(CategoryEvent categoryEvent)
        {
            using (var producer = new ProducerBuilder<Null, string>(_config).Build())
            {
                try
                {
                    var messageValue = JsonConvert.SerializeObject(categoryEvent); // Serialize event object

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
