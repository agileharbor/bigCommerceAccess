using LINQtoCSV;

namespace BigCommerceAccessTests
{
	internal class TestConfig
	{
		[ CsvColumn( Name = "ShopName", FieldIndex = 1 ) ]
		public string ShopName{ get; set; }

		[ CsvColumn( Name = "UserName", FieldIndex = 2 ) ]
		public string UserName{ get; set; }

		[ CsvColumn( Name = "ApiKey", FieldIndex = 3 ) ]
		public string ApiKey{ get; set; }

		[ CsvColumn( Name = "ShortShopName", FieldIndex = 4 ) ]
		public string ShortShopName{ get; set; }

		[ CsvColumn( Name = "ClientId", FieldIndex = 5 ) ]
		public string ClientId{ get; set; }

		[ CsvColumn( Name = "ClientSecret", FieldIndex = 6 ) ]
		public string ClientSecret{ get; set; }

		[ CsvColumn( Name = "Token", FieldIndex = 7 ) ]
		public string Token{ get; set; }
	}
}