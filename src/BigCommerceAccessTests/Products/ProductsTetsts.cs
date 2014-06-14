using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using FluentAssertions;
using LINQtoCSV;
using NUnit.Framework;

namespace BigCommerceAccessTests.Products
{
	[ TestFixture ]
	public class ProductsTetsts
	{
		private readonly IBigCommerceFactory BigCommerceFactory = new BigCommerceFactory();
		private BigCommerceConfig Config;

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\BigCommerceCredentials.csv";

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();

			if( testConfig != null )
				this.Config = new BigCommerceConfig( testConfig.ShopName, testConfig.UserName, testConfig.ApiKey );
		}

		[ Test ]
		public void GetProducts()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );
			var products = service.GetProducts();

			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsAsync()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );
			var products = await service.GetProductsAsync();

			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void ProductsQuantityUpdated()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );

			var productToUpdate = new BigCommerceProduct { Id = 75, Quantity = "6" };
			service.UpdateProducts( new List< BigCommerceProduct > { productToUpdate } );
		}

		[ Test ]
		public async Task ProductsQuantityUpdatedAsync()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );

			var productToUpdate = new BigCommerceProduct { Id = 75, Quantity = "6" };
			await service.UpdateProductsAsync( new List< BigCommerceProduct > { productToUpdate } );
		}

		[ Test ]
		public void ProductOptionsQuantityUpdated()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );

			var productToUpdate = new BigCommerceProductOption { ProductId = 75, Id = 4, Quantity = "6" };
			service.UpdateProductOptions( new List< BigCommerceProductOption > { productToUpdate } );
		}

		[ Test ]
		public async Task ProductOptionsQuantityUpdatedAsync()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );

			var productToUpdate = new BigCommerceProductOption { ProductId = 75, Id = 4, Quantity = "6" };
			await service.UpdateProductOptionsAsync( new List< BigCommerceProductOption > { productToUpdate } );
		}

	}
}