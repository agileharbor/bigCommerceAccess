using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Product;

namespace BigCommerceAccess
{
	public interface IBigCommerceProductsService
	{
		List< BigCommerceProduct > GetProducts();
		Task< List< BigCommerceProduct > > GetProductsAsync();

		void UpdateProducts( List< BigCommerceProduct > products );
		Task UpdateProductsAsync( List< BigCommerceProduct > products );

		void UpdateProductOptions( List< BigCommerceProductOption > productOptions );
		Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions );
	}
}