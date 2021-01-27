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
				var ordersWithinPage = ActionPolicies.Get( marker, compositeEndpoint ).Get( () =>
					this._webRequestServices.GetResponseByRelativeUrl< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2, compositeEndpoint, marker ) );
				this.CreateApiDelay( ordersWithinPage.Limits ).Wait(); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				this.GetOrdersProducts( ordersWithinPage.Response, marker );
				this.GetOrdersCoupons( ordersWithinPage.Response, marker );
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
				var ordersWithinPage = ActionPolicies.Get( marker, compositeEndpoint ).Get( () =>
					this._webRequestServices.GetResponseByRelativeUrl< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2_OAuth, compositeEndpoint, marker ) );
				this.CreateApiDelay( ordersWithinPage.Limits ).Wait(); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				this.GetOrdersProducts( ordersWithinPage.Response, marker );
				this.GetOrdersCoupons( ordersWithinPage.Response, marker );
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
				var ordersWithinPage = await ActionPolicies.GetAsync( marker, compositeEndpoint ).Get( async () =>
					await this._webRequestServices.GetResponseByRelativeUrlAsync< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2, compositeEndpoint, marker ) );
				await this.CreateApiDelay( ordersWithinPage.Limits, token ); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				await this.GetOrdersProductsAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				await this.GetOrdersCouponsAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
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
				var ordersWithinPage = await ActionPolicies.GetAsync( marker, compositeEndpoint ).Get( async () =>
					await this._webRequestServices.GetResponseByRelativeUrlAsync< List< BigCommerceOrder > >( BigCommerceCommand.GetOrdersV2_OAuth, compositeEndpoint, marker ) );
				await this.CreateApiDelay( ordersWithinPage.Limits, token ); //API requirement

				if( ordersWithinPage.Response == null )
					break;

				await this.GetOrdersProductsAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				await this.GetOrdersCouponsAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				await this.GetOrdersShippingAddressesAsync( ordersWithinPage.Response, ordersWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				orders.AddRange( ordersWithinPage.Response );
				if( ordersWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}

		#endregion

		#region Order products
		private void GetOrdersProducts( IEnumerable< BigCommerceOrder > orders, string marker )
		{
			foreach( var order in orders )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					if ( string.IsNullOrWhiteSpace( order.ProductsReference?.Url ) )
						break;

					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var products = ActionPolicies.Get( marker, endpoint ).Get( () =>
						this._webRequestServices.GetResponseByRelativeUrl< List< BigCommerceOrderProduct > >( order.ProductsReference.Url, endpoint, marker ) );
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
				for( var i = 1; i < int.MaxValue; i++ )
				{
					if ( string.IsNullOrWhiteSpace( order.ProductsReference?.Url ) )
						break;

					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var products = await ActionPolicies.GetAsync( marker, endpoint ).Get( async () =>
						await this._webRequestServices.GetResponseByRelativeUrlAsync< List< BigCommerceOrderProduct > >( order.ProductsReference.Url, endpoint, marker ) );
					await this.CreateApiDelay( products.Limits, token ); //API requirement

					if( products.Response == null )
						break;
					order.Products.AddRange( products.Response );
					if( products.Response.Count < RequestMaxLimit )
						break;
				}
			} );
		}
		#endregion

		#region Order Coupons
		private void GetOrdersCoupons( IEnumerable< BigCommerceOrder > orders, string marker )
		{
			foreach( var order in orders )
			{
				if ( string.IsNullOrWhiteSpace( order.CouponsReference?.Url ) )
					continue;

				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var coupons = ActionPolicies.Get( marker, endpoint ).Get( () =>
						this._webRequestServices.GetResponseByRelativeUrl< List< BigCommerceOrderCoupon > >( order.CouponsReference.Url, endpoint, marker ) );
					this.CreateApiDelay( coupons.Limits ).Wait(); //API requirement

					if( coupons.Response == null )
						break;
					order.Coupons.AddRange( coupons.Response );
					if( coupons.Response.Count < RequestMaxLimit )
						break;
				}
			}
		}

		private async Task GetOrdersCouponsAsync( IEnumerable< BigCommerceOrder > orders, bool isUnlimit, CancellationToken token, string marker )
		{
			var threadCount = isUnlimit ? MaxThreadsCount : 1;
			await orders.DoInBatchAsync( threadCount, async order =>
			{
				if ( string.IsNullOrWhiteSpace( order.CouponsReference?.Url ) )
					return;

				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var coupons = await ActionPolicies.GetAsync( marker, endpoint ).Get( async () =>
						await this._webRequestServices.GetResponseByRelativeUrlAsync< List< BigCommerceOrderCoupon > >( order.CouponsReference.Url, endpoint, marker ) );
					await this.CreateApiDelay( coupons.Limits, token ); //API requirement

					if( coupons.Response == null )
						break;
					order.Coupons.AddRange( coupons.Response );
					if( coupons.Response.Count < RequestMaxLimit )
						break;
				}
			} );
		}
		#endregion
		
		#region ShippingAddress
		private void GetOrdersShippingAddresses( IEnumerable< BigCommerceOrder > orders, string marker )
		{
			foreach( var order in orders )
			{
				if ( string.IsNullOrWhiteSpace( order.ShippingAddressesReference?.Url ) )
					continue;

				var addresses = ActionPolicies.Get( marker, order.ShippingAddressesReference.Url ).Get( () =>
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
				if ( string.IsNullOrWhiteSpace( order.ShippingAddressesReference?.Url ) )
					return;

				var addresses = await ActionPolicies.GetAsync( marker, order.ShippingAddressesReference.Url ).Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceShippingAddress > >( order.ShippingAddressesReference.Url, marker ) );
				order.ShippingAddresses = addresses.Response;
				await this.CreateApiDelay( addresses.Limits, token ); //API requirement
			} );
		}
		#endregion
	}
}