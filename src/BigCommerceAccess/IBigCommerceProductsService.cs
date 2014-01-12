using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Product;

namespace BigCommerceAccess
{
	public interface IBigCommerceProductsService
	{
		IEnumerable< BigCommerceProduct > GetProducts();
		Task< IEnumerable< BigCommerceProduct > > GetProductsAsync();

		void UpdateProducts( IEnumerable< BigCommerceProduct > products );
		Task UpdateProductsAsync( IEnumerable< BigCommerceProduct > products );
	}
}