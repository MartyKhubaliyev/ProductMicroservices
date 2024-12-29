using Moq;
using ProductService.API.Controllers;
using ProductManagement.Domain.Repositories;
using ProductManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IRepository<Product>> _mockRepository;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockRepository = new Mock<IRepository<Product>>();
            _controller = new ProductController(_mockRepository.Object, new API.Producer.KafkaProducer());
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Category = 1},
                new Product { Id = 2, Name = "Product2", Category = 2}
            };
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }
    }
}
