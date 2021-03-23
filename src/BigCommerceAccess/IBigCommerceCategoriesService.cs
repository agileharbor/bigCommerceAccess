using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Category;

namespace BigCommerceAccess
{
	public interface IBigCommerceCategoriesService
	{
		List< BigCommerceCategory > GetCategories();
		Task< List<BigCommerceCategory> > GetCategoriesAsync( CancellationToken token);
		
	}
}