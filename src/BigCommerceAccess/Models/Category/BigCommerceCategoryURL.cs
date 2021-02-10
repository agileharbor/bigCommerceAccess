using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Category
{
	[ DataContract ]
	public class BigCommerceCategoryURL
	{
		[ DataMember( Name = "is_customized") ]
		public bool isCustomized{ get; set; }

		[ DataMember( Name = "url") ]
		public string Url { get; set; }
	}
}