using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Address;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Order;
using BigCommerceAccess.Services;
using CuttingEdge.Conditions;
using Netco.Extensions;

namespace BigCommerceAccess
{
	public class BigCommerceOrdersService: BigCommerceServiceBase, IBigCommerceOrdersService
	{
		private readonly WebRequestServices _webRequestServices;
		private readonly APIVersion _apiVersion;

		public BigCommerceOrdersService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();
			this._webRequestServices = new WebRequestServices( config, this.GetMarker() );
			this._apiVersion = config.GetAPIVersion();
		}

		#region Orders
		public List< BigCommerceOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			return this._apiVersion == APIVersion.V2 ? this.GetOrdersForV2( dateFrom, dateTo ) : this.GetOrdersForV3( dateFrom, dateTo );
		}

		public Task< List< BigCommerceOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			return this._apiVersion == APIVersion.V2 ? this.GetOrdersForV2Async( dateFrom, dateTo, token ) : this.GetOrdersForV3Async( dateFrom, dateTo, token );
		}

		public List< BigCommerceOrder > GetOrdersForV2( DateTime dateFrom, DateTime dateTo )
		{
			var mainEndpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = new List< BigCommerceOrder >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var ordersWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2, compositeEndpoint, marker ) );
				this.CreateApiDelay( ordersWithinPage.Limits ).Wait(); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				this.GetOrdersProducts( ordersWithinPage.Response, marker );
				this.GetOrdersShippingAddresses( ordersWithinPage.Response, marker );
				orders.AddRange( ordersWithinPage.Response );
				if( ordersWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}

		public List< BigCommerceOrder > GetOrdersForV3( DateTime dateFrom, DateTime dateTo )
		{
			var mainEndpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = new List< BigCommerceOrder >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var ordersWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2_OAuth, compositeEndpoint, marker ) );
				this.CreateApiDelay( ordersWithinPage.Limits ).Wait(); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				this.GetOrdersProducts( ordersWithinPage.Response, marker );
				this.GetOrdersShippingAddresses( ordersWithinPage.Response, marker );
				orders.AddRange( ordersWithinPage.Response );
				if( ordersWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}

		private async Task< List< BigCommerceOrder > > GetOrdersForV2Async( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			var mainEndpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = new List< BigCommerceOrder >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var ordersWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2, compositeEndpoint, marker ) );
				await this.CreateApiDelay( ordersWithinPage.Limits, token ); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				await this.GetOrdersProductsAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				await this.GetOrdersShippingAddressesAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				orders.AddRange( ordersWithinPage.Response );
				if( ordersWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}

		private async Task< List< BigCommerceOrder > > GetOrdersForV3Async( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			var mainEndpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = new List< BigCommerceOrder >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var ordersWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2_OAuth, compositeEndpoint, marker ) );
				await this.CreateApiDelay( ordersWithinPage.Limits, token ); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				await this.GetOrdersProductsAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				await this.GetOrdersShippingAddressesAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				orders.AddRange( ordersWithinPage.Response );
				if( ordersWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}

		#endregion

		#region Orders
		public Task< BigCommerceOrder > GetOrderAsync( int orderId, CancellationToken token )
		{
			return this._apiVersion == APIVersion.V2 ? this.GetOrderForV2Async( orderId, token ) : this.GetOrderForV3Async( orderId, token );
		}

		private async Task< BigCommerceOrder > GetOrderForV2Async( int orderId, CancellationToken token )
		{
			var mainEndpoint = ParamsBuilder.CreateOrderParams( orderId );
			var marker = this.GetMarker();

			var ordersWithinPage = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< BigCommerceOrder >( BigCommerceCommand.GetOrdersV2, mainEndpoint, marker ) );
			await this.CreateApiDelay( ordersWithinPage.Limits, token ); //API requirement

			if( ordersWithinPage.Response == null )
				return null;

			await this.GetOrderProductsAsync( ordersWithinPage.Response, token, marker );
			await this.GetOrderShippingAddressesAsync( ordersWithinPage.Response, token, marker );

			return ordersWithinPage.Response;
		}

		private async Task< BigCommerceOrder > GetOrderForV3Async( int orderId, CancellationToken token )
		{
			var mainEndpoint = ParamsBuilder.CreateOrderParams( orderId );
			var marker = this.GetMarker();

			var ordersWithinPage = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< BigCommerceOrder >( BigCommerceCommand.GetOrdersV2_OAuth, mainEndpoint, marker ) );
			await this.CreateApiDelay( ordersWithinPage.Limits, token ); //API requirement

			if( ordersWithinPage.Response == null )
				return null;

			await this.GetOrderProductsAsync( ordersWithinPage.Response, token, marker );
			await this.GetOrderShippingAddressesAsync( ordersWithinPage.Response, token, marker );

			return ordersWithinPage.Response;
		}

		#endregion

		#region Order products
		private void GetOrdersProducts( IEnumerable< BigCommerceOrder > orders, string marker )
		{
			foreach( var order in orders )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var products = ActionPolicies.Get.Get( () =>
						this._webRequestServices.GetResponse< List< BigCommerceOrderProduct > >( order.ProductsReference.Url, endpoint, marker ) );
					this.CreateApiDelay( products.Limits ).Wait(); //API requirement

					if( products.Response == null )
						break;
					order.Products.AddRange( products.Response );
					if( products.Response.Count < RequestMaxLimit )
						break;
				}
			}
		}

		private async Task GetOrdersProductsAsync( IEnumerable< BigCommerceOrder > orders, bool isUnlimit, CancellationToken token, string marker )
		{
			var threadCount = isUnlimit ? MaxThreadsCount : 1;
			await orders.DoInBatchAsync( threadCount, async order =>
			{
				await this.GetOrderProductsAsync( order, token, marker );
			} );
		}

		private async Task GetOrderProductsAsync( BigCommerceOrder order, CancellationToken token, string marker )
		{
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var products = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceOrderProduct > >( order.ProductsReference.Url, endpoint, marker ) );
				await this.CreateApiDelay( products.Limits, token ); //API requirement

				if( products.Response == null )
					break;
				order.Products.AddRange( products.Response );
				if( products.Response.Count < RequestMaxLimit )
					break;
			}
		}
		#endregion
		
		#region ShippingAddress
		private void GetOrdersShippingAddresses( IEnumerable< BigCommerceOrder > orders, string marker )
		{
			foreach( var order in orders )
			{
				var addresses = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceShippingAddress > >( order.ShippingAddressesReference.Url, marker ) );
				order.ShippingAddresses = addresses.Response;
				this.CreateApiDelay( addresses.Limits ).Wait(); //API requirement
			}
		}

		private async Task GetOrdersShippingAddressesAsync( IEnumerable< BigCommerceOrder > orders, bool isUnlimit, CancellationToken token, string marker )
		{
			var threadCount = isUnlimit ? MaxThreadsCount : 1;
			await orders.DoInBatchAsync( threadCount, async order =>
			{
				await this.GetOrderShippingAddressesAsync( order, token, marker );
			} );
		}

		private async Task GetOrderShippingAddressesAsync( BigCommerceOrder order, CancellationToken token, string marker )
		{
			var addresses = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< List< BigCommerceShippingAddress > >( order.ShippingAddressesReference.Url, marker ) );
			order.ShippingAddresses = addresses.Response;
			await this.CreateApiDelay( addresses.Limits, token ); //API requirement
		}
		#endregion
	}
}