using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using Netco.Logging;
using ServiceStack;

namespace BigCommerceAccess.Services
{
	internal class WebRequestServices
	{
		private readonly BigCommerceConfig _config;

		public WebRequestServices( BigCommerceConfig config )
		{
			this._config = config;
		}

		public T GetResponse< T >( BigCommerceCommand command, string commandParams )
		{
			T result;
			var request = this.CreateGetServiceGetRequest( string.Concat( this._config.Host, command.Command, commandParams ) );
			using( var response = request.GetResponse() )
				result = ParseResponse< T >( response );

			return result;
		}

		public T GetResponse< T >( string url )
		{
			T result;
			var request = this.CreateGetServiceGetRequest( url );
			using( var response = request.GetResponse() )
				result = ParseResponse< T >( response );

			return result;
		}

		public async Task< T > GetResponseAsync< T >( BigCommerceCommand command, string commandParams )
		{
			T result;
			var request = this.CreateGetServiceGetRequest( string.Concat( this._config.Host, command.Command, commandParams ) );
			using( var response = await request.GetResponseAsync() )
				result = ParseResponse< T >( response );

			return result;
		}

		public async Task< T > GetResponseAsync< T >( string url )
		{
			T result;
			var request = this.CreateGetServiceGetRequest( url );
			using( var response = await request.GetResponseAsync() )
				result = ParseResponse< T >( response );

			return result;
		}

		public void PutData( BigCommerceCommand command, string endpoint, string jsonContent )
		{
			var request = this.CreateServicePutRequest( command, endpoint, jsonContent );
			using( var response = ( HttpWebResponse )request.GetResponse() )
				this.LogUpdateInfo( endpoint, response.StatusCode, jsonContent );
		}

		public async Task PutDataAsync( BigCommerceCommand command, string endpoint, string jsonContent )
		{
			var request = this.CreateServicePutRequest( command, endpoint, jsonContent );
			using( var response = await request.GetResponseAsync() )
				this.LogUpdateInfo( endpoint, ( ( HttpWebResponse )response ).StatusCode, jsonContent );
		}

		#region WebRequest configuration
		private HttpWebRequest CreateGetServiceGetRequest( string url )
		{
			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Get;
			request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );

			return request;
		}

		private HttpWebRequest CreateServicePutRequest( BigCommerceCommand command, string endpoint, string content )
		{
			var uri = new Uri( string.Concat( this._config.Host, command.Command, endpoint ) );
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

				this.Log().Trace( "[bigcommerce]\tResponse\t{0} - {1}", response.ResponseUri, jsonResponse );

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

		private void LogUpdateInfo( string url, HttpStatusCode statusCode, string jsonContent )
		{
			this.Log().Trace( "[bigcommerce]\tPUT/POST call for the url '{0}' has been completed with code '{1}'.\n{2}", url, statusCode, jsonContent );
		}
		#endregion
	}
}