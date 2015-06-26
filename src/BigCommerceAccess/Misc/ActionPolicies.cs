using System;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;

namespace BigCommerceAccess.Misc
{
	public static class ActionPolicies
	{
		public static readonly ActionPolicy Submit = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 5 + 20 * i ) );
		} );

		public static readonly ActionPolicyAsync SubmitAsync = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 5 + 20 * i ) );
		} );

		public static readonly ActionPolicy Get = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API get call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 5 + 20 * i ) );
		} );

		public static readonly ActionPolicyAsync GetAsync = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API get call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 5 + 20 * i ) );
		} );
	}
}