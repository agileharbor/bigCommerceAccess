using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Services;
using System;

namespace BigCommerceAccess
{
	public interface IBigCommerceFactory
	{
		IBigCommerceOrdersService CreateOrdersService( BigCommerceConfig config );
		IBigCommerceProductsService CreateProductsService( BigCommerceConfig config );
	}

	public sealed class BigCommerceFactory : IBigCommerceFactory
	{
		public IBigCommerceOrdersService CreateOrdersService( BigCommerceConfig config )
		{
			return new BigCommerceOrdersService( config );
		}

		public IBigCommerceProductsService CreateProductsService( BigCommerceConfig config )
		{
			var apiVersion = config.GetAPIVersion();
			var marker = Guid.NewGuid().ToString();
			var services = new WebRequestServices( config, marker );

			if ( apiVersion == APIVersion.V2 )
			{
				return new BigCommerceProductsServiceV2( services );
			}

			return new BigCommerceProductsServiceV3( services );
		}
	}
}