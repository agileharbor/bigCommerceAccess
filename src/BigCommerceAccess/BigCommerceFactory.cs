using BigCommerceAccess.Models.Configuration;

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
			return new BigCommerceProductsService( config );
		}
	}
}