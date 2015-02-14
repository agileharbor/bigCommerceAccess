using Netco.Logging;

namespace BigCommerceAccess.Misc
{
	public class BigCommerceLogger
	{
		public static ILogger Log{ get; private set; }

		static BigCommerceLogger()
		{
			Log = NetcoLogger.GetLogger( "BigCommerceLogger" );
		}
	}
}