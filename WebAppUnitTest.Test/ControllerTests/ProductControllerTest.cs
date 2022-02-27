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

namespace WebAppUnitTest.Test.ControllerTests
{
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> mockRepo;

        private readonly ProductsController productsController;

        private List<Product> products;

        public ProductControllerTest()
        {
            mockRepo = new Mock<IRepository<Product>>();
            productsController = new ProductsController(mockRepo.Object);
            products = new List<Product>
            {
                new Product{Id = 1 , Name = "Jean", Price = 250, Stock = 20, Color = "Red"},
                new Product{Id = 2 , Name = "Jacket", Price = 180, Stock = 24, Color = "Blue"},
                new Product{Id = 3 , Name = "Skirt", Price = 150, Stock = 15, Color = "Green"},
            };
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnView()
        {
            // In this test method only action is been tested.
            // in index method real GetAllProducts is not executed because there is no setup on GetAllProducts
            // so default is been returned

            var result = await productsController.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnProducts()
        {
            mockRepo.Setup(repository => repository.GetAll()).ReturnsAsync(products);

            var result = await productsController.Index();

            var viewResult = Assert.IsType<ViewResult>(result); // validates type and returns viewresult

            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model); // does it return Product enumarable ? 

            Assert.Equal<int>(3, productList.Count());

        }

        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await productsController.Details(null);

            var redirectedResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirectedResult.ActionName);
        }

        [Fact]
        public async void Details_ProductNotFound_ReturnNotFound()
        {
            Product product = null;
            // When GetById method called with "0", null will be returned
            mockRepo.Setup(repository => repository.GetById(0)).ReturnsAsync(product);

            var result = await productsController.Details(0);

            var returned = Assert.IsType<NotFoundResult>(result);

            Assert.Equal(404, returned.StatusCode);

        }

        [Theory]
        [InlineData(1)]
        public async void Details_ProductExists_ReturnView(int productId)
        {
            Product product = products.Find(I => I.Id == productId);

            mockRepo.Setup(repository => repository.GetById(productId)).ReturnsAsync(product);

            var result = await productsController.Details(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            var result = productsController.Create();
            Assert.IsType<ViewResult>(result);

        }

        // Create post method
        [Fact]
        public async void Create_InvalidModel_ReturnView()
        {
            productsController.ModelState.AddModelError("Name", "Name is invalid");
            var result = await productsController.Create(products.First());

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Product>(viewResult.Model);
        }
        [Fact]
        public async void Create_Post_ValidModel_ReturnRedirectToAction()
        {
            // its not mocked because 
            var result = await productsController.Create(products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);

        }

        [Fact]
        public async void Create_Post_ValidModel_ProductCreated()
        {
            Product product = null;

            mockRepo.Setup(repo => repo.Create(It.IsAny<Product>()))
                .Callback<Product>(I => product = I);

            var result = await productsController.Create(products.First());

            mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);

            Assert.Equal(products.First().Id, product.Id);

        }

        [Fact]
        public async void Create_Post_InvalidModel_CreatemethodNotExecuted()
        {
            productsController.ModelState.AddModelError("Name", "Name is invalid");

            var result = await productsController.Create(products.First());

            mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);

        }

        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndex()
        {
            var result = await productsController.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

        }

        [Theory]
        [InlineData(1)]
        public async void Edit_ProductNotFound_ReturnNotFound(int productId)
        {
            Product product = null;

            mockRepo.Setup(repo => repo.GetById(It.IsAny<int>())).ReturnsAsync(product);

            var result = await productsController.Edit(productId);

            var redirectResult = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirectResult.StatusCode);

        }
        [Theory]
        [InlineData(1)]
        public async void Edit_ProductExists_ReturnView(int productId)
        {
            Product product = products.Find(I => I.Id == productId);

            mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await productsController.Edit(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var assignableProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(productId, assignableProduct.Id);

        }

        [Theory]
        [InlineData(1)]
        public void Edit_Post_IdsDoesntMatch_NotFound(int productId)
        {
            var result = productsController.Edit(2, products.Find(I => I.Id == productId));
            var actionresult = Assert.IsType<NotFoundResult>(result);
        }


        [Theory]
        [InlineData(1)]
        public void Edit_Post_ModelStateIsNotValid_ViewResult(int productId)
        {
            productsController.ModelState.AddModelError("Name", "Invalid name");
            var result = productsController.Edit(productId, products.Find(I => I.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)] // mocking isn't used since the scope of this test independent from update method 
        public void Edit_Post_ModelStateValid_ReturnRedirectToIndex(int productId)
        {
            var result = productsController.Edit(productId, products.Find(I => I.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public void Edit_Post_ModelStateValid_UpdateMethodExecutes(int productId)
        {
            var product = products.Find(I => I.Id == productId);

            //mockRepo.Setup(repo => repo.Update(product));

            productsController.Edit(productId, product);

            mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);

        }

        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            var result = await productsController.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(0)]
        public async void Delete_ProductNotFound_ReturnNotFound(int productId)
        {
            Product product = null;
            mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await productsController.Delete(productId);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecutes_ReturnProduct(int productId)
        {
            Product product = products.Find(I => I.Id == productId);

            mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await productsController.Delete(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)] // this test only covers return type this is why mocking not used
        public async void DeleteConfirmed_ActionExecutes_RedirectToIndex(int productId)
        {
            var result = await productsController.DeleteConfirmed(productId);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecutes(int productId)
        {
            var product = products.Find(I => I.Id == productId);

            mockRepo.Setup(repo => repo.Delete(product));

            await productsController.DeleteConfirmed(productId);

            mockRepo.Verify(repo => repo.Delete(It.IsAny<Product>()), Times.Once);

        }


    }
}
