namespace BigCommerceAccess.Models.Throttling
{
	internal class BigCommerceLimits: IBigCommerceRateLimits
	{
		// 2018-10-11: https://support.bigcommerce.com/articles/Public/Platform-Limits
		// Trial Stores, Standard and Plus plans : 20000 per hour
		// Pro plans : 60000 per hour
		// Enterprise : Unlimited
		private const int UnlimitCnt = 60001;
		public int CallsRemaining{ get; private set; }

		public int LimitRequestsLeft{ get; private set; }
		public int LimitTimeResetMs{ get; private set; }

		public bool IsUnlimitedCallsCount
		{
			get
			{
				if( this.CallsRemaining != -1 && this.CallsRemaining > UnlimitCnt )
					return true; // because plan of client is Enterprise and he shouldn't have any delays

				if( this.LimitRequestsLeft != -1 ) // it means that client use OAuth and we should check LimitRequestsLeft
					return this.LimitRequestsLeft > 20;

				return this.CallsRemaining > 100; // it means that client use BasicAuth and CallsRemaining contains number of API requests remaining for the current period
			}
		}

		public BigCommerceLimits( int callsRemaining = -1, int limitRequestsLeft = -1, int limitTimeResetMs = -1 )
		{
			this.CallsRemaining = callsRemaining;
			this.LimitRequestsLeft = limitRequestsLeft;
			this.LimitTimeResetMs = limitTimeResetMs;
		}
	}
}
