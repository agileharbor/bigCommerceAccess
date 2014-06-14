using System;
using System.Text;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;

namespace BigCommerceAccess.Services
{
	internal static class ParamsBuilder
	{
		public static readonly string EmptyParams = string.Empty;

		public static string CreateOrdersParams( DateTime startDate, DateTime endDate )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}",
				BigCommerceParam.OrdersModifiedDateFrom.Name, DateTime.SpecifyKind( startDate, DateTimeKind.Utc ).ToString( "o" ),
				BigCommerceParam.OrdersModifiedDateTo.Name, DateTime.SpecifyKind( endDate, DateTimeKind.Utc ).ToString( "o" ) );
			return endpoint;
		}

		public static string CreateProductUpdateEndpoint( long productId )
		{
			var endpoint = string.Format( "{0}.json", productId );
			return endpoint;
		}

		public static string CreateProductOptionUpdateEndpoint( long productId, long optionId )
		{
			return string.Format( "{0}/skus/{1}.json", productId, optionId );
		}

		public static string CreateGetSinglePageParams( BigCommerceCommandConfig config )
		{
			var endpoint = string.Format( "?{0}={1}", BigCommerceParam.Limit.Name, config.Limit );
			return endpoint;
		}

		public static string CreateGetNextPageParams( BigCommerceCommandConfig config )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}",
				BigCommerceParam.Limit.Name, config.Limit,
				BigCommerceParam.Page.Name, config.Page );
			return endpoint;
		}

		public static string ConcatParams( this string mainEndpoint, params string[] endpoints )
		{
			var result = new StringBuilder( mainEndpoint );

			foreach( var endpoint in endpoints )
				result.Append( endpoint.Replace( "?", "&" ) );

			return result.ToString();
		}
	}
}