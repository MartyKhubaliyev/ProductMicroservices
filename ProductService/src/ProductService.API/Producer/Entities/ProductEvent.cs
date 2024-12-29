using ProductService.API.Producer.Enums;

namespace ProductService.API.Producer.Entities
{
    public class ProductEvent
    {
        public int ProductId { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }

        public string Changes { get; set; }
    }
}
