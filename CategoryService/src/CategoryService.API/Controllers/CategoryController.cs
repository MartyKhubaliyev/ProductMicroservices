using Microsoft.AspNetCore.Mvc;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Repositories;
using CategoryService.API.Producer;
using CategoryService.API.Producer.Enums;
using Newtonsoft.Json;

namespace CategoryService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly KafkaProducer _kafkaProducer;

        public CategoryController(IRepository<Category> categoryRepository, KafkaProducer kafkaProducer)
        {
            _categoryRepository = categoryRepository;
            _kafkaProducer = kafkaProducer;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Category category)
        {
            if (category == null)
            {
                return BadRequest();
            }

            await _categoryRepository.AddAsync(category);

            await _kafkaProducer.ProduceCategoryEventAsync(category.Id, EventType.Created, JsonConvert.SerializeObject(category));

            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        // PUT: api/Category
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Category category)
        {
            if (category == null)
            {
                return BadRequest();
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(category.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = category.Name;

            await _categoryRepository.UpdateAsync(existingCategory);

            await _kafkaProducer.ProduceCategoryEventAsync(category.Id, EventType.Updated, JsonConvert.SerializeObject(category));

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _kafkaProducer.ProduceCategoryEventAsync(id, EventType.Deleted);

            await _categoryRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
