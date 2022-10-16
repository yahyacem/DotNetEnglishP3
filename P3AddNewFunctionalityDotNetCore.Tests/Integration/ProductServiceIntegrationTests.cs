using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System.Collections.Generic;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests.Integration
{
    [Collection("Sequential")]
    public class ProductServiceIntegrationTests
    {
        IStringLocalizerFactory _factory;
        IStringLocalizer<ProductService> _localizer;

        public ProductServiceIntegrationTests()
        {
            var localizationOptions = Options.Create(new LocalizationOptions());
            this._factory = new ResourceManagerStringLocalizerFactory(localizationOptions, NullLoggerFactory.Instance);
            this._localizer = new StringLocalizer<ProductService>(_factory);
        }

        private DbContextOptions<P3Referential> GetOptions()
        {
            return new DbContextOptionsBuilder<P3Referential>()
            .UseInMemoryDatabase("ProductServiceRead" + Guid.NewGuid().ToString())
            .Options;
        }

        [Fact]
        public void CreateProduct()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                List<ProductViewModel> NewSeedData = new List<ProductViewModel>()
                {
                    new ProductViewModel()
                    {
                        Name = "New product 1",
                        Price = "100",
                        Stock = "50",
                        Description = "Description for new product 1",
                        Details = "Details for new product 1"
                    },
                    new ProductViewModel()
                    {
                        Name = "New product 2",
                        Price = "200",
                        Stock = "75",
                        Description = "Description for new product 2",
                        Details = "Details for new product 2"
                    },
                    new ProductViewModel()
                    {
                        Name = "New product 3",
                        Price = "300",
                        Stock = "100",
                        Description = "Description for new product 3",
                        Details = "Details for new product 3"
                    },
                };

                // Act
                productService.SaveProduct(NewSeedData[0]);
                productService.SaveProduct(NewSeedData[1]);
                productService.SaveProduct(NewSeedData[2]);

                // Assert
                Assert.NotNull(productService.GetAllProducts().Find(x => x.Name == NewSeedData[0].Name));
                Assert.NotNull(productService.GetAllProducts().Find(x => x.Name == NewSeedData[1].Name));
                Assert.NotNull(productService.GetAllProducts().Find(x => x.Name == NewSeedData[2].Name));
            }
        }

        [Fact]
        public void DeleteProduct()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel()
                {
                    Name = "Dummy product",
                    Price = "400",
                    Stock = "10",
                    Description = "Dummy description",
                    Details = "Dummy details"
            };

                // Add product to list of products
                productService.SaveProduct(product);

                // Get new product object
                Product createdProduct = productService.GetAllProducts().Find(x => x.Name == product.Name);

                // Add it to cart
                cart.AddItem(createdProduct, 3);

                // Get cart line with new product
                CartLine cartLine = ((List<CartLine>)cart.Lines).Find(x => x.Product.Id == createdProduct.Id);


                // Act
                productService.DeleteProduct(createdProduct.Id);


                // Assert

                // Check if product has been deleted from list of products
                Assert.Null(productService.GetAllProducts().Find(x => x.Name == product.Name));

                // Check if product has been deleted from cart in every cart line
                Assert.Null(((List<CartLine>)cart.Lines).Find(x => x.Product.Id == createdProduct.Id));

                // Check if cart lines with product have been deleted
                Assert.DoesNotContain(cartLine, (List<CartLine>)cart.Lines);
            }
        }

        #region Check model errors

        [Fact]
        public void MissingName()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Name = null;

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("MissingName", modelErrors);
            }
        }

        [Fact]
        public void MissingPrice()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Price = null;

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("MissingPrice", modelErrors);
            }
        }

        [Fact]
        public void PriceNotANumber()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Price = "Not a number";

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("PriceNotANumber", modelErrors);
            }
        }

        [Fact]
        public void PriceNotGreaterThanZero()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Price = "-1";

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("PriceNotGreaterThanZero", modelErrors);
            }
        }

        [Fact]
        public void MissingQuantity()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Stock = null;

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("MissingQuantity", modelErrors);
            }
        }

        [Fact]
        public void StockNotAnInteger()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Stock = "0.2";

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("StockNotAnInteger", modelErrors);

                // Arrange
                product = new ProductViewModel();
                product.Stock = "Not an integer";

                // Act
                modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("StockNotAnInteger", modelErrors);

            }
        }

        [Fact]
        public void StockNotGreaterThanZero()
        {
            using (var context = new P3Referential(GetOptions()))
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                ProductViewModel product = new ProductViewModel();
                product.Stock = "-1";

                // Act
                List<string> modelErrors = productService.CheckProductModelErrors(product);

                // Assert
                Assert.Contains("StockNotGreaterThanZero", modelErrors);
            }
        }

        #endregion
    }
}

