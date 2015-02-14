using System;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;

namespace BigCommerceAccess.Misc
{
	public static class ActionPolicies
	{
		public static ActionPolicy Submit
		{
			get { return _bigCommerceSumbitPolicy; }
		}

		private static readonly ActionPolicy _bigCommerceSumbitPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicyAsync SubmitAsync
		{
			get { return _bigCommerceSumbitAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _bigCommerceSumbitAsyncPolicy = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicy Get
		{
			get { return _bigCommerceGetPolicy; }
		}

		private static readonly ActionPolicy _bigCommerceGetPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API get call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicyAsync GetAsync
		{
			get { return _bigCommerceGetAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _bigCommerceGetAsyncPolicy = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
		{
			BigCommerceLogger.Log.Trace( ex, "Retrying BigCommerce API get call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
		} );
	}
}