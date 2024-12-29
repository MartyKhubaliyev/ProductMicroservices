using Microsoft.EntityFrameworkCore;
using ProductService.API.Controllers;
using ProductService.API.Producer;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.DbContexts;
using ProductManagement.Infrastructure.Repositories;

namespace ProductService.Tests.IntegrationTests
{
    public class ProductControllerIntegrationTests
    {
        private readonly AppDbContext _context;
        private readonly ProductController _controller;

        public ProductControllerIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new AppDbContext(options);

            var repository = new ProductRepository(_context);
            _controller = new ProductController(repository, new KafkaProducer());
        }

        [Fact]
        public async Task AddProduct_ShouldAddProduct_ToDatabase()
        {
            // Arrange
            var product = new Product { Name = "New Product", Category = 1};

            // Act
            var result = await _controller.Post(product);

            // Assert
            var addedProduct = await _context.Products.FirstOrDefaultAsync();
            Assert.NotNull(addedProduct);
            Assert.Equal(product.Name, addedProduct.Name);
        }
    }
}
