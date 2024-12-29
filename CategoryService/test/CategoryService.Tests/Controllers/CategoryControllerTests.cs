using CategoryService.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Repositories;

namespace CategoryService.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<IRepository<Category>> _mockRepository;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockRepository = new Mock<IRepository<Category>>();
            _controller = new CategoryController(_mockRepository.Object, new API.Producer.KafkaProducer());
        }

        [Fact]
        public async Task GetCategories_ShouldReturnOkResult_WithListOfCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category1"},
                new Category { Id = 2, Name = "Category2"}
            };
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }
    }
}
