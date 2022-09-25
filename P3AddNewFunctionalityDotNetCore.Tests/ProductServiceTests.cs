using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {
        IStringLocalizerFactory _factory;
        IStringLocalizer<ProductService> _localizer;
        private DbContextOptions<P3Referential> GetOptions()
        {
            return new DbContextOptionsBuilder<P3Referential>()
            .UseInMemoryDatabase("ProductServiceRead" + Guid.NewGuid().ToString())
            .Options;
        }

        public ProductServiceTests()
        {
            var localizationOptions = Options.Create(new LocalizationOptions());
            this._factory = new ResourceManagerStringLocalizerFactory(localizationOptions, NullLoggerFactory.Instance);
            this._localizer = new StringLocalizer<ProductService>(_factory);
        }

        [Fact]
        public void CheckProductModelErrors()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // 1st test
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Name = null;
                product.Price = null;
                product.Stock = null;

                // Act
                List<string> modalErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("MissingName", modalErrors);
                Assert.Contains("MissingPrice", modalErrors);
                Assert.Contains("MissingQuantity", modalErrors);

                // 2nd test
                // Arrange
                product = new ProductViewModel();
                product.Price = "Not a number";
                product.Stock = "5.2";

                // Act
                modalErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("PriceNotANumber", modalErrors);
                Assert.Contains("StockNotAnInteger", modalErrors);

                // 3rd test
                // Arrange
                product = new ProductViewModel();
                product.Price = "-5";
                product.Stock = "-8";

                // Act
                modalErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("PriceNotGreaterThanZero", modalErrors);
                Assert.Contains("StockNotGreaterThanZero", modalErrors);
            }
        }
    }
}