using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Order;
using FluentAssertions;
using LINQtoCSV;
using Netco.Logging;
using NUnit.Framework;

namespace BigCommerceAccessTests.Orders
{
	public class OrderTests
	{
		private readonly IBigCommerceFactory BigCommerceFactory = new BigCommerceFactory();
		private BigCommerceConfig ConfigV2;
		private BigCommerceConfig ConfigV3;

		[ SetUp ]
		public void Init()
		{
			NetcoLogger.LoggerFactory = new ConsoleLoggerFactory();
			const string credentialsFilePath = @"..\..\Files\BigCommerceCredentials.csv";

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true } ).FirstOrDefault();

			if( testConfig != null )
			{
				this.ConfigV2 = new BigCommerceConfig( testConfig.ShopName, testConfig.UserName, testConfig.ApiKey );
				this.ConfigV3 = new BigCommerceConfig( testConfig.ShortShopName, testConfig.ClientId, testConfig.ClientSecret, testConfig.Token );
			}
		}

		[ Test ]
		public void GetOrdersV2()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.ConfigV2 );
			var orders = service.GetOrders( DateTime.UtcNow.AddMonths( -6 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrdersV2Async()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.ConfigV2 );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddMonths( -6 ), DateTime.UtcNow, CancellationToken.None );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetOrdersV3()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.ConfigV3 );
			var orders = service.GetOrders( DateTime.UtcNow.AddMonths( -6 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrdersV3Async()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.ConfigV3 );
			var orders = await service.GetOrdersAsync( new DateTime( 2018, 08, 16, 00, 16, 00, DateTimeKind.Utc ), new DateTime( 2018, 08, 16, 00, 16, 59, DateTimeKind.Utc ), CancellationToken.None );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrderV3Async()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.ConfigV3 );
			var order = await service.GetOrderAsync( 9648, CancellationToken.None );
			
			order.Should().NotBeNull();

			var createdDate = order.DateCreated.ToUniversalTime();
			var modified = order.DateModified.ToUniversalTime();
			var shipped = order.DateShipped.Value.ToUniversalTime();
		}

		[ Test ]
		public void OrdersNotLoaded_IncorrectApiKey()
		{
			var config = new BigCommerceConfig( this.ConfigV2.ShopName, this.ConfigV2.UserName, "blabla" );
			IEnumerable< BigCommerceOrder > orders = null;
			try
			{
				var service = this.BigCommerceFactory.CreateOrdersService( config );
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
			var config = new BigCommerceConfig( "blabla", this.ConfigV2.UserName, this.ConfigV2.ApiKey );
			IEnumerable< BigCommerceOrder > orders = null;
			try
			{
				var service = this.BigCommerceFactory.CreateOrdersService( config );
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
			var config = new BigCommerceConfig( this.ConfigV2.ShopName, "blabla", this.ConfigV2.ApiKey );
			IEnumerable< BigCommerceOrder > orders = null;
			try
			{
				var service = this.BigCommerceFactory.CreateOrdersService( config );
				orders = service.GetOrders( DateTime.UtcNow.AddDays( -200 ), DateTime.UtcNow );
			}
			catch( WebException )
			{
				orders.Should().BeNull();
			}
		}
	}
}