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
		public static readonly BigCommerceCommand GetStoreV2 = new BigCommerceCommand( "/api/v2/store.json" );
		public static readonly BigCommerceCommand GetBrandsV2 = new BigCommerceCommand( "/api/v2/brands.json" );

		public static readonly BigCommerceCommand GetProductsV2_OAuth = new BigCommerceCommand( "/v2/products" );
		public static readonly BigCommerceCommand GetOrdersV2_OAuth = new BigCommerceCommand( "/v2/orders" );
		public static readonly BigCommerceCommand GetOrdersCountV2_OAuth = new BigCommerceCommand( "/v2/orders/count" );
		public static readonly BigCommerceCommand GetProductsCountV2_OAuth = new BigCommerceCommand( "/v2/products/count" );
		public static readonly BigCommerceCommand UpdateProductV2_OAuth = new BigCommerceCommand( "/v2/products/" );
		public static readonly BigCommerceCommand GetStoreV2_OAuth = new BigCommerceCommand( "/v2/store" );
		public static readonly BigCommerceCommand GetBrandsV2_OAuth = new BigCommerceCommand( "/v2/brands" );

		public static readonly BigCommerceCommand GetProductsV3 = new BigCommerceCommand( "/v3/catalog/products" );
		public static readonly BigCommerceCommand UpdateProductsV3 = new BigCommerceCommand( "/v3/catalog/products" );

		public static readonly BigCommerceCommand GetCategoriesV3 = new BigCommerceCommand("/v3/catalog/categories");

		private BigCommerceCommand( string command )
		{
			this.Command = command;
		}

		public string Command{ get; private set; }
	}
}