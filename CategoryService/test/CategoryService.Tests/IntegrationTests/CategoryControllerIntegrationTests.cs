using CategoryService.API.Controllers;
using CategoryService.API.Producer;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.DbContexts;
using ProductManagement.Infrastructure.Repositories;

namespace CategoryService.Tests.IntegrationTests
{
    public class CategoryControllerIntegrationTests
    {
        private readonly AppDbContext _context;
        private readonly CategoryController _controller;

        public CategoryControllerIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new AppDbContext(options);

            var repository = new CategoryRepository(_context);
            _controller = new CategoryController(repository, new KafkaProducer());
        }

        [Fact]
        public async Task AddCategory_ShouldAddCategory_ToDatabase()
        {
            // Arrange
            var category = new Category { Name = "New Category" };

            // Act
            var result = await _controller.Post(category);

            // Assert
            var addedCategory = await _context.Categories.FirstOrDefaultAsync();
            Assert.NotNull(addedCategory);
            Assert.Equal(category.Name, addedCategory.Name);
        }
    }
}
