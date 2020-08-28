using System;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;

namespace BigCommerceAccess.Misc
{
	public static class ActionPolicies
	{
#if DEBUG
		public const int RetryCount = 1;
#else
		public const int RetryCount = 10;
#endif
		
		public static ActionPolicy Submit( string marker, string url )
		{
			return ActionPolicy.Handle< Exception >().Retry( RetryCount, ( ex, i ) =>
			{
				var delay = TimeSpan.FromSeconds( 5 + 20 * i );
				BigCommerceLogger.LogTraceException( new RetryInfo()
				{
					Mark = marker,
					Url = url,
					CurrentRetryAttempt = i,
					TotalRetriesAttempts = RetryCount,
					DelayInSeconds = delay.TotalSeconds,
					Category = MessageCategoryEnum.Warning
				}, ex );
				SystemUtil.Sleep( delay );
			} );
		}

		public static ActionPolicyAsync SubmitAsync( string marker, string url )
		{
			return ActionPolicyAsync.Handle< Exception >().RetryAsync( RetryCount, async ( ex, i ) =>
			{
				var delay = TimeSpan.FromSeconds( 5 + 20 * i );
				BigCommerceLogger.LogTraceException( new RetryInfo()
				{
					Mark = marker,
					Url = url,
					CurrentRetryAttempt = i,
					TotalRetriesAttempts = RetryCount,
					DelayInSeconds = delay.TotalSeconds,
					Category = MessageCategoryEnum.Warning
				}, ex );
				await Task.Delay( delay );
			} );
		}

		public static ActionPolicy Get( string marker, string url )
		{
			return ActionPolicy.Handle< Exception >().Retry( RetryCount, ( ex, retryAttempt ) => { 
				var delay = TimeSpan.FromSeconds( 5 + 20 * retryAttempt );
				BigCommerceLogger.LogTraceException( new RetryInfo()
				{
					Mark = marker,
					Url = url,
					CurrentRetryAttempt = retryAttempt,
					TotalRetriesAttempts = RetryCount,
					DelayInSeconds = delay.TotalSeconds,
					Category = MessageCategoryEnum.Warning
				}, ex );
				SystemUtil.Sleep( delay );
			} );
		}

		public static ActionPolicyAsync GetAsync( string marker, string url )
		{
			return ActionPolicyAsync.Handle< Exception >().RetryAsync( RetryCount, async ( ex, retryAttempt ) => { 
				var delay = TimeSpan.FromSeconds( 5 + 20 * retryAttempt );
				BigCommerceLogger.LogTraceException( new RetryInfo()
				{
					Mark = marker,
					Url = url,
					CurrentRetryAttempt = retryAttempt,
					TotalRetriesAttempts = RetryCount,
					DelayInSeconds = delay.TotalSeconds,
					Category = MessageCategoryEnum.Warning
				}, ex );
				await Task.Delay( delay );
			} );
		}

		public static void LogRetryAndWait( Exception ex, string marker, string url, int retryAttempt )
		{
			var delay = TimeSpan.FromSeconds( 5 + 20 * retryAttempt );
			BigCommerceLogger.LogTraceException( new RetryInfo()
			{
				Mark = marker,
				Url = url,
				CurrentRetryAttempt = retryAttempt,
				TotalRetriesAttempts = RetryCount,
				DelayInSeconds = delay.TotalSeconds,
				Category = MessageCategoryEnum.Warning
			}, ex );
			SystemUtil.Sleep( delay );
		}

		public static async Task LogRetryAndWaitAsync( Exception ex, string marker, string url, int retryAttempt )
		{
			var delay = TimeSpan.FromSeconds( 5 + 20 * retryAttempt );
			BigCommerceLogger.LogTraceException( new RetryInfo()
			{
				Mark = marker,
				Url = url,
				CurrentRetryAttempt = retryAttempt,
				TotalRetriesAttempts = RetryCount,
				DelayInSeconds = delay.TotalSeconds,
				Category = MessageCategoryEnum.Warning
			}, ex );
			await Task.Delay( delay );
		}
	}
}