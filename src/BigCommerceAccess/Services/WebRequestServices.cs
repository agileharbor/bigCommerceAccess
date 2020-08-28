using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Throttling;
using Newtonsoft.Json;

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

		public BigCommerceResponse< T > GetResponseByRelativeUrl< T >( string url, string commandParams, string marker, [ CallerMemberName ] string callerMethodName = null ) where T : class
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = this.GetResponse< T >( requestUrl, marker, callerMethodName );
			return result;
		}

		public async Task< BigCommerceResponse< T > > GetResponseByRelativeUrlAsync< T >( string url, string commandParams, string marker, [ CallerMemberName ] string callerMethodName = null ) where T : class
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl, marker, callerMethodName );
			return result;
		}

		public BigCommerceResponse< T > GetResponseByRelativeUrl< T >( BigCommerceCommand command, string commandParams, string marker, [ CallerMemberName ] string callerMethodName = null ) where T : class
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = this.GetResponse< T >( requestUrl, marker, callerMethodName );
			return result;
		}

		public async Task< BigCommerceResponse< T > > GetResponseByRelativeUrlAsync< T >( BigCommerceCommand command, string commandParams, string marker, [ CallerMemberName ] string callerMethodName = null ) where T : class
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl, marker, callerMethodName );
			return result;
		}

		public BigCommerceResponse< T > GetResponse< T >( string url, string marker, [ CallerMemberName ] string callerMethodName = null ) where T : class
		{
			this.LogCallStarted( url, marker, callerMethodName );
			var responseStatusCode = HttpStatusCode.OK;

			try
			{
				BigCommerceResponse< T > result;
				var request = this.CreateGetServiceGetRequest( url );
				using( var response = request.GetResponse() )
				{
					responseStatusCode = ( ( HttpWebResponse )response ).StatusCode;
					result = this.ParseResponse< T >( response, marker, url, callerMethodName );
				}
					
				return result;
			}
			catch( Exception ex )
			{
				throw this.HandleExceptionAndLog( url, marker, callerMethodName, responseStatusCode.ToString(), ex );
			}
		}

		public async Task< BigCommerceResponse< T > > GetResponseAsync< T >( string url, string marker, [ CallerMemberName ] string callerMethodName = null ) where T : class
		{
			this.LogCallStarted( url, marker, callerMethodName );
			var responseStatusCode = HttpStatusCode.OK;

			try
			{
				BigCommerceResponse< T > result;
				var request = this.CreateGetServiceGetRequest( url );
				var timeoutToken = this.GetTimeoutToken( RequestTimeoutMs );
				using( timeoutToken.Register( request.Abort ) )
				using( var response = await request.GetResponseAsync() )
				{
					responseStatusCode = ( ( HttpWebResponse )response ).StatusCode;
					timeoutToken.ThrowIfCancellationRequested();
					result = this.ParseResponse< T >( response, marker, url, callerMethodName );
				}
				return result;
			}
			catch( Exception ex )
			{
				throw this.HandleExceptionAndLog( url, marker, callerMethodName, responseStatusCode.ToString(), ex );
			}
		}

		public IBigCommerceRateLimits PutData( BigCommerceCommand command, string endpoint, string jsonContent, string marker, [ CallerMemberName ] string callerMethodName = null )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogCallStarted( url, marker, callerMethodName, HttpMethodEnum.Put, jsonContent );
			var responseStatusCode = HttpStatusCode.OK;

			try
			{
				var request = this.CreateServicePutRequest( url, jsonContent );
				using( var response = ( HttpWebResponse )request.GetResponse() )
				{
					responseStatusCode = response.StatusCode;
					var currentLimits = this.ParseLimits( response );
					this.LogCallEnded( url, marker, callerMethodName, response.StatusCode.ToString(), null, currentLimits.CallsRemaining.ToString(), null );
					return currentLimits;
				}
			}
			catch( Exception ex )
			{
				throw this.HandleExceptionAndLog( url, marker, callerMethodName, responseStatusCode.ToString(), ex );
			}
		}

		public async Task< IBigCommerceRateLimits > PutDataAsync( BigCommerceCommand command, string endpoint, string jsonContent, string marker, [ CallerMemberName ] string callerMethodName = null )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogCallStarted( url, marker, callerMethodName, HttpMethodEnum.Put, jsonContent );
			var responseStatusCode = HttpStatusCode.OK;

			try
			{
				var request = this.CreateServicePutRequest( url, jsonContent );
				var timeoutToken = this.GetTimeoutToken( RequestTimeoutMs );
				using( timeoutToken.Register( request.Abort ) )
				using( var response = await request.GetResponseAsync() )
				{
					responseStatusCode = ( ( HttpWebResponse )response ).StatusCode;
					timeoutToken.ThrowIfCancellationRequested();
					var currentLimits = this.ParseLimits( response );
					this.LogCallEnded( url, marker, callerMethodName, ( ( HttpWebResponse )response ).StatusCode.ToString(), null, currentLimits.CallsRemaining.ToString(), null );
					return currentLimits;
				}
			}
			catch( Exception ex )
			{
				throw this.HandleExceptionAndLog( url, marker, callerMethodName, responseStatusCode.ToString(), ex );
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
		private BigCommerceResponse< T > ParseResponse< T >( WebResponse response, string marker, string url, string callerMethodName ) where T : class
		{
			using( var stream = response.GetResponseStream() )
			using( var reader = new StreamReader( stream ) )
			{
				var jsonResponse = reader.ReadToEnd();

				var remainingLimit = this.GetRemainingLimit( response );
				var version = response.Headers.Get( "X-BC-Store-Version" );
				var statusCode = ( ( HttpWebResponse )response ).StatusCode;
				this.LogCallEnded( url, marker, callerMethodName, statusCode.ToString(), jsonResponse, remainingLimit, version );
				var limits = this.ParseLimits( response );

				if( string.IsNullOrEmpty( jsonResponse ) )
					return new BigCommerceResponse< T >( null, limits );

				var result = JsonConvert.DeserializeObject< T >( jsonResponse );
				return new BigCommerceResponse< T >( result, limits );
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

		private string ResolveHost( BigCommerceConfig config, string marker, [ CallerMemberName ] string callerMethodName = null )
		{
			try
			{
				var url = string.Concat( config.NativeHost, BigCommerceCommand.GetOrdersCountV2.Command );
				this.GetResponse< BigCommerceItemsCount >( url, marker, callerMethodName );
				return config.NativeHost;
			}
			catch( Exception )
			{
				try
				{
					var url = string.Concat( config.CustomHost, BigCommerceCommand.GetOrdersCountV2.Command );
					this.GetResponse< BigCommerceItemsCount >( url, marker, callerMethodName );
					return config.CustomHost;
				}
				catch( Exception )
				{
					var clippedHost = config.CustomHost.Replace( "www.", string.Empty );
					var url = string.Concat( clippedHost, BigCommerceCommand.GetOrdersCountV2.Command );
					this.GetResponse< BigCommerceItemsCount >( url, marker, callerMethodName );
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

		private void LogCallStarted( string url, string marker, string callerMethodName, HttpMethodEnum httpMethod = HttpMethodEnum.Get, string body = null )
		{
			BigCommerceLogger.TraceLog( new RequestInfo()
			{
				Mark = marker,
				Url = url,
				LibMethodName = callerMethodName,
				Category = MessageCategoryEnum.Information,
				HttpMethod = httpMethod,
				Body = body,
				TenantId = this._config.TenantId,
				ChannelAccountId = this._config.ChannelAccountId
			} );
		}

		private void LogCallEnded( string url, string marker, string callerMethodName, string statusCode, string response, string remainingCalls, string systemVersion )
		{
			BigCommerceLogger.TraceLog( new ResponseInfo()
			{
				Mark = marker,
				Url = url,
				LibMethodName = callerMethodName,
				Category = MessageCategoryEnum.Information,
				Response = response,
				StatusCode = statusCode,
				RemainingCalls = remainingCalls,
				SystemVersion = systemVersion,
				TenantId = this._config.TenantId,
				ChannelAccountId = this._config.ChannelAccountId
			} );
		}

		private Exception HandleExceptionAndLog( string url, string marker, string callerMethodName, string statusCode, Exception ex )
		{
			BigCommerceLogger.LogTraceException( new ResponseInfo()
			{
				Mark = marker,
				Url = url,
				LibMethodName = callerMethodName,
				StatusCode = statusCode,
				Category = MessageCategoryEnum.Critical,
				TenantId = this._config.TenantId,
				ChannelAccountId = this._config.ChannelAccountId
			}, ex );

			return new Exception( string.Format( "Marker: '{0}'. Call to url '{1}' failed", marker, url ), ex );
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