using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Category
{
	[ DataContract ]
	public class BigCommerceCategoryInfo
	{
		[ DataMember( Name = "id" ) ]
		public int Id{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[DataMember(Name = "custom_url")]		
		public BigCommerceCategoryURL Category_URL { get; set; }

	}
}