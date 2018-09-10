namespace BigCommerceAccess.Models.Throttling
{
	internal class BigCommerceLimits: IBigCommerceRateLimits
	{
		private const int UnlimitCnt = 100000;
		public int CallsRemaining{ get; private set; }

		public int LimitRequestsLeft{ get; private set; }
		public int LimitTimeResetMs{ get; private set; }

		public bool IsUnlimitedCallsCount
		{
			get
			{
				if( this.CallsRemaining != -1 && this.LimitRequestsLeft != -1 )
					return this.CallsRemaining > UnlimitCnt && this.LimitRequestsLeft > 20;

				if( this.CallsRemaining != -1 )
					return this.CallsRemaining > UnlimitCnt;

				return this.LimitRequestsLeft > 20;
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
