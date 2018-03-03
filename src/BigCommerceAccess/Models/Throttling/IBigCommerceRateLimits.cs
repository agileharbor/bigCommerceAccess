namespace BigCommerceAccess.Models.Throttling
{
	public interface IBigCommerceRateLimits
	{
		int CallsRemaining{ get; }

		int LimitRequestsLeft{ get; }
		int LimitTimeResetMs{ get; }

		bool IsUnlimitedCallsCount{ get; }
	}
}
