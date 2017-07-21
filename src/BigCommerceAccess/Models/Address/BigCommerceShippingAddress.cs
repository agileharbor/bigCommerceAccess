using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Address
{
	[ DataContract ]
	public class BigCommerceShippingAddress
	{
		[ DataMember( Name = "street_1" ) ]
		public string Street1 { get; set; }

		[ DataMember( Name = "street_2" ) ]
		public string Street2 { get; set; }

		[ DataMember( Name = "city" ) ]
		public string City { get; set; }

		[ DataMember( Name = "zip" ) ]
		public string Zip { get; set; }

		[ DataMember( Name = "state" ) ]
		public string State { get; set; }

		[ DataMember( Name = "country" ) ]
		public string Country { get; set; }

		[ DataMember( Name = "country_iso2" ) ]
		public string CountryIso2 { get; set; }

		[ DataMember( Name = "shipping_method" ) ]
		public string ShippingMethod{ get; set; }

		[ DataMember( Name = "first_name" ) ]
		public string FirstName{ get; set; }

		[ DataMember( Name = "last_name" ) ]
		public string LastName{ get; set; }

		[ DataMember( Name = "company" ) ]
		public string Company{ get; set; }

		[ DataMember( Name = "phone" ) ]
		public string Phone{ get; set; }

		[ DataMember( Name = "email" ) ]
		public string Email{ get; set; }
	}
}