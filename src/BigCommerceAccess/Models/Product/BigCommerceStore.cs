using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceStore
	{
		[ DataMember( Name = "weight_units" ) ]
		public string WeightUnits{ get; set; }
	}
}