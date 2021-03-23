using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Category
{
	[ DataContract ]
	public class BigCommerceCategoryInfoData : BigCommerceObjectBase
	{
		[DataMember(Name = "data")]
		public BigCommerceCategoryInfo [] Data { get; set; }

		public BigCommerceCategoryInfoData()
		{
			this.Data = new BigCommerceCategoryInfo[500];
		}
	}
	
}