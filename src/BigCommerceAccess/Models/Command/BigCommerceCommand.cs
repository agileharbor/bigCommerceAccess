namespace BigCommerceAccess.Models.Command
{
	public class BigCommerceCommand
	{
		public static readonly BigCommerceCommand Unknown = new BigCommerceCommand( string.Empty );

		public static readonly BigCommerceCommand GetProductsV2 = new BigCommerceCommand( "/api/v2/products.json" );
		public static readonly BigCommerceCommand GetOrdersV2 = new BigCommerceCommand( "/api/v2/orders.json" );
		public static readonly BigCommerceCommand GetOrdersCountV2 = new BigCommerceCommand( "/api/v2/orders/count.json" );
		public static readonly BigCommerceCommand GetProductsCountV2 = new BigCommerceCommand( "/api/v2/products/count.json" );
		public static readonly BigCommerceCommand UpdateProductV2 = new BigCommerceCommand( "/api/v2/products/" );

		public static readonly BigCommerceCommand GetProductsV3 = new BigCommerceCommand( "/v2/products" );
		public static readonly BigCommerceCommand GetOrdersV3 = new BigCommerceCommand( "/v2/orders" );
		public static readonly BigCommerceCommand GetOrdersCountV3 = new BigCommerceCommand( "/v2/orders/count" );
		public static readonly BigCommerceCommand GetProductsCountV3 = new BigCommerceCommand( "/v2/products/count" );
		public static readonly BigCommerceCommand UpdateProductV3 = new BigCommerceCommand( "/v2/products/" );

		private BigCommerceCommand( string command )
		{
			this.Command = command;
		}

		public string Command{ get; private set; }
	}
}