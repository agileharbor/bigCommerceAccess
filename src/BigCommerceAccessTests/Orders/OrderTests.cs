using System;
using System.Linq;
using BigCommerceAccess;
using BigCommerceAccess.Models.Configuration;
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
		public void OrdersFilteredFulfillmentStatusDateLoaded()
		{
			var service = this.BigCommerceFactory.CreateOrdersService( this.Config );
			service.GetOrders( DateTime.UtcNow.AddDays( -200 ), DateTime.UtcNow );

			//orders.Count.Should().Be(1);
		}
	}
}