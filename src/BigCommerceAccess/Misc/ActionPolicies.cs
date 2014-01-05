using System;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Logging;
using Netco.Utils;

namespace BigCommerceAccess.Misc
{
	public static class ActionPolicies
	{
		public static ActionPolicy BigCommerceGetPolicy
		{
			get { return _bigCommerceGetPolicy; }
		}

		private static readonly ActionPolicy _bigCommerceGetPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
			{
				typeof( ActionPolicies ).Log().Trace( ex, "Retrying BigCommerce API get call for the {0} time", i );
				SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
			} );

		public static ActionPolicy BigCommerceSubmitPolicy
		{
			get { return _bigCommerceSumbitPolicy; }
		}

		private static readonly ActionPolicy _bigCommerceSumbitPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
			{
				typeof( ActionPolicies ).Log().Trace( ex, "Retrying BigCommerce API submit call for the {0} time", i );
				SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
			} );

		public static ActionPolicyAsync QueryAsync
		{
			get { return _queryAsync; }
		}

		private static readonly ActionPolicyAsync _queryAsync = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
			{
				typeof( ActionPolicies ).Log().Trace( ex, "Retrying BigCommerce API get call for the {0} time", i );
				await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
			} );
	}
}