using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Order;
using FluentAssertions;
using LINQtoCSV;
using NUnit.Framework;

namespace BigCommerceAccessTests.Orders
{
	public class OrderTests
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
		public void GetOrders()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.Config );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -400 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrdersAsync()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.Config );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -200 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void OrdersNotLoaded_IncorrectApiKey()
		{
			var config = new BigCommerceConfig( this.Config.ShopName, this.Config.UserName, "blabla" );
			var service = this.BigCommerceFactory.CreateOrdersService( config );
			IEnumerable< BigCommerceOrder > orders = null;
			try
			{
				orders = service.GetOrders( DateTime.UtcNow.AddDays( -200 ), DateTime.UtcNow );
			}
			catch( WebException )
			{
				orders.Should().BeNull();
			}
		}

		[ Test ]
		public void OrdersNotLoaded_IncorrectShopName()
		{
			var config = new BigCommerceConfig( "blabla", this.Config.UserName, this.Config.ApiKey );
			var service = this.BigCommerceFactory.CreateOrdersService( config );
			IEnumerable< BigCommerceOrder > orders = null;
			try
			{
				orders = service.GetOrders( DateTime.UtcNow.AddDays( -200 ), DateTime.UtcNow );
			}
			catch( WebException )
			{
				orders.Should().BeNull();
			}
		}

		[ Test ]
		public void OrdersNotLoaded_IncorrectUserName()
		{
			var config = new BigCommerceConfig( this.Config.ShopName, "blabla", this.Config.ApiKey );
			var service = this.BigCommerceFactory.CreateOrdersService( config );
			IEnumerable< BigCommerceOrder > orders = null;
			try
			{
				orders = service.GetOrders( DateTime.UtcNow.AddDays( -200 ), DateTime.UtcNow );
			}
			catch( WebException )
			{
				orders.Should().BeNull();
			}
		}
	}
}