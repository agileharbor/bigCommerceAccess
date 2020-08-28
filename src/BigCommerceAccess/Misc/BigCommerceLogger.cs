using System;
using System.Diagnostics;
using System.Reflection;
using Netco.Logging;
using ServiceStack;

namespace BigCommerceAccess.Misc
{
	public class BigCommerceLogger
	{
		private static readonly string _versionInfo;
		private const string ChannelName = "bigCommerce";
		private const int MaxLogLineSize = 0xA00000; //10mb

		static BigCommerceLogger()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			_versionInfo = FileVersionInfo.GetVersionInfo( assembly.Location ).FileVersion;
		}

		public static ILogger Log()
		{
			return NetcoLogger.GetLogger( "BigCommerceLogger" );
		}

		public static void LogTraceException( CallInfo callInfo, Exception exception )
		{
			if ( callInfo is ResponseInfo responseInfo )
			{
				Log().Trace( exception, "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Request '{callMarker}' to url '{callUrl}' failed. Response status code: {callResponseStatusCode}", 
					ChannelName, 
					_versionInfo, 
					callInfo.TenantId ?? 0,
					callInfo.ChannelAccountId ?? 0,
					responseInfo.Category, 
					responseInfo.LibMethodName,
					responseInfo.Mark, 
					responseInfo.Url, 
					responseInfo.StatusCode );
			}

			if ( callInfo is RetryInfo retryInfo )
			{
				Log().Trace( exception, "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Request '{callMarker}' to url '{callUrl}' failed. Gonna retry request for the {callRetryAttempt} attempt, delay {callRetryDelay} seconds, total retry attempts {callRetryTotalAttempts}", 
					ChannelName, 
					_versionInfo, 
					callInfo.TenantId ?? 0,
					callInfo.ChannelAccountId ?? 0,
					retryInfo.Category, 
					retryInfo.LibMethodName,
					retryInfo.Mark, 
					retryInfo.Url, 
					retryInfo.CurrentRetryAttempt, 
					retryInfo.DelayInSeconds, 
					retryInfo.TotalRetriesAttempts );
			}
		}

		public static void TraceLog( CallInfo callInfo )
		{
			var requestInfo = callInfo as RequestInfo;
			if ( requestInfo != null )
			{
				if ( !string.IsNullOrWhiteSpace( requestInfo.Body?.ToString() ) )
				{
					Log().Trace( "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Starting {callHttpMethod} call '{callMarker}' to '{callUrl}' with body: '{callRequestBody}'", 
						ChannelName, 
						_versionInfo, 
						callInfo.TenantId ?? 0,
						callInfo.ChannelAccountId ?? 0,
						requestInfo.Category, 
						requestInfo.LibMethodName, 
						requestInfo.HttpMethod.ToString().ToUpper(), 
						requestInfo.Mark, 
						requestInfo.Url, 
						requestInfo.Body ?? string.Empty );
				}
				else
				{
					Log().Trace( "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Starting {callHttpMethod} call '{callMarker}' to '{callUrl}'", 
						ChannelName, 
						_versionInfo, 
						callInfo.TenantId ?? 0,
						callInfo.ChannelAccountId ?? 0,
						requestInfo.Category, 
						requestInfo.LibMethodName, 
						requestInfo.HttpMethod.ToString().ToUpper(),
						requestInfo.Mark,  
						requestInfo.Url );
				}
				
				return;
			}
			
			var responseInfo = callInfo as ResponseInfo;
			if ( responseInfo != null )
			{
				Log().Trace( "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Completed call '{callMarker}' to '{callUrl}'. Response status code: {callResponseStatusCode}, api calls remaining: {callRemainingCalls}, system version: {callExternalSystemVersion}. Response body: '{callResponseBody}'", 
					ChannelName,
					_versionInfo, 
					callInfo.TenantId ?? 0,
					callInfo.ChannelAccountId ?? 0,
					responseInfo.Category,
					callInfo.LibMethodName,
					callInfo.Mark, 
					callInfo.Url, 
					responseInfo.StatusCode, 
					responseInfo.RemainingCalls, 
					responseInfo.SystemVersion, 
					responseInfo.Response != null ? responseInfo.Response.ToJson() : string.Empty );
			}
		}
	}
}