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
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Moq;
using System.Linq;

namespace P3AddNewFunctionalityDotNetCore.Tests.Unit
{
    [Collection("Sequential")]
    public class ProductServiceUnitTests
    {
        IStringLocalizerFactory _factory;
        IStringLocalizer<ProductService> _localizer;

        public ProductServiceUnitTests()
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

        // Seed data for moq database
        private List<Product> DBSeedData = new List<Product>()
        {
            new Product()
            {
                Id = 1,
                Name = "Dummy product",
                Price = 159.99,
                Quantity = 25,
                Description = "Dummy description",
                Details = "Dummy details"
            },
            new Product()
            {
                Id = 2,
                Name = "Another dummy product",
                Price = 259.99,
                Quantity = 15,
                Description = "Another dummy description",
                Details = "Another dummy details"
            },
            new Product()
            {
                Id = 3,
                Name = "Last dummy product",
                Price = 359.99,
                Quantity = 50,
                Description = "Last dummy description",
                Details = "Last dummy details"
            },
        };

        [Fact]
        public void CreateProduct()
        {
            var dbListProducts = new List<Product>();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(dbListProducts.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(dbListProducts.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(dbListProducts.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => dbListProducts.AsQueryable().GetEnumerator());
            mockSet.Setup(m => m.Add(It.IsAny<Product>())).Callback<Product>((entity) => dbListProducts.Add(entity));

            var mockContext = new Mock<P3Referential>(GetOptions());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);

            using (var context = mockContext.Object)
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
                mockSet.Verify(m => m.Add(It.IsAny<Product>()), Times.Exactly(3));
                mockContext.Verify(m => m.SaveChanges(), Times.Exactly(3));

                Assert.NotNull(dbListProducts.Find(x => x.Name == NewSeedData[0].Name));
                Assert.NotNull(dbListProducts.Find(x => x.Name == NewSeedData[1].Name));
                Assert.NotNull(dbListProducts.Find(x => x.Name == NewSeedData[2].Name));
            }
        }

        [Fact]
        public void DeleteProduct()
        {
            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(DBSeedData.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(DBSeedData.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(DBSeedData.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => DBSeedData.AsQueryable().GetEnumerator());
            mockSet.Setup(m => m.Remove(It.IsAny<Product>())).Callback<Product>((entity) => DBSeedData.Remove(entity));

            var mockContext = new Mock<P3Referential>(GetOptions());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);

            using (var context = mockContext.Object)
            {
                // Arrange
                Cart cart = new Cart();
                IProductRepository productRepository = new ProductRepository(context);
                IOrderRepository orderRepository = new OrderRepository(context);
                IProductService productService = new ProductService(cart, productRepository, orderRepository, _localizer);

                // Act
                int idToDelete = 3;
                productService.DeleteProduct(idToDelete);

                List<Product> products = productService.GetAllProducts();

                // Assert
                mockSet.Verify(m => m.Remove(It.IsAny<Product>()), Times.Once());
                mockContext.Verify(m => m.SaveChanges(), Times.Once());

                Assert.Equal(2, products.Count);
                Assert.Equal("Dummy product", products[0].Name);
                Assert.Equal("Another dummy product", products[1].Name);
                Assert.False(products.Any(x => x.Id == idToDelete));
            }
        }

        #region Check model errors

        [Fact]
        public void MissingName()
        {
            var mockContext = new Mock<P3Referential>(GetOptions());

            using (var context = mockContext.Object)
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
            var mockContext = new Mock<P3Referential>(GetOptions());

            using (var context = mockContext.Object)
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
            var mockContext = new Mock<P3Referential>(GetOptions());

            using (var context = mockContext.Object)
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
            var mockContext = new Mock<P3Referential>(GetOptions());

            using (var context = mockContext.Object)
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
            var mockContext = new Mock<P3Referential>(GetOptions());

            using (var context = mockContext.Object)
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
            var mockContext = new Mock<P3Referential>(GetOptions());

            using (var context = mockContext.Object)
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

