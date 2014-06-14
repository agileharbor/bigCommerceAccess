using System.Runtime.Serialization;

namespace BigCommerceAccess.Models
{
	[ DataContract ]
	sealed class BigCommerceItemsCount
	{
		[ DataMember( Name = "count" ) ]
		public int Count { get; set; }
	}
}