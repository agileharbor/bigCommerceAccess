﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Product;

namespace BigCommerceAccess
{
	public interface IBigCommerceProductsService
	{
		string GetStoreName();

		string GetStoreDomain();

		string GetStoreSafeURL();
		List< BigCommerceProduct > GetProducts( bool includeExtendInfo = false );
		Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendInfo = false );

		void UpdateProducts( List< BigCommerceProduct > products );
		Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token );

		void UpdateProductOptions( List< BigCommerceProductOption > productOptions );
		Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token );
	}
}