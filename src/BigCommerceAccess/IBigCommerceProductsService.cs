using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Product;

namespace BigCommerceAccess
{
	public interface IBigCommerceProductsService
	{
		List< BigCommerceProduct > GetProducts( bool includeExtendInfo = false );
		Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendInfo = false );

		void UpdateProducts( List< BigCommerceProduct > products );
		Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token );

		void UpdateProductOptions( List< BigCommerceProductOption > productOptions );
		Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token );

		//List< BigCommerceProductInfo > GetProductsInfo();
		//Task< List< BigCommerceProductInfo > > GetProductsInfoAsync( CancellationToken token );
	}
}