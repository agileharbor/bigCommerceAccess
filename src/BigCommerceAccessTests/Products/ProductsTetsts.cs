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

			products.Count.Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetProductsAsync()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );
			var products = await service.GetProductsAsync();

			products.Count.Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void ProductVQuantityUpdated()
		{
			var service = this.BigCommerceFactory.CreateProductsService( this.Config );

			var productToUpdate = new BigCommerceProduct { Id = 74, Quantity = "55" };
			service.UpdateProducts( new List< BigCommerceProduct > { productToUpdate } );
		}
	}
}