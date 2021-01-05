using System;
using System.IO;
using System.Net;
using BigCommerceAccess.Misc;
using FluentAssertions;
using NUnit.Framework;

namespace BigCommerceAccessTests
{
	[ TestFixture ]
	public class PageAdjusterTests
	{
		private const int DefaultPageSize = 250;
		private const int MinPageSize = 50;

		[ Test ]
		public void GivenPageWithDefaultSize_WhenGetHalfPageCalled_ThenHalfPageSizeIsReturned()
		{
			var currentPageSize = DefaultPageSize;
			var newPageSize = PageAdjuster.GetHalfPageSize( currentPageSize );
			newPageSize.Should().Be( DefaultPageSize / 2 );
		}

		[ Test ]
		public void GivenPageWithMinPage_WhenGetHalfPageCalled_ThenSamePageSizeIsReturned()
		{
			var currentPageSize = 1;
			var newPageSize = PageAdjuster.GetHalfPageSize( currentPageSize );
			newPageSize.Should().Be( currentPageSize );
		}

		[ Test ]
		public void GivenFirstPageWithHalfPageExpected_WhenGetNextPageIndexCalled_ThenNextPageIndexIsTheSame()
		{
			var firstPageIndex = 1;
			var newPageIndex = PageAdjuster.GetNextPageIndex( new PageInfo( firstPageIndex, DefaultPageSize ), 125 );
			newPageIndex.Should().Be( firstPageIndex );
		}

		[ Test ]
		public void GivenPageWithHalfPageExpected_WhenGetNextPageIndexCalled_ThenNextPageIndexIsRecalculatedCorrectly()
		{
			var currentPageIndex = 5;
			var newPageIndex = PageAdjuster.GetNextPageIndex( new PageInfo( currentPageIndex, DefaultPageSize ), 125 );
			newPageIndex.Should().Be( currentPageIndex * 2 - 1 );
		}

		[ Test ]
		public void GivenPageWithHalfPageExpectedAndNotDefaultCurrentPage_WhenGetNextPageIndexCalled_ThenNextPageIndexIsRecalculatedCorrectly()
		{
			var currentPageIndex = 5;
			var currentPageSize = 125;
			var newPageIndex = PageAdjuster.GetNextPageIndex( new PageInfo( currentPageIndex, currentPageSize ), 62 );
			newPageIndex.Should().Be( currentPageIndex * 2 - 1 );
		}

		[ Test ]
		public void GivenResponseWithNullException_WhenIsResponseTooLargeCalled_ThenNegativeResultExpected()
		{
			bool isResponseTooLarge = PageAdjuster.IsResponseTooLargeToRead( new Exception() );
			isResponseTooLarge.Should().Be( false );
		}

		[ Test ]
		public void GivenResponseWithCommonException_WhenIsResponseTooLargeCalled_ThenNegativeResultExpected()
		{
			bool isResponseTooLarge = PageAdjuster.IsResponseTooLargeToRead( new Exception( null, new Exception() ) );
			isResponseTooLarge.Should().Be( false );
		}

		[ Test ]
		public void GivenResponseWithIOException_WhenIsResponseTooLargeCalled_ThenPositiveResultExpected()
		{
			bool isResponseTooLarge = PageAdjuster.IsResponseTooLargeToRead( new Exception( string.Empty, new IOException() ) );
			isResponseTooLarge.Should().Be( true );
		}

		[ Test ]
		public void GivenResponseWithWebException_WhenIsResponseTooLargeCalled_ThenPositiveResultExpected()
		{
			bool isResponseTooLarge = PageAdjuster.IsResponseTooLargeToRead( new Exception( string.Empty, new WebException( string.Empty, WebExceptionStatus.ConnectionClosed ) ) );
			isResponseTooLarge.Should().Be( true );
		}

		[ Test ]
		public void GivenResponseWithIOException_WhenTryAdjustPageInfoCalled_ThenAdjustedPageInfoIsReturned()
		{
			var initialPageIndex = 5;
			var currentPageInfo = new PageInfo( initialPageIndex, DefaultPageSize );
			var responseException = new Exception( string.Empty, new IOException( "Unable to read data" ) );
			var isResponseTooLarge = PageAdjuster.TryAdjustPageIfResponseTooLarge( currentPageInfo, MinPageSize, responseException, out PageInfo newPageInfo );
			
			isResponseTooLarge.Should().Be( true );
			newPageInfo.Size.Should().Be( DefaultPageSize / 2 );
			newPageInfo.Index.Should().Be( initialPageIndex * 2 - 1 );
		}

		[ Test ]
		public void GivenResponseWithWebExceptionAndStatusConnectionClosed_WhenTryAdjustPageInfoCalled_ThenAdjustedPageInfoIsReturned()
		{
			var initialPageIndex = 5;
			var currentPageInfo = new PageInfo( initialPageIndex, DefaultPageSize );
			var responseException = new Exception( string.Empty, new WebException( "Unable to read data", WebExceptionStatus.ConnectionClosed ) );
			var isResponseTooLarge = PageAdjuster.TryAdjustPageIfResponseTooLarge( currentPageInfo, MinPageSize, responseException, out PageInfo newPageInfo );
			
			isResponseTooLarge.Should().Be( true );
			newPageInfo.Size.Should().Be( DefaultPageSize / 2 );
			newPageInfo.Index.Should().Be( initialPageIndex * 2 - 1 );
		}

		[ Test ]
		public void GivenResponseWithCommonException_WhenTryAdjustPageInfoCalled_ThenTheSamePageInfoIsReturned()
		{
			var currentPageInfo = new PageInfo( 5, DefaultPageSize );
			var responseException = new Exception( "some unrepeatable error happened" );
			var isResponseTooLarge = PageAdjuster.TryAdjustPageIfResponseTooLarge( currentPageInfo, MinPageSize, responseException, out PageInfo newPageInfo );
			
			isResponseTooLarge.Should().Be( false );
			newPageInfo.Size.Should().Be( currentPageInfo.Size );
			newPageInfo.Index.Should().Be( currentPageInfo.Index );
		}

		[ Test ]
		public void GivenResponseWithInnerCommonException_WhenTryAdjustPageInfoCalled_ThenTheSamePageInfoIsReturned()
		{
			var currentPageInfo = new PageInfo( 5, DefaultPageSize );
			var responseException = new Exception( string.Empty, new Exception( "some unrepeatable error happened" ) );
			var isResponseTooLarge = PageAdjuster.TryAdjustPageIfResponseTooLarge( currentPageInfo, MinPageSize, responseException, out PageInfo newPageInfo );
			
			isResponseTooLarge.Should().Be( false );
			newPageInfo.Size.Should().Be( currentPageInfo.Size );
			newPageInfo.Index.Should().Be( currentPageInfo.Index );
		}

		[ Test ]
		public void GivenResponseWithIOExceptionAndMinPageSize_WhenTryAdjustPageInfoCalled_ThenTheSamePageIsReturned()
		{
			var currentPageInfo = new PageInfo( 5, 62 );
			var responseException = new Exception( string.Empty, new IOException( "Unable to read data" ) );
			var isResponseTooLarge = PageAdjuster.TryAdjustPageIfResponseTooLarge( currentPageInfo, MinPageSize, responseException, out PageInfo newPageInfo );
			
			isResponseTooLarge.Should().Be( false );
			newPageInfo.Size.Should().Be( currentPageInfo.Size );
			newPageInfo.Index.Should().Be( currentPageInfo.Index );
		}
	}
}