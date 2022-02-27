using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppUnitTest.Controllers;
using WebAppUnitTest.Models;
using WebAppUnitTest.Repositories;
using Xunit;

namespace WebAppUnitTest.Test.ApiControllerTests
{

    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> mockRepo;

        private readonly ProductsApiController apiController;

        private readonly List<Product> products;

        public ProductApiControllerTest()
        {
            mockRepo = new Mock<IRepository<Product>>();
            apiController = new ProductsApiController(mockRepo.Object);
            products = new List<Product>
            {
                new Product{Id = 1 , Name = "Jean", Price = 250, Stock = 20, Color = "Red"},
                new Product{Id = 2 , Name = "Jacket", Price = 180, Stock = 24, Color = "Blue"},
                new Product{Id = 3 , Name = "Skirt", Price = 150, Stock = 15, Color = "Green"},
            };
        }

        [Fact]
        public async Task GetProduct_ActionExecutes_OkWithProduct()
        {

            mockRepo.Setup(I => I.GetAll()).ReturnsAsync(products);

            var result = await apiController.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal(3, returnedProducts.ToList().Count);
        }

        [Theory]
        [InlineData(-1)]
        public async Task GetProduct_IdIsInvalid_ReturnNotFound(int productId)
        {
            Product product = null;

            mockRepo.Setup(I => I.GetById(productId)).ReturnsAsync(product);

            var result = await apiController.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(1)]
        public async Task GetProduct_IdValid_OkResult(int productId)
        {
            Product product = products.Find(I => I.Id == productId);

            mockRepo.Setup(I => I.GetById(productId)).ReturnsAsync(product);

            var result = await apiController.GetProduct(productId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedProduct = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(productId, returnedProduct.Id);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_IdNotEqualProduct_ReturnsBadRequest(int productId)
        {
            Product product = products.Find(I => I.Id == productId);
            var result = apiController.PutProduct(2, product);
            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_ActionExecutes_ReturnsNoContent(int productId)
        {
            Product product = products.Find(I => I.Id == productId);

            mockRepo.Setup(I => I.Update(product));

            var result = apiController.PutProduct(productId, product);

            mockRepo.Verify(x => x.Update(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }


        [Theory]
        [InlineData(1)]
        public async Task Post_Product_ActionExecutes_ReturnsCreatedAtAction(int productId)
        {
            var product = products.First();
            mockRepo.Setup(I => I.Create(product)).Returns(Task.CompletedTask);

            var result = await apiController.PostProduct(product);

            var createdAction = Assert.IsType<CreatedAtActionResult>(result);

            mockRepo.Verify(I => I.Create(It.IsAny<Product>()), Times.Once);
            Assert.Equal("GetProduct", createdAction.ActionName);

        }

        [Theory]
        [InlineData(0)]
        public async Task Delete_InvalidId_ReturnNotFound(int productId)
        {
            Product product = null;
            mockRepo.Setup(I => I.GetById(productId)).ReturnsAsync(product);

            var resultNotFound = await apiController.DeleteProduct(productId);

            Assert.IsType<NotFoundResult>(resultNotFound);
            // if it would be returning a class , then we would have to take resultNotFound.Result
        }


        [Theory]
        [InlineData(0)]
        public async Task Delete_ActionExecutes_ReturnNotContent(int productId)
        {
            var product = products.First();

            mockRepo.Setup(I => I.Delete(product));
            mockRepo.Setup(I => I.GetById(productId)).ReturnsAsync(product);

            var noContentResult = await apiController.DeleteProduct(productId);

            mockRepo.Verify(I => I.Delete(product), Times.Once);

            Assert.IsType<NoContentResult>(noContentResult);

        }

    }
}