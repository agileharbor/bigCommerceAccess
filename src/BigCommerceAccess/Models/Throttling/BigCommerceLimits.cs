namespace BigCommerceAccess.Models.Throttling
{
	internal class BigCommerceLimits: IBigCommerceRateLimits
	{
		private const int UnlimitCnt = 100000;
		public int CallsRemaining{ get; }

		public int LimitRequestsLeft{ get; }
		public int LimitTimeResetMs{ get; }

		public bool IsUnlimitedCallsCount
		{
			get
			{
				if( this.CallsRemaining != -1 )
					return this.CallsRemaining > UnlimitCnt;

				return this.LimitRequestsLeft > 10;
			}
		}

		public BigCommerceLimits( int callsRemaining )
		{
			this.CallsRemaining = callsRemaining;
			this.LimitRequestsLeft = -1;
			this.LimitTimeResetMs = -1;
		}

		public BigCommerceLimits( int limitRequestsLeft, int limitTimeResetMs )
		{
			this.CallsRemaining = -1;
			this.LimitRequestsLeft = limitRequestsLeft;
			this.LimitTimeResetMs = limitTimeResetMs;
		}
	}
}
