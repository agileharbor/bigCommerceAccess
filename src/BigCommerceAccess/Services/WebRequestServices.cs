using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Order;
using BigCommerceAccess.Models.Throttling;
using Newtonsoft.Json;
using ServiceStack;

namespace BigCommerceAccess.Services
{
	internal class WebRequestServices
	{
		private const int RequestTimeoutMs = 30 * 60 * 1000;

		private readonly BigCommerceConfig _config;
		private readonly string _host;
		private readonly APIVersion _apiVersion;
		private const string AcceptValue = "application/json";

		public WebRequestServices( BigCommerceConfig config, string marker )
		{
			this._config = config;
			this._apiVersion = config.GetAPIVersion();
			this._host = this._apiVersion == APIVersion.V2 ? this.ResolveHost( config, marker ) : config.NativeHost;
		}

		private static HttpWebRequest CreateWebRequest( string url )
		{
			AllowInvalidCertificate();
			SetSecurityProtocol();
			var uri = new Uri( url );
			return ( HttpWebRequest )WebRequest.Create( uri );
		}

		public BigCommerceResponse< T > GetResponse< T >( string url, string commandParams, string marker ) where T : class
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = this.GetResponse< T >( requestUrl, marker );
			return result;
		}

		public async Task< BigCommerceResponse< T > > GetResponseAsync< T >( string url, string commandParams, string marker ) where T : class
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl, marker );
			return result;
		}

		public BigCommerceResponse< T > GetResponse< T >( BigCommerceCommand command, string commandParams, string marker ) where T : class
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = this.GetResponse< T >( requestUrl, marker );
			return result;
		}

		public async Task< BigCommerceResponse< T > > GetResponseAsync< T >( BigCommerceCommand command, string commandParams, string marker ) where T : class
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl, marker );
			return result;
		}

		public BigCommerceResponse< T > GetResponse< T >( string url, string marker ) where T : class
		{
			this.LogGetInfo( url, marker );

			try
			{
				BigCommerceResponse< T > result;
				var request = this.CreateGetServiceGetRequest( url );
				using( var response = request.GetResponse() )
					result = this.ParseResponse< T >( response, marker );
				return result;
			}
			catch( Exception ex )
			{
				throw this.ExceptionForGetInfo( url, ex, marker );
			}
		}

		public async Task< BigCommerceResponse< T > > GetResponseAsync< T >( string url, string marker ) where T : class
		{
			this.LogGetInfo( url, marker );

			try
			{
				BigCommerceResponse< T > result;
				var request = this.CreateGetServiceGetRequest( url );
				var timeoutToken = this.GetTimeoutToken( RequestTimeoutMs );
				using( timeoutToken.Register( request.Abort ) )
				using( var response = await request.GetResponseAsync() )
				{
					timeoutToken.ThrowIfCancellationRequested();
					result = this.ParseResponse< T >( response, marker );
				}
				return result;
			}
			catch( Exception ex )
			{
				throw this.ExceptionForGetInfo( url, ex, marker );
			}
		}

		public IBigCommerceRateLimits PutData( BigCommerceCommand command, string endpoint, string jsonContent, string marker )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogPutInfo( url, jsonContent, marker );

			try
			{
				var request = this.CreateServicePutRequest( url, jsonContent );
				using( var response = ( HttpWebResponse )request.GetResponse() )
				{
					this.LogPutInfoResult( url, response.StatusCode, jsonContent, marker );
					return this.ParseLimits( response );
				}
			}
			catch( Exception ex )
			{
				throw this.ExceptionForPutInfo( url, ex, marker );
			}
		}

		public async Task< IBigCommerceRateLimits > PutDataAsync( BigCommerceCommand command, string endpoint, string jsonContent, string marker )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogPutInfo( url, jsonContent, marker );

			try
			{
				var request = this.CreateServicePutRequest( url, jsonContent );
				var timeoutToken = this.GetTimeoutToken( RequestTimeoutMs );
				using( timeoutToken.Register( request.Abort ) )
				using( var response = await request.GetResponseAsync() )
				{
					timeoutToken.ThrowIfCancellationRequested();
					this.LogPutInfoResult( url, ( ( HttpWebResponse )response ).StatusCode, jsonContent, marker );
					return this.ParseLimits( response );
				}
			}
			catch( Exception ex )
			{
				throw this.ExceptionForPutInfo( url, ex, marker );
			}
		}

		#region WebRequest configuration
		private string GetUrl( string url, string commandParams )
		{
			return string.Concat( url, commandParams );
		}

		private string GetUrl( BigCommerceCommand command, string commandParams )
		{
			return string.Concat( this._host, command.Command, commandParams );
		}

		private HttpWebRequest CreateGetServiceGetRequest( string url )
		{
			if( this._apiVersion == APIVersion.V2 )
			{
				var request = CreateWebRequest( url );

				request.Method = WebRequestMethods.Http.Get;
				request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );
				request.Timeout = RequestTimeoutMs;
				request.ReadWriteTimeout = RequestTimeoutMs;

				return request;
			}
			else
			{
				var request = CreateWebRequest( url );

				request.Method = WebRequestMethods.Http.Get;
				request.Accept = AcceptValue;
				request.Headers.Add( "X-Auth-Client", this._config.ClientId );
				request.Headers.Add( "X-Auth-Token", this._config.Token );
				request.Timeout = RequestTimeoutMs;
				request.ReadWriteTimeout = RequestTimeoutMs;

				return request;
			}
		}

		private HttpWebRequest CreateServicePutRequest( string url, string content )
		{
			if( this._apiVersion == APIVersion.V2 )
			{
				var request = CreateWebRequest( url );

				request.Method = WebRequestMethods.Http.Put;
				request.ContentType = "application/json";
				request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );
				request.Timeout = RequestTimeoutMs;
				request.ReadWriteTimeout = RequestTimeoutMs;

				using( var writer = new StreamWriter( request.GetRequestStream() ) )
					writer.Write( content );

				return request;
			}
			else
			{
				var request = CreateWebRequest( url );

				request.Method = WebRequestMethods.Http.Put;
				request.ContentType = "application/json";
				request.Accept = AcceptValue;
				request.Headers.Add( "X-Auth-Client", this._config.ClientId );
				request.Headers.Add( "X-Auth-Token", this._config.Token );
				request.Timeout = RequestTimeoutMs;
				request.ReadWriteTimeout = RequestTimeoutMs;

				using( var writer = new StreamWriter( request.GetRequestStream() ) )
					writer.Write( content );

				return request;
			}
		}
		#endregion

		#region Misc
		private BigCommerceResponse< T > ParseResponse< T >( WebResponse response, string marker ) where T : class
		{
			using( var stream = response.GetResponseStream() )
			using( var reader = new StreamReader( stream ) )
			{
				var jsonResponse = reader.ReadToEnd();

				var remainingLimit = this.GetRemainingLimit( response );
				var version = response.Headers.Get( "X-BC-Store-Version" );
				this.LogGetInfoResult( response.ResponseUri.OriginalString, ( ( HttpWebResponse )response ).StatusCode, jsonResponse, remainingLimit, version, marker );
				var limits = this.ParseLimits( response );

				if( string.IsNullOrEmpty( jsonResponse ) )
					return new BigCommerceResponse< T >( null, limits );

				var serviceStackResult = jsonResponse.FromJson< T >();
				var jsonNetResult = JsonConvert.DeserializeObject< T >( jsonResponse );

				//TODO: Added for investigation SI-730. Remove it if all are ok
				var orders = serviceStackResult as List< BigCommerceOrder >;
				if( orders != null && orders.Count > 0 )
				{
					var serviceStackOrdersStr = string.Join( ", ", orders.Select( x => string.Format( "id:{0} date:{1}", x.Id, x.DateCreated ) ) );
					BigCommerceLogger.Log.Trace( "Marker: '{0}'. Url: '{1}' Retrieved ServiceStack BigCommerce orders: '{2}'",
						marker, response.ResponseUri.OriginalString, serviceStackOrdersStr );

					var jsonNetOrders = jsonNetResult as List< BigCommerceOrder >;
					if( jsonNetOrders != null && jsonNetOrders.Count > 0 )
					{
						var jsonNetOrdersStr = string.Join( ", ", orders.Select( x => string.Format( "id:{0} date:{1}", x.Id, x.DateCreated ) ) );
						if( !serviceStackOrdersStr.Equals( jsonNetOrdersStr ) )
						{
							BigCommerceLogger.Log.Warn( "Marker: '{0}'. Url: '{1}' Retrieved different Json.Net BigCommerce orders: '{2}'",
								marker, response.ResponseUri.OriginalString, jsonNetOrdersStr );
						}
					}
				}
				return new BigCommerceResponse< T >( jsonNetResult, limits );
			}
		}

		private IBigCommerceRateLimits ParseLimits( WebResponse response )
		{
			var remainingLimit = response.Headers.Get( "X-BC-ApiLimit-Remaining" );
			var callsRemaining = -1;

			if( !string.IsNullOrWhiteSpace( remainingLimit ) )
			{
				int.TryParse( remainingLimit, out callsRemaining );
			}

			var limitRequestsLeftValue = -1;
			var limitTimeResetMsValue = -1;

			var limitRequestsLeft = response.Headers.Get( "X-Rate-Limit-Requests-Left" );
			if( !string.IsNullOrWhiteSpace( limitRequestsLeft ) )
				int.TryParse( limitRequestsLeft, out limitRequestsLeftValue );

			var limitTimeResetMs = response.Headers.Get( "X-Rate-Limit-Time-Reset-Ms" );
			if( !string.IsNullOrWhiteSpace( limitTimeResetMs ) )
				int.TryParse( limitTimeResetMs, out limitTimeResetMsValue );

			return new BigCommerceLimits( callsRemaining, limitRequestsLeftValue, limitTimeResetMsValue );
		}

		private string GetRemainingLimit( WebResponse response )
		{
			var remainingLimit = response.Headers.Get( "X-BC-ApiLimit-Remaining" );
			var limitRequestsLeft = response.Headers.Get( "X-Rate-Limit-Requests-Left" );

			if( !string.IsNullOrEmpty( remainingLimit ) )
				return remainingLimit;

			return limitRequestsLeft;
		}

		private string CreateAuthenticationHeader()
		{
			var authInfo = string.Concat( this._config.UserName, ":", this._config.ApiKey );
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private string ResolveHost( BigCommerceConfig config, string marker )
		{
			try
			{
				var url = string.Concat( config.NativeHost, BigCommerceCommand.GetOrdersCountV2.Command );
				this.GetResponse< BigCommerceItemsCount >( url, marker );
				return config.NativeHost;
			}
			catch( Exception )
			{
				try
				{
					var url = string.Concat( config.CustomHost, BigCommerceCommand.GetOrdersCountV2.Command );
					this.GetResponse< BigCommerceItemsCount >( url, marker );
					return config.CustomHost;
				}
				catch( Exception )
				{
					var clippedHost = config.CustomHost.Replace( "www.", string.Empty );
					var url = string.Concat( clippedHost, BigCommerceCommand.GetOrdersCountV2.Command );
					this.GetResponse< BigCommerceItemsCount >( url, marker );
					return clippedHost;
				}
			}
		}

		private CancellationToken GetTimeoutToken( int timeout )
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter( timeout );
			return cancellationTokenSource.Token;
		}

		private void LogGetInfo( string url, string marker )
		{
			BigCommerceLogger.Log.Trace( "Marker: '{0}'. GET call for url '{1}'", marker, url );
		}

		private void LogGetInfoResult( string url, HttpStatusCode statusCode, string jsonContent, string remainingLimit, string version, string marker )
		{
			BigCommerceLogger.Log.Trace( "Marker: '{0}'. GET call for url '{1}' has been completed with code '{2}'. Remaining Limit: '{3}' Version: '{4}'.\n{5}",
				marker, url, statusCode, remainingLimit, version, jsonContent );
		}

		private Exception ExceptionForGetInfo( string url, Exception ex, string marker )
		{
			return new Exception( string.Format( "Marker: '{0}'. GET call for url '{1}' failed", marker, url ), ex );
		}

		private void LogPutInfo( string url, string jsonContent, string marker )
		{
			BigCommerceLogger.Log.Trace( "Marker: '{0}'. PUT/POST data for url '{1}':\n{2}", marker, url, jsonContent );
		}

		private void LogPutInfoResult( string url, HttpStatusCode statusCode, string jsonContent, string marker )
		{
			BigCommerceLogger.Log.Trace( "Marker: '{0}'. PUT/POST data for url '{1}' has been completed with code '{2}'.\n{3}", marker, url, statusCode, jsonContent );
		}

		private Exception ExceptionForPutInfo( string url, Exception ex, string marker )
		{
			return new Exception( string.Format( "Marker: '{0}'. PUT/POST data for url '{1}' failed", marker, url ), ex );
		}
		#endregion

		#region SSL certificate hack
		private static void AllowInvalidCertificate()
		{
			ServicePointManager.ServerCertificateValidationCallback += AllowCert;
		}

		private static bool AllowCert( object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error )
		{
			return true;
		}

		public static void SetSecurityProtocol()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}
		#endregion
	}
}