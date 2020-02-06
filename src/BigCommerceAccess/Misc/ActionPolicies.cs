﻿using System;
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

		public static readonly ActionPolicy Submit = ActionPolicy.Handle< Exception >().Retry( RetryCount, ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 5 + 20 * i ) );
		} );

		public static readonly ActionPolicyAsync SubmitAsync = ActionPolicyAsync.Handle< Exception >().RetryAsync( RetryCount, async ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 5 + 20 * i ) );
		} );

		public static readonly ActionPolicy Get = ActionPolicy.Handle< Exception >().Retry( RetryCount, LogRetryAndWait );

		public static readonly ActionPolicyAsync GetAsync = ActionPolicyAsync.Handle< Exception >().RetryAsync( RetryCount, LogRetryAndWaitAsync );

		public static void LogRetryAndWait( Exception ex, int retryAttempt )
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API get call for the {0} time", retryAttempt );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 5 + 20 * retryAttempt ) );
		}

		public static async Task LogRetryAndWaitAsync( Exception ex, int retryAttempt )
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API get call for the {0} time", retryAttempt );
			await Task.Delay( TimeSpan.FromSeconds( 5 + 20 * retryAttempt ) );
		}
	}
}