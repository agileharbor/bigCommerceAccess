using System;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Throttling;

namespace BigCommerceAccess
{
	public abstract class BigCommerceServiceBase
	{
		//since we have 20000 api calls per hour:
		//we need to grant not more than 5 api calls per second
		//to have 18000 per hour and 2000 calls for the retry needs
		private readonly TimeSpan DefaultApiDelay = TimeSpan.FromMilliseconds( 200 );
		protected int RequestMaxLimit = 250;
		protected  int RequestMinLimit = 50;
		protected const int MaxThreadsCount = 5;

		protected Task CreateApiDelay( IBigCommerceRateLimits limits )
		{
			return this.CreateApiDelay( limits, CancellationToken.None );
		}

		protected Task CreateApiDelay( IBigCommerceRateLimits limits, CancellationToken token )
		{
			return limits.IsUnlimitedCallsCount ? Task.FromResult( 0 ) : Task.Delay( limits.LimitTimeResetMs != -1 ? TimeSpan.FromMilliseconds( limits.LimitTimeResetMs ) : this.DefaultApiDelay, token );
		}

		protected int CalculatePagesCount( int itemsCount )
		{
			var result = ( int )Math.Ceiling( ( double )itemsCount / RequestMaxLimit );
			return result;
		}

		protected string GetMarker()
		{
			var marker = Guid.NewGuid().ToString();
			return marker;
		}
	}
}