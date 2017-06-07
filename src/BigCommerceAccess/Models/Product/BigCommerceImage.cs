using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceImage
	{
		[ DataMember( Name = "url_standard" ) ]
		public string UrlStandard{ get; set; }
	}
}