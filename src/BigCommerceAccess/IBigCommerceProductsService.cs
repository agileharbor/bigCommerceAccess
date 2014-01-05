using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Product;

namespace BigCommerceAccess
{
	public interface IBigCommerceProductsService
	{
		IList< BigCommerceProduct > GetProducts();
		Task< IList< BigCommerceProduct > > GetProductsAsync();

		void UpdateProducts( IEnumerable< BigCommerceProduct > products );
		Task UpdateProductsAsync( IEnumerable< BigCommerceProduct > products );
	}
}