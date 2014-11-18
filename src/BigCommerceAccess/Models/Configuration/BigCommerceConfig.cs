using CuttingEdge.Conditions;

namespace BigCommerceAccess.Models.Configuration
{
	public sealed class BigCommerceConfig
	{
		public string NativeHost{ get; private set; }
		public string CustomHost{ get; private set; }
		public string ShopName{ get; private set; }
		public string UserName{ get; private set; }
		public string ApiKey{ get; private set; }

		public BigCommerceConfig( string shopName, string userName, string apiKey )
		{
			Condition.Requires( shopName, "shopName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( userName, "userName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( apiKey, "apiKey" ).IsNotNullOrWhiteSpace();

			this.NativeHost = string.Format( "https://{0}.mybigcommerce.com", shopName );
			this.CustomHost = string.Format( "https://www.{0}.com", shopName );
			this.ShopName = shopName;
			this.UserName = userName;
			this.ApiKey = apiKey;
		}
	}
}