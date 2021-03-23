using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceCustomURL
	{
		[ DataMember( Name = "is_customized") ]
		public bool UrlStandard{ get; set; }

		[ DataMember( Name = "url") ]
		public string ProductURL { get; set; }
	}
}