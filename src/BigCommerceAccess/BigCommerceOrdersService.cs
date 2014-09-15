using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models;
using BigCommerceAccess.Models.Address;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Order;
using BigCommerceAccess.Services;
using CuttingEdge.Conditions;

namespace BigCommerceAccess
{
	public class BigCommerceOrdersService : BigCommerceServiceBase, IBigCommerceOrdersService
	{
		private readonly WebRequestServices _webRequestServices;

		public BigCommerceOrdersService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this._webRequestServices = new WebRequestServices( config );
		}

		#region Orders
		public IEnumerable< BigCommerceOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			IList< BigCommerceOrder > orders;
			var endpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var ordersCount = this.GetOrdersCount();

			if( ordersCount > RequestMaxLimit )
				orders = this.CollectOrdersFromAllPages( endpoint, ordersCount );
			else
				orders = this.CollectOrdersFromSinglePage( endpoint );

			orders = orders ?? new List<BigCommerceOrder>();

			this.GetOrdersProducts( orders );
			this.GetOrdersShippingAddresses( orders );

			return orders;
		}

		public async Task< IEnumerable< BigCommerceOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			IList< BigCommerceOrder > orders;
			var endpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var ordersCount = await this.GetOrdersCountAsync();

			if( ordersCount > RequestMaxLimit )
				orders = await this.CollectOrdersFromAllPagesAsync( endpoint, ordersCount );
			else
				orders = await this.CollectOrdersFromSinglePageAsync( endpoint );

			orders = orders ?? new List< BigCommerceOrder >();

			await this.GetOrdersProductsAsync( orders );
			await this.GetOrdersShippingAddressesAsync( orders );

			return orders;
		}

		private IList< BigCommerceOrder > CollectOrdersFromAllPages( string mainEndpoint, int ordersCount )
		{
			var pagesCount = this.CalculatePagesCount( ordersCount );
			var orders = new List< BigCommerceOrder >();

			for( var i = 0; i < pagesCount; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i + 1, RequestMaxLimit ) ) );

				ActionPolicies.Get.Do( () =>
				{
					var ordersWithinPage = this._webRequestServices.GetResponse< IList< BigCommerceOrder > >( BigCommerceCommand.GetOrders, compositeEndpoint );

					if( ordersWithinPage != null )
						orders.AddRange( ordersWithinPage );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}

			return orders;
		}

		private async Task< IList< BigCommerceOrder > > CollectOrdersFromAllPagesAsync( string mainEndpoint, int ordersCount )
		{
			var pagesCount = this.CalculatePagesCount( ordersCount );
			var orders = new List< BigCommerceOrder >();

			for( var i = 0; i < pagesCount; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i + 1, RequestMaxLimit ) ) );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var ordersWithinPage = await this._webRequestServices.GetResponseAsync< IList< BigCommerceOrder > >( BigCommerceCommand.GetOrders, compositeEndpoint );

					if( ordersWithinPage != null )
						orders.AddRange( ordersWithinPage );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}

			return orders;
		}

		private IList< BigCommerceOrder > CollectOrdersFromSinglePage( string endpoint )
		{
			IList< BigCommerceOrder > orders = null;

			ActionPolicies.Get.Do( () =>
			{
				orders = this._webRequestServices.GetResponse< IList< BigCommerceOrder > >( BigCommerceCommand.GetOrders, endpoint );

				//API requirement
				this.CreateApiDelay().Wait();
			} );

			return orders;
		}

		private async Task< IList< BigCommerceOrder > > CollectOrdersFromSinglePageAsync( string endpoint )
		{
			IList< BigCommerceOrder > orders = null;

			await ActionPolicies.GetAsync.Do( async () =>
			{
				orders = await this._webRequestServices.GetResponseAsync< IList< BigCommerceOrder > >( BigCommerceCommand.GetOrders, endpoint );

				//API requirement
				this.CreateApiDelay().Wait();
			} );

			return orders;
		}
		#endregion
		
		#region Order products
		private void GetOrdersProducts( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				var o = order;
				ActionPolicies.Get.Do( () =>
				{
					o.Products = this._webRequestServices.GetResponse< IList< BigCommerceOrderProduct > >( o.ProductsReference.Url );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}
		}

		private async Task GetOrdersProductsAsync( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				var o = order;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					o.Products = await this._webRequestServices.GetResponseAsync< IList< BigCommerceOrderProduct > >( o.ProductsReference.Url );

					//API requirement
					this.CreateApiDelay().Wait();
				} );

			}
		}
		#endregion

		#region ShippingAddress
		private void GetOrdersShippingAddresses( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				var o = order;
				ActionPolicies.Get.Do( () =>
				{
					o.ShippingAddresses = this._webRequestServices.GetResponse< IList< BigCommerceShippingAddress > >( o.ProductsReference.Url );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}
		}

		private async Task GetOrdersShippingAddressesAsync( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				var o = order;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					o.ShippingAddresses = await this._webRequestServices.GetResponseAsync< IList< BigCommerceShippingAddress > >( o.ProductsReference.Url );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}
		}
		#endregion

		#region OrdersCount
		private int GetOrdersCount()
		{
			var count = 0;
			ActionPolicies.Get.Do( () =>
			{
				count = this._webRequestServices.GetResponse< BigCommerceItemsCount >( BigCommerceCommand.GetOrdersCount, ParamsBuilder.EmptyParams ).Count;

				//API requirement
				this.CreateApiDelay().Wait();
			} );
			return count;
		}

		private async Task< int > GetOrdersCountAsync()
		{
			var count = 0;
			await ActionPolicies.GetAsync.Do( async () =>
			{
				count = ( await this._webRequestServices.GetResponseAsync< BigCommerceItemsCount >( BigCommerceCommand.GetOrdersCount, ParamsBuilder.EmptyParams ) ).Count;

				//API requirement
				this.CreateApiDelay().Wait();
			} );
			return count;
		}
		#endregion
	}
}