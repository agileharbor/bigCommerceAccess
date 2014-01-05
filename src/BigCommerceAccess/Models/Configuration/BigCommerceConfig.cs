using System;
using System.Text;
using CuttingEdge.Conditions;

namespace BigCommerceAccess.Models.Configuration
{
	public sealed class BigCommerceConfig
	{
		public string Host { get; private set; }
		public string ShopName { get; private set; }
		public string UserName { get; private set; }
		public string ApiKey { get; private set; }

		public BigCommerceConfig( string shopName, string userName, string apiKey )
		{
			Condition.Requires( shopName, "shopName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( userName, "userName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( apiKey, "apiKey" ).IsNotNullOrWhiteSpace();

			this.Host = string.Format( "https://{0}.mybigcommerce.com", shopName );
			this.ShopName = shopName;
			this.UserName = userName;
			this.ApiKey = apiKey;
		}
	}
}