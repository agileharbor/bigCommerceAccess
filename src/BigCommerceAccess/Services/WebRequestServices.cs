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
using Newtonsoft.Json;
using ServiceStack;

namespace BigCommerceAccess.Services
{
	internal class WebRequestServices
	{
		private const int RequestTimeoutMs = 30 * 60 * 1000;

		private readonly BigCommerceConfig _config;
		private readonly string _host;

		public WebRequestServices( BigCommerceConfig config, string marker )
		{
			this._config = config;
			this._host = this.ResolveHost( config, marker );
		}

		private static HttpWebRequest CreateWebRequest( string url )
		{
			AllowInvalidCertificate();
			SetSecurityProtocol();
			var uri = new Uri( url );
			return ( HttpWebRequest )WebRequest.Create( uri );
		}

		public T GetResponse< T >( string url, string commandParams, string marker )
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = this.GetResponse< T >( requestUrl, marker );
			return result;
		}

		public async Task< T > GetResponseAsync< T >( string url, string commandParams, string marker )
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl, marker );
			return result;
		}

		public T GetResponse< T >( BigCommerceCommand command, string commandParams, string marker )
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = this.GetResponse< T >( requestUrl, marker );
			return result;
		}

		public async Task< T > GetResponseAsync< T >( BigCommerceCommand command, string commandParams, string marker )
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl, marker );
			return result;
		}

		public T GetResponse< T >( string url, string marker )
		{
			this.LogGetInfo( url, marker );

			try
			{
				T result;
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

		public async Task< T > GetResponseAsync< T >( string url, string marker )
		{
			this.LogGetInfo( url, marker );

			try
			{
				T result;
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

		public void PutData( BigCommerceCommand command, string endpoint, string jsonContent, string marker )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogPutInfo( url, jsonContent, marker );

			try
			{
				var request = this.CreateServicePutRequest( url, jsonContent );
				using( var response = ( HttpWebResponse )request.GetResponse() )
					this.LogPutInfoResult( url, response.StatusCode, jsonContent, marker );
			}
			catch( Exception ex )
			{
				throw this.ExceptionForPutInfo( url, ex, marker );
			}
		}

		public async Task PutDataAsync( BigCommerceCommand command, string endpoint, string jsonContent, string marker )
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
			var request = CreateWebRequest( url );

			request.Method = WebRequestMethods.Http.Get;
			request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );
			request.Timeout = RequestTimeoutMs;
			request.ReadWriteTimeout = RequestTimeoutMs;

			return request;
		}

		private HttpWebRequest CreateServicePutRequest( string url, string content )
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
		#endregion

		#region Misc
		private T ParseResponse< T >( WebResponse response, string marker )
		{
			using( var stream = response.GetResponseStream() )
			using( var reader = new StreamReader( stream ) )
			{
				var jsonResponse = reader.ReadToEnd();

				this.LogGetInfoResult( response.ResponseUri.OriginalString, ( ( HttpWebResponse )response ).StatusCode, jsonResponse, marker );

				if( string.IsNullOrEmpty( jsonResponse ) )
					return default(T);

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
				return jsonNetResult;
			}
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
				var url = string.Concat( config.NativeHost, BigCommerceCommand.GetOrdersCount.Command );
				this.GetResponse< BigCommerceItemsCount >( url, marker );
				return config.NativeHost;
			}
			catch( Exception )
			{
				try
				{
					var url = string.Concat( config.CustomHost, BigCommerceCommand.GetOrdersCount.Command );
					this.GetResponse< BigCommerceItemsCount >( url, marker );
					return config.CustomHost;
				}
				catch( Exception )
				{
					var clippedHost = config.CustomHost.Replace( "www.", string.Empty );
					var url = string.Concat( clippedHost, BigCommerceCommand.GetOrdersCount.Command );
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

		private void LogGetInfoResult( string url, HttpStatusCode statusCode, string jsonContent, string marker )
		{
			BigCommerceLogger.Log.Trace( "Marker: '{0}'. GET call for url '{1}' has been completed with code '{2}'.\n{3}", marker, url, statusCode, jsonContent );
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
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
		}
		#endregion
	}
}