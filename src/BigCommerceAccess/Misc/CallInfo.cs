namespace BigCommerceAccess.Misc
{
	public enum HttpMethodEnum { Get, Post, Put }
	public enum MessageCategoryEnum { Information, Warning, Critical }

	public abstract class CallInfo
	{
		protected CallInfo()
		{
			this.Mark = "Unknown";
		}

		public string Mark { get; set; }
		public string LibMethodName { get; set; }
		public string Url { get; set; }
		public MessageCategoryEnum Category { get; set; }

		public long? TenantId { get; set; }
		public long? ChannelAccountId { get; set; }
	}

	public sealed class RequestInfo : CallInfo
	{
		public HttpMethodEnum HttpMethod { get; set; }
		public object Body { get; set; }
	}

	public sealed class ResponseInfo : CallInfo
	{
		public object Response { get; set; }
		public string RemainingCalls { get; set; }
		public string SystemVersion { get; set; }
		public string StatusCode { get; set; }
	}

	public sealed class RetryInfo : CallInfo
	{
		public int TotalRetriesAttempts { get; set; }
		public int CurrentRetryAttempt { get; set; }
		public double DelayInSeconds { get; set; }
	}
}