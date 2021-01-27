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

		public static void LogTraceException( ResponseInfo responseInfo, Exception exception )
		{
			Log().Trace( exception, "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Request '{callMarker}' to url '{callUrl}' failed. Response status code: {callResponseStatusCode}", 
									ChannelName, 
									_versionInfo, 
									responseInfo.TenantId ?? 0,
									responseInfo.ChannelAccountId ?? 0,
									responseInfo.Category, 
									responseInfo.LibMethodName,
									responseInfo.Mark, 
									responseInfo.Url, 
									responseInfo.StatusCode );
		}

		public static void LogTraceException( RetryInfo retryInfo, Exception exception )
		{
			Log().Trace( exception, "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Request '{callMarker}' to url '{callUrl}' failed. Gonna retry request for the {callRetryAttempt} attempt, delay {callRetryDelay} seconds, total retry attempts {callRetryTotalAttempts}", 
									ChannelName, 
									_versionInfo, 
									retryInfo.TenantId ?? 0,
									retryInfo.ChannelAccountId ?? 0,
									retryInfo.Category, 
									retryInfo.LibMethodName,
									retryInfo.Mark, 
									retryInfo.Url, 
									retryInfo.CurrentRetryAttempt, 
									retryInfo.DelayInSeconds, 
									retryInfo.TotalRetriesAttempts );
		}

		public static void TraceLog( RequestInfo requestInfo )
		{
			if ( !string.IsNullOrWhiteSpace( requestInfo.Body?.ToString() ) )
			{
				Log().Trace( "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Starting {callHttpMethod} call '{callMarker}' to '{callUrl}' with body: '{callRequestBody}'", 
								ChannelName, 
								_versionInfo, 
								requestInfo.TenantId ?? 0,
								requestInfo.ChannelAccountId ?? 0,
								requestInfo.Category, 
								requestInfo.LibMethodName, 
								requestInfo.HttpMethod.ToString().ToUpper(), 
								requestInfo.Mark, 
								requestInfo.Url, 
								requestInfo.Body ?? string.Empty );
				return;
			}
			
			Log().Trace( "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Starting {callHttpMethod} call '{callMarker}' to '{callUrl}'", 
							ChannelName, 
							_versionInfo, 
							requestInfo.TenantId ?? 0,
							requestInfo.ChannelAccountId ?? 0,
							requestInfo.Category, 
							requestInfo.LibMethodName, 
							requestInfo.HttpMethod.ToString().ToUpper(),
							requestInfo.Mark,  
							requestInfo.Url );
		}

		public static void TraceLog( ResponseInfo responseInfo )
		{
			Log().Trace( "[{channel}] [{version}] [{tenantId}] [{accountId}] [{callCategory}] [{callLibMethodName}] Completed call '{callMarker}' to '{callUrl}'. Response status code: {callResponseStatusCode}, api calls remaining: {callRemainingCalls}, system version: {callExternalSystemVersion}. Response body: '{callResponseBody}'", 
							ChannelName,
							_versionInfo, 
							responseInfo.TenantId ?? 0,
							responseInfo.ChannelAccountId ?? 0,
							responseInfo.Category,
							responseInfo.LibMethodName,
							responseInfo.Mark, 
							responseInfo.Url, 
							responseInfo.StatusCode, 
							responseInfo.RemainingCalls, 
							responseInfo.SystemVersion, 
							responseInfo.Response != null ? responseInfo.Response.ToJson() : string.Empty );
		}
	}
}