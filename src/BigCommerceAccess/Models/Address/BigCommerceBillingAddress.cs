using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Address
{
	[ DataContract ]
	public class BigCommerceBillingAddress
	{
		[ DataMember( Name = "first_name" ) ]
		public string FirstName { get; set; }

		[ DataMember( Name = "last_name" ) ]
		public string LastName { get; set; }

		[ DataMember( Name = "company" ) ]
		public string Company { get; set; }

		[ DataMember( Name = "phone" ) ]
		public string Phone { get; set; }

		[ DataMember( Name = "email" ) ]
		public string Email { get; set; }
	}
}