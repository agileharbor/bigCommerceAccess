using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using ServiceStack;

namespace BigCommerceAccess.Services
{
	internal class WebRequestServices
	{
		private readonly BigCommerceConfig _config;
		private string _host;

		public WebRequestServices( BigCommerceConfig config )
		{
			this._config = config;
			this._host = config.NativeHost;

			this.ResolveHost( string.Concat( config.NativeHost, BigCommerceCommand.GetOrdersCount.Command ) );
		}

		public T GetResponse< T >( string url, string commandParams )
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = this.GetResponse< T >( requestUrl );
			return result;
		}

		public async Task< T > GetResponseAsync< T >( string url, string commandParams )
		{
			var requestUrl = this.GetUrl( url, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl );
			return result;
		}

		public T GetResponse< T >( BigCommerceCommand command, string commandParams )
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = this.GetResponse< T >( requestUrl );
			return result;
		}

		public async Task< T > GetResponseAsync< T >( BigCommerceCommand command, string commandParams )
		{
			var requestUrl = this.GetUrl( command, commandParams );
			var result = await this.GetResponseAsync< T >( requestUrl );
			return result;
		}

		public T GetResponse< T >( string url )
		{
			this.LogGetInfo( url );

			T result;
			var request = this.CreateGetServiceGetRequest( url );
			using( var response = request.GetResponse() )
				result = ParseResponse< T >( response );
			return result;
		}

		public async Task< T > GetResponseAsync< T >( string url )
		{
			this.LogGetInfo( url );

			T result;
			var request = this.CreateGetServiceGetRequest( url );
			using( var response = await request.GetResponseAsync() )
				result = ParseResponse< T >( response );
			return result;
		}

		public void PutData( BigCommerceCommand command, string endpoint, string jsonContent )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogPutInfo( url, jsonContent );

			var request = this.CreateServicePutRequest( url, jsonContent );
			using( var response = ( HttpWebResponse )request.GetResponse() )
				this.LogPutInfoResult( url, response.StatusCode, jsonContent );
		}

		public async Task PutDataAsync( BigCommerceCommand command, string endpoint, string jsonContent )
		{
			var url = this.GetUrl( command, endpoint );
			this.LogPutInfo( url, jsonContent );

			var request = this.CreateServicePutRequest( url, jsonContent );
			using( var response = await request.GetResponseAsync() )
				this.LogPutInfoResult( url, ( ( HttpWebResponse )response ).StatusCode, jsonContent );
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
			this.AllowInvalidCertificate();

			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Get;
			request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );

			return request;
		}

		private HttpWebRequest CreateServicePutRequest( string url, string content )
		{
			this.AllowInvalidCertificate();

			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Put;
			request.ContentType = "application/json";
			request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );

			using( var writer = new StreamWriter( request.GetRequestStream() ) )
				writer.Write( content );

			return request;
		}
		#endregion

		#region Misc
		private T ParseResponse< T >( WebResponse response )
		{
			var result = default( T );

			using( var stream = response.GetResponseStream() )
			{
				var reader = new StreamReader( stream );
				var jsonResponse = reader.ReadToEnd();

				this.LogGetInfoResult( response.ResponseUri.OriginalString, ( ( HttpWebResponse )response ).StatusCode, jsonResponse );

				if( !String.IsNullOrEmpty( jsonResponse ) )
					result = jsonResponse.FromJson< T >();
			}

			return result;
		}

		private string CreateAuthenticationHeader()
		{
			var authInfo = string.Concat( this._config.UserName, ":", this._config.ApiKey );
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private void ResolveHost( string url )
		{
			try
			{
				this.GetResponse< BigCommerceItemsCount >( url );
			}
			catch( WebException )
			{
				if( url.Contains( this._config.NativeHost ) )
				{
					var customUrl = string.Concat( this._config.CustomHost, BigCommerceCommand.GetOrdersCount.Command );
					this._host = this._config.CustomHost;

					this.ResolveHost( customUrl );
				}
				else if( url.Contains( this._config.CustomHost ) )
				{
					var clippedHost = this._config.CustomHost.Replace( "www.", string.Empty );
					var customUrl = string.Concat( clippedHost, BigCommerceCommand.GetOrdersCount.Command );
					this._host = clippedHost;

					this.ResolveHost( customUrl );
				}
				else
					throw;
			}
		}

		private void LogGetInfo( string url )
		{
			BigCommerceLogger.Log.Trace( "[bigcommerce]\tGET data for url '{0}'", url );
		}

		private void LogGetInfoResult( string url, HttpStatusCode statusCode, string jsonContent )
		{
			BigCommerceLogger.Log.Trace( "[bigcommerce]\tGET call for url '{0}' has been completed with code '{1}'.\n{2}", url, statusCode, jsonContent );
		}

		private void LogPutInfo( string url, string jsonContent )
		{
			BigCommerceLogger.Log.Trace( "[bigcommerce]\tPUT data for url '{0}':\n{1}", url, jsonContent );
		}

		private void LogPutInfoResult( string url, HttpStatusCode statusCode, string jsonContent )
		{
			BigCommerceLogger.Log.Trace( "[bigcommerce]\tPUT/POST call for url '{0}' has been completed with code '{1}'.\n{2}", url, statusCode, jsonContent );
		}
		#endregion

		#region SSL certificate hack
		private void AllowInvalidCertificate()
		{
			ServicePointManager.ServerCertificateValidationCallback += AllowCert;
		}

		private bool AllowCert( object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error )
		{
			return true;
		}
		#endregion
	}
}