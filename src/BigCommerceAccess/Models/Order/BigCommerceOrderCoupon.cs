using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Order
{
	[ DataContract ]
	public class BigCommerceOrderCoupon : BigCommerceObjectBase
	{
		[ DataMember( Name = "code" ) ]
		public string Code { get; set; }

		[ DataMember( Name = "type" ) ]
		public string Type { get; set; }

		[ DataMember( Name = "discount" ) ]
		public string Discount { get; set; }
	}
}
