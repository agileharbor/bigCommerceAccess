using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Order
{
	[ DataContract ]
	public class OrdersCount
	{
		[ DataMember( Name = "count" ) ]
		public int Count { get; set; }
	}
}