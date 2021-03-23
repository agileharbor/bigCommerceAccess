using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Category
{
	[ DataContract ]
	public class BigCommerceCategory : BigCommerceObjectBase
	{
		[ DataMember( Name = "url") ]
		public BigCommerceCategoryURL Category_URL { get; set; }

		[ DataMember( Name = "name") ]
		public string Category_Name { get; set; }

		[DataMember(Name = "is_visible")]
		public bool IsVisible { get; set; }

		public BigCommerceCategory()
		{
			this.Category_URL = new BigCommerceCategoryURL();
			this.Category_Name = "";
		}
	}
}