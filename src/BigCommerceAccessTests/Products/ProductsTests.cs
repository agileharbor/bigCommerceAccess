using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using FluentAssertions;
using LINQtoCSV;
using Netco.Logging;
using NUnit.Framework;

namespace BigCommerceAccessTests.Products
{
	[ TestFixture ]
	public class ProductsTests
	{
		private readonly IBigCommerceFactory BigCommerceFactory = new BigCommerceFactory();
		private BigCommerceConfig ConfigV2;
		private BigCommerceConfig ConfigV3;
		private string testProductSku = "testSku1";
		private string testProductWithOptionsSku = "BW345";
		private string testProductOptionSku = "SKU-F665988D";

		[ SetUp ]
		public void Init()
		{
			NetcoLogger.LoggerFactory = new NullLoggerFactory();
			const string credentialsFilePath = @"..\..\Files\BigCommerceCredentials.csv";

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true } ).FirstOrDefault();

			if( testConfig != null )
			{
				this.ConfigV2 = new BigCommerceConfig( testConfig.ShopName, testConfig.UserName, testConfig.Token );
				this.ConfigV3 = new BigCommerceConfig( testConfig.ShortShopName, testConfig.ClientId, testConfig.ClientSecret, testConfig.ApiKey );
			}
		}

		[ Test ]
		public void GetProductsV2()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV2 );
			var products = service.GetProducts( true );

			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsV2Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV2 );
			var products = await service.GetProductsAsync( CancellationToken.None, true );

			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetProductsV3()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var products = service.GetProducts();

			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetProductsImagesV3()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var products = service.GetProducts();

			products.Count().Should().BeGreaterThan( 0 );
			var productWithImages = products.Where( pr => pr.ImageUrls != null && !string.IsNullOrWhiteSpace( pr.ImageUrls.StandardUrl ) );
			productWithImages.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsV3Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var products = await service.GetProductsAsync( CancellationToken.None );

			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsImagesV3Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var products = await service.GetProductsAsync( CancellationToken.None );
			
			products.Count().Should().BeGreaterThan( 0 );
			var productWithImages = products.Where( pr => pr.ImageUrls != null && !string.IsNullOrWhiteSpace( pr.ImageUrls.StandardUrl ) );
			productWithImages.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void ProductShouldHaveEnabledTrackingInventoryByProduct()
		{
			var product = this.GetProductBySkuV3( this.testProductSku );

			product.InventoryTracking.Should().Be( InventoryTrackingEnum.simple );
		}

		[ Test ]
		public void ProductShouldHaveEnabledTrackingInventoryByOption()
		{
			var product = this.GetProductBySkuV3( this.testProductWithOptionsSku );

			product.InventoryTracking.Should().Be( InventoryTrackingEnum.sku );
		}

		[ Test ]
		public void ProductsQuantityUpdatedV2()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV2 );

			var productToUpdate = new BigCommerceProduct { Id = 74, Quantity = "1" };
			service.UpdateProducts( new List< BigCommerceProduct > { productToUpdate } );
		}

		[ Test ]
		public async Task ProductsQuantityUpdatedV2Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV2 );

			var productToUpdate = new BigCommerceProduct { Id = 74, Quantity = "6" };
			await service.UpdateProductsAsync( new List< BigCommerceProduct > { productToUpdate }, CancellationToken.None );
		}

		[ Test ]
		public void ProductsQuantityUpdatedV3()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var testProduct = this.GetProductBySkuV3( this.testProductSku );
			var rand = new Random();
			int newProductQuantity = rand.Next( 1, 100 );

			if ( testProduct != null )
			{
				var productToUpdate = new BigCommerceProduct { Id = testProduct.Id, Quantity = newProductQuantity.ToString() };
				service.UpdateProducts( new List< BigCommerceProduct > { productToUpdate } );

				var updatedProduct = this.GetProductBySkuV3( this.testProductSku );
				updatedProduct.Quantity.Should().Be( newProductQuantity.ToString() );
			}
		}

		[ Test ]
		public async Task ProductsQuantityUpdatedV3Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var testProduct = this.GetProductBySkuV3( this.testProductSku );
			var rand = new Random();
			int newProductQuantity = rand.Next( 1, 100 );

			if ( testProduct != null )
			{
				var productToUpdate = new BigCommerceProduct { Id = testProduct.Id, Quantity = newProductQuantity.ToString() };
				await service.UpdateProductsAsync( new List< BigCommerceProduct > { productToUpdate }, CancellationToken.None );

				var updatedProduct = this.GetProductBySkuV3( this.testProductSku );
				updatedProduct.Quantity.Should().Be( newProductQuantity.ToString() );
			}
		}

		[ Test ]
		public void ProductOptionsQuantityUpdatedV2()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV2 );

			var productToUpdate = new BigCommerceProductOption { ProductId = 45, Id = 53, Quantity = "6" };
			service.UpdateProductOptions( new List< BigCommerceProductOption > { productToUpdate } );
		}

		[ Test ]
		public async Task ProductOptionsQuantityUpdatedV2Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV2 );

			var productToUpdate = new BigCommerceProductOption { ProductId = 45, Id = 53, Quantity = "6" };
			await service.UpdateProductOptionsAsync( new List< BigCommerceProductOption > { productToUpdate }, CancellationToken.None );
		}

		[ Test ]
		public void ProductOptionsQuantityUpdatedV3()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var testProduct = this.GetProductBySkuV3( testProductWithOptionsSku );
			var testProductOption = this.GetProductOptionBySku( testProduct, testProductOptionSku );
			var newOptionQuantity = new Random().Next( 1, 100 );

			var productToUpdate = new BigCommerceProductOption()
			{
				ProductId = testProduct.Id, 
				Id = testProductOption.Id, 
				Quantity = newOptionQuantity.ToString() 
			};

			service.UpdateProductOptions( new List< BigCommerceProductOption > { productToUpdate } );

			var updatedProduct = this.GetProductBySkuV3( testProductWithOptionsSku );
			var updatedProductOption = this.GetProductOptionBySku( updatedProduct, testProductOptionSku );
			updatedProductOption.Quantity.Should().Be( newOptionQuantity.ToString() );
		}

		[ Test ]
		public async Task ProductOptionsQuantityUpdatedV3Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var testProduct = this.GetProductBySkuV3( testProductWithOptionsSku );
			var testProductOption = this.GetProductOptionBySku( testProduct, testProductOptionSku );
			var newOptionQuantity = new Random().Next( 1, 100 );

			var productToUpdate = new BigCommerceProductOption()
			{
				ProductId = testProduct.Id, 
				Id = testProductOption.Id, 
				Quantity = newOptionQuantity.ToString() 
			};
			
			await service.UpdateProductOptionsAsync( new List< BigCommerceProductOption > { productToUpdate }, CancellationToken.None );
			
			var updatedProduct = this.GetProductBySkuV3( testProductWithOptionsSku );
			var updatedProductOption = this.GetProductOptionBySku( updatedProduct, testProductOptionSku );
			updatedProductOption.Quantity.Should().Be( newOptionQuantity.ToString() );
		}

		private BigCommerceProduct GetProductBySkuV3( string sku )
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var products = service.GetProducts();
			return products.FirstOrDefault( pr => pr.Sku != null && pr.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		private BigCommerceProduct GetProductByIdV3( int productId )
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.ConfigV3 );
			var products = service.GetProducts();
			return products.FirstOrDefault( pr => pr.Id == productId );
		}

		private BigCommerceProductOption GetProductOptionBySku( BigCommerceProduct product, string sku )
		{
			return product.ProductOptions.FirstOrDefault( o => o.Sku != null && o.Sku.ToLower().Equals( sku.ToLower() ) );
		}
	}
}