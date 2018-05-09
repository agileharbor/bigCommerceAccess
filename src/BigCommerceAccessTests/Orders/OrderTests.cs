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
			//NetcoLogger.LoggerFactory = new ConsoleLoggerFactory();
			//const string credentialsFilePath = @"..\..\Files\BigCommerceCredentials.csv";

			//var cc = new CsvContext();
			//var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true } ).FirstOrDefault();

			//if( testConfig != null )
			//{
			//	this.ConfigV2 = new BigCommerceConfig( testConfig.ShopName, testConfig.UserName, testConfig.ApiKey );
			//	this.ConfigV3 = new BigCommerceConfig( testConfig.ShortShopName, testConfig.ClientId, testConfig.ClientSecret, testConfig.Token );
			//}

			this.ConfigV2 = new BigCommerceConfig("store-lgq8il", "skuvault", "47666818d2389afc328cff8122ecca1fac2c1096");
			this.ConfigV3 = new BigCommerceConfig("lgq8il", "3jpmm2merakwmwd708q9c2smf8hqsnp", "l27ccs00t7rl983ty8yqtmos8bkefva", "g53o5wsce7ai9gomg687taicg7sxw32");
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
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddMonths( -6 ), DateTime.UtcNow, CancellationToken.None );

			orders.Count().Should().BeGreaterThan( 0 );
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