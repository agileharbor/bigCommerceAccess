using System.Runtime.Serialization;

namespace BigCommerceAccess.Models
{
	public abstract class BigCommerceObjectBase
	{
		[ DataMember( Name = "id" ) ]
		public long Id { get; set; }
	}
}