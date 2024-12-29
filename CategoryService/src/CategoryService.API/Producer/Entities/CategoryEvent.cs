using CategoryService.API.Producer.Enums;

namespace CategoryService.API.Producer.Entities
{
    public class CategoryEvent
    {
        public int CategoryId { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }

        public string Changes { get; set; }
    }
}
