namespace BigCommerceAccess.Models.Command
{
	public class BigCommerceCommand
	{
		public static readonly BigCommerceCommand Unknown = new BigCommerceCommand( string.Empty );
		public static readonly BigCommerceCommand GetProducts = new BigCommerceCommand( "/api/v2/products.json" );
		public static readonly BigCommerceCommand GetOrders = new BigCommerceCommand( "/api/v2/orders.json" );
		public static readonly BigCommerceCommand GetProductsCount = new BigCommerceCommand( "/api/v2/products/count.json" );
		public static readonly BigCommerceCommand UpdateProduct = new BigCommerceCommand( "/api/v2/products/" );

		private BigCommerceCommand( string command )
		{
			this.Command = command;
		}

		public string Command { get; private set; }
	}
}