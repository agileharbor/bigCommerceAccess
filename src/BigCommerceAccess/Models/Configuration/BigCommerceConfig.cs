using System;
using System.Collections.Generic;
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

		public string ClientId{ get; private set; }
		public string ClientSecret{ get; private set; }
		public string Token{ get; private set; }

		public BigCommerceConfig( string shopName, string userName, string apiKey )
		{
			Condition.Requires( shopName, "shopName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( userName, "userName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( apiKey, "apiKey" ).IsNotNullOrWhiteSpace();

			var domainAndShopName = this.GetDomainAndShopName( shopName );

			this.NativeHost = string.Format( "https://{0}.mybigcommerce{1}", domainAndShopName.Item2, domainAndShopName.Item1 );
			this.CustomHost = string.Format( "https://www.{0}{1}", domainAndShopName.Item2, domainAndShopName.Item1 );
			this.UserName = userName;
			this.ApiKey = apiKey;

			this.ShopName = shopName;

			this.ClientId = string.Empty;
			this.ClientSecret = string.Empty;
			this.Token = string.Empty;
		}

		public BigCommerceConfig( string shopName, string clientId, string clientSecret, string token )
		{
			Condition.Requires( shopName, "shopName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( clientId, "clientId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( clientSecret, "clientSecret" ).IsNotNullOrWhiteSpace();
			Condition.Requires( token, "token" ).IsNotNullOrWhiteSpace();

			this.NativeHost = string.Format( "https://api.bigcommerce.com/stores/{0}", shopName );
			this.CustomHost = string.Empty;
			this.UserName = string.Empty;
			this.ApiKey = string.Empty;

			this.ShopName = shopName;

			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			this.Token = token;
		}

		public APIVersion GetAPIVersion()
		{
			return string.IsNullOrEmpty( this.ClientId ) ? APIVersion.V2 : APIVersion.V3;
		}

		private Tuple< string, string > GetDomainAndShopName( string shopName )
		{
			var lastIndexPoint = shopName.LastIndexOf( '.' );
			if( lastIndexPoint == -1 )
				return new Tuple< string, string >( ".com", shopName );

			var domain = shopName.Substring( lastIndexPoint );
			if( this._existDomains.Contains( domain ) )
				return new Tuple< string, string >( domain, shopName.Substring( 0, lastIndexPoint ) );

			return new Tuple< string, string >( ".com", shopName );
		}

		private readonly List< string > _existDomains = new List< string >
		{
			".aero",
			".asia",
			".biz",
			".cat",
			".com",
			".coop",
			".edu",
			".gov",
			".info",
			".int",
			".jobs",
			".mil",
			".mobi",
			".museum",
			".name",
			".net",
			".org",
			".post",
			".pro",
			".properties",
			".tel",
			".travel",
			".ru",
			".uk"
		};
	}

	public enum APIVersion
	{
		V2 = 0,
		V3 = 1
	}
}