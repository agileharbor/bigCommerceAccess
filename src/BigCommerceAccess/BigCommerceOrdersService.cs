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
	public class BigCommerceOrdersService : IBigCommerceOrdersService
	{
		private readonly WebRequestServices _webRequestServices;

		public BigCommerceOrdersService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this._webRequestServices = new WebRequestServices( config );
		}

		public IEnumerable< BigCommerceOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var orders = new List< BigCommerceOrder >();
			var endpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );

			ActionPolicies.Get.Do( () =>
			{
				orders = this._webRequestServices.GetResponse< List< BigCommerceOrder > >( BigCommerceCommand.GetOrders, endpoint );
			} );

			this.GetOrdersProducts( orders );
			this.GetOrdersShippingAddresses( orders );

			return orders;
		}

		public async Task< IEnumerable< BigCommerceOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var endpoint = ParamsBuilder.CreateOrdersParams( dateFrom, dateTo );
			var orders = new List< BigCommerceOrder >();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				orders = await this._webRequestServices.GetResponseAsync< List< BigCommerceOrder > >( BigCommerceCommand.GetOrders, endpoint );
			} );

			await this.GetOrdersProductsAsync( orders );
			await this.GetOrdersShippingAddressesAsync( orders );

			return orders;
		}

		private void GetOrdersProducts( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				var o = order;
				ActionPolicies.Get.Do( () =>
				{
					o.Products = this._webRequestServices.GetResponse< IList< BigCommerceOrderProduct > >( o.ProductsReference.Url );
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
				} );
			}
		}

		private void GetOrdersShippingAddresses( IEnumerable< BigCommerceOrder > orders )
		{
			foreach( var order in orders )
			{
				var o = order;
				ActionPolicies.Get.Do( () =>
				{
					o.ShippingAddresses = this._webRequestServices.GetResponse< IList< BigCommerceShippingAddress > >( o.ProductsReference.Url );
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
				} );
			}
		}
	}
}