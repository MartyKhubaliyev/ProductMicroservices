using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Repositories;
using ProductService.API.Producer;
using ProductService.API.Producer.Enums;

namespace ProductService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IRepository<Product> _productRepository;
        private readonly KafkaProducer _kafkaProducer;

        public ProductController(IRepository<Product> productRepository, KafkaProducer kafkaProducer)
        {
            _productRepository = productRepository;
            _kafkaProducer = kafkaProducer;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _productRepository.GetAllAsync();
            return Ok(products);
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            await _productRepository.AddAsync(product);

            await _kafkaProducer.ProduceProductEventAsync(product.Id, EventType.Created, JsonConvert.SerializeObject(product));

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        // PUT: api/Product
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Category = product.Category;

            await _productRepository.UpdateAsync(existingProduct);

            await _kafkaProducer.ProduceProductEventAsync(product.Id, EventType.Updated, JsonConvert.SerializeObject(product));

            return NoContent();
        }
        
        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _kafkaProducer.ProduceProductEventAsync(product.Id, EventType.Deleted);

            await _productRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
