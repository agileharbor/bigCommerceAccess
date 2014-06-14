using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProductOption : BigCommerceProductBase
	{
		[ DataMember( Name = "product_id" ) ]
		public long ProductId { get; set; }
	}
}