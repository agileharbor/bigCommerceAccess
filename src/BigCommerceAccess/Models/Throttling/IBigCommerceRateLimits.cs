namespace BigCommerceAccess.Models.Throttling
{
	public interface IBigCommerceRateLimits
	{
		int CallsRemaining{ get; }
		bool IsUnlimitedCallsCount{ get; }
	}
}
