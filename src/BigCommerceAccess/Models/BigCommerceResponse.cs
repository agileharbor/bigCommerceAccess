using BigCommerceAccess.Models.Throttling;

namespace BigCommerceAccess.Models
{
	internal class BigCommerceResponse< T > where T : class
	{
		public T Response{ get; private set; }
		public IBigCommerceRateLimits Limits{ get; private set; }

		public BigCommerceResponse( T response, IBigCommerceRateLimits limits )
		{
			this.Response = response;
			this.Limits = limits;
		}
	}
}
