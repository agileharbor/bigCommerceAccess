using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Order;

namespace BigCommerceAccess
{
	public interface IBigCommerceOrdersService
	{
		List< BigCommerceOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< List< BigCommerceOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );
	}
}