using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Address;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Order;
using BigCommerceAccess.Services;
using CuttingEdge.Conditions;

namespace BigCommerceAccess
{
	public class BigCommerceOrdersService: BigCommerceServiceBase, IBigCommerceOrdersService
	{
		private readonly WebRequestServices _webRequestServices;

		public BigCommerceOrdersService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();
			this._webRequestServices = new WebRequestServices( config );
		}

		#region Orders
		public List< BigCommerceOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var endpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = this.CollectOrdersFromAllPages( endpoint );

			return orders;
		}

		public async Task< List< BigCommerceOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var endpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = await this.CollectOrdersFromAllPagesAsync( endpoint );

			return orders;
		}

		private List< BigCommerceOrder > CollectOrdersFromAllPages( string mainEndpoint )
		{
			var orders = new List< BigCommerceOrder >();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var ordersWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceOrder > >( BigCommerceCommand.GetOrders, compositeEndpoint ) );
				this.CreateApiDelay().Wait(); //API requirement

				if( ordersWithinPage == null )
					break;

				this.GetOrdersProducts( ordersWithinPage );
				this.GetOrdersShippingAddresses( ordersWithinPage );
				orders.AddRange( ordersWithinPage );
				if( ordersWithinPage.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}

		private async Task< List< BigCommerceOrder > > CollectOrdersFromAllPagesAsync( string mainEndpoint )
		{
			var orders = new List< BigCommerceOrder >();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var compositeEndpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var ordersWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceOrder > >( BigCommerceCommand.GetOrders, compositeEndpoint ) );
				await this.CreateApiDelay(); //API requirement

				if( ordersWithinPage == null )
					break;

				await this.GetOrdersProductsAsync( ordersWithinPage );
				await this.GetOrdersShippingAddressesAsync( ordersWithinPage );
				orders.AddRange( ordersWithinPage );
				if( ordersWithinPage.Count < RequestMaxLimit )
					break;
			}

			return orders;
		}
		#endregion

		#region Order products
		private void GetOrdersProducts( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var products = ActionPolicies.Get.Get( () =>
						this._webRequestServices.GetResponse< List< BigCommerceOrderProduct > >( order.ProductsReference.Url, endpoint ) );
					this.CreateApiDelay().Wait(); //API requirement

					if( products == null )
						break;
					order.Products.AddRange( products );
					if( products.Count < RequestMaxLimit )
						break;
				}
			}
		}

		private async Task GetOrdersProductsAsync( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var products = await ActionPolicies.GetAsync.Get( async () =>
						await this._webRequestServices.GetResponseAsync< List< BigCommerceOrderProduct > >( order.ProductsReference.Url, endpoint ) );
					await this.CreateApiDelay(); //API requirement

					if( products == null )
						break;
					order.Products.AddRange( products );
					if( products.Count < RequestMaxLimit )
						break;
				}
			}
		}
		#endregion

		#region ShippingAddress
		private void GetOrdersShippingAddresses( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				order.ShippingAddresses = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceShippingAddress > >( order.ShippingAddressesReference.Url ) );
				this.CreateApiDelay().Wait(); //API requirement
			}
		}

		private async Task GetOrdersShippingAddressesAsync( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				order.ShippingAddresses = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceShippingAddress > >( order.ShippingAddressesReference.Url ) );
				await this.CreateApiDelay(); //API requirement
			}
		}
		#endregion
	}
}