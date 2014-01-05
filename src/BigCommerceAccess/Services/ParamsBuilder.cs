using System;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;

namespace BigCommerceAccess.Services
{
	internal static class ParamsBuilder
	{
		public static readonly string EmptyParams = string.Empty;

		public static string CreateOrdersParams( DateTime startDate, DateTime endDate )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
				BigCommerceParam.OrdersCreatedDateFrom.Name, DateTime.SpecifyKind( startDate, DateTimeKind.Utc ).ToString( "o" ),
				BigCommerceParam.OrdersCreatedDateTo.Name, DateTime.SpecifyKind( endDate, DateTimeKind.Utc ).ToString( "o" ),
				BigCommerceParam.OrdersModifiedDateFrom.Name, DateTime.SpecifyKind( startDate, DateTimeKind.Utc ).ToString( "o" ),
				BigCommerceParam.OrdersModifiedDateTo.Name, DateTime.SpecifyKind( endDate, DateTimeKind.Utc ).ToString( "o" ) );
			return endpoint;
		}

		public static string CreateProductUpdateEndpoint( long productId )
		{
			var endpoint = string.Format( "{0}.json", productId );
			return endpoint;
		}

		public static string CreateGetSinglePageParams( BigCommerceCommandConfig config )
		{
			var endpoint = string.Format( "?{0}={1}", BigCommerceCommandParamName.Limit.Name, config.Limit );
			return endpoint;
		}

		public static string CreateGetNextPageParams( BigCommerceCommandConfig config )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}",
				BigCommerceCommandParamName.Limit.Name, config.Limit,
				BigCommerceCommandParamName.Page.Name, config.Page );
			return endpoint;
		}
	}
}