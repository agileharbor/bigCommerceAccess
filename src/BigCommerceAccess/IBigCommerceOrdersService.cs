using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Order;

namespace BigCommerceAccess
{
	public interface IBigCommerceOrdersService
	{
		IList< BigCommerceOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IList< BigCommerceOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );
	}
}