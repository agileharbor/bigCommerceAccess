using System;
using System.Threading.Tasks;

namespace BigCommerceAccess
{
	public abstract class BigCommerceServiceBase
	{
		//since we have 20000 api calls per hour:
		//we need to grant not more than 5 api calls per second
		//to have 18000 per hour and 2000 calls for the retry needs
		private readonly TimeSpan DefaultApiDelay = TimeSpan.FromMilliseconds( 200 );
		protected const int RequestMaxLimit = 200;

		protected Task CreateApiDelay()
		{
			return Task.Delay( this.DefaultApiDelay );
		}

		protected int CalculatePagesCount( int itemsCount )
		{
			var result = ( int )Math.Ceiling( ( double )itemsCount / RequestMaxLimit );
			return result;
		}
	}
}