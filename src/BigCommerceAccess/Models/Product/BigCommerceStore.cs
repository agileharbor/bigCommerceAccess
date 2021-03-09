using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[DataContract]
	public class BigCommerceStore
	{

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "domain")]
		public string Domain { get; set; }

		[DataMember(Name = "secure_URL")]
		public string SecureURL { get; set; }

		[DataMember(Name = "weight_units")]
		public string WeightUnits { get; set; }
	}
}