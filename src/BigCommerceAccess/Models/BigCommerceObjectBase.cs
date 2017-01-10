using System.Runtime.Serialization;

namespace BigCommerceAccess.Models
{
	[ DataContract ]
	public abstract class BigCommerceObjectBase
	{
		[ DataMember( Name = "id" ) ]
		public long Id{ get; set; }
	}
}