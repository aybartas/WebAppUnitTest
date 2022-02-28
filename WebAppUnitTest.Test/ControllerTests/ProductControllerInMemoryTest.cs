using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppUnitTest.Controllers;
using WebAppUnitTest.Models;
using Xunit;

namespace WebAppUnitTest.Test.ControllerTests
{
    public class ProductControllerInMemoryTest : ProductControllerTest
    {
        public ProductControllerInMemoryTest()
        {
            SetContext(new DbContextOptionsBuilder<WebAppTestDbContext>().UseInMemoryDatabase("ProviderUnitTestDb").Options);
        }

        [Fact]
        public async  Task Create_ValidModelProduct_ReturnRedirectToActionWithSaveProduct()
        {
            using var context = new WebAppTestDbContext(contextOptions);

            var category = context.Categories.First();
            var newProduct = new Product { Name = "Blue Pencil", Price = 200, Stock = 100, CategoryId = category.Id };

            var controller = new ProductsController(context);

            var result = await controller.Create(newProduct);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);

            using var externalContext = new WebAppTestDbContext(contextOptions);

            var product = context.Products.FirstOrDefault(I => I.Name == newProduct.Name);

            Assert.Equal(newProduct.Name, product.Name);

        }

        [InlineData(1)]
        public async Task Delete_Category_CategoryIdExist_Deleted(int categoryId)
        {
            using (var context = new WebAppTestDbContext(contextOptions) )
            {
                var category = await context.Categories.FindAsync(categoryId);

                context.Categories.Remove(category);
                context.SaveChanges();
            }

            using (var context = new WebAppTestDbContext(contextOptions))
            {
                var products = await context.Products.Where(I => I.CategoryId == categoryId).ToListAsync();

                Assert.Empty(products);
            }
        }


    }
}
