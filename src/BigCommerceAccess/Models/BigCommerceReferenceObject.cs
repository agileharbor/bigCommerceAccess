using System.Runtime.Serialization;

namespace BigCommerceAccess.Models
{
	[ DataContract ]
	public sealed class BigCommerceReferenceObject
	{
		[ DataMember( Name = "url" ) ]
		public string Url { get; set; }

		[ DataMember( Name = "resource" ) ]
		public string Resource { get; set; }
	}
}