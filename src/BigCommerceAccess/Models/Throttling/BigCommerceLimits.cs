namespace BigCommerceAccess.Models.Throttling
{
	internal class BigCommerceLimits: IBigCommerceRateLimits
	{
		private const int UnlimitCnt = 100000;
		public int CallsRemaining{ get; private set; }

		public bool IsUnlimitedCallsCount
		{
			get { return this.CallsRemaining > UnlimitCnt; }
		}

		public BigCommerceLimits( int callsRemaining )
		{
			this.CallsRemaining = callsRemaining;
		}
	}
}
