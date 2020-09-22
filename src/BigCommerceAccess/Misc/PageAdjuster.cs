using System;
using System.IO;
using System.Net;

namespace BigCommerceAccess.Misc
{
	public static class PageAdjuster
	{
		public static int GetHalfPageSize( int currentPageSize )
		{
			return Math.Max( (int)Math.Floor( currentPageSize / 2d ), 1 );
		}

		public static int GetNextPageIndex( PageInfo currentPageInfo, int newPageSize )
		{
			var entitiesReceived = currentPageInfo.Size * ( currentPageInfo.Index - 1 );
			return (int)Math.Floor( entitiesReceived * 1.0 / newPageSize ) + 1;
		}

		public static bool TryAdjustPageIfResponseTooLarge( PageInfo currentPageInfo, int minPageSize, Exception ex, out PageInfo newPageInfo )
		{
			newPageInfo = currentPageInfo;

			if ( IsResponseTooLargeToRead( ex ) )
			{
				var newPageSize = PageAdjuster.GetHalfPageSize( currentPageInfo.Size );
				if ( newPageSize >= minPageSize )
				{
					newPageInfo.Index = PageAdjuster.GetNextPageIndex( currentPageInfo, newPageSize );
					newPageInfo.Size = newPageSize;

					return true;
				}
			}

			return false;
		}

		public static bool IsResponseTooLargeToRead( Exception ex )
		{
			if ( ex?.InnerException == null )
				return false;

			if ( ex.InnerException is IOException )
				return true;

			var webEx = ex.InnerException as WebException;
			if ( webEx != null )
			{
				return webEx.Status == WebExceptionStatus.ConnectionClosed;
			}

			return false;
		}
	}

	public struct PageInfo
	{
		public PageInfo( int index, int size )
		{
			this.Index = index;
			this.Size = size;
		}

		public int Index { get; set; }
		public int Size { get; set; }
	}
}