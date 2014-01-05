using LINQtoCSV;

namespace BigCommerceAccessTests
{
	internal class TestConfig
	{
		[ CsvColumn( Name = "ShopName", FieldIndex = 1 ) ]
		public string ShopName { get; set; }

		[ CsvColumn( Name = "UserName", FieldIndex = 2 ) ]
		public string UserName { get; set; }

		[ CsvColumn( Name = "ApiKey", FieldIndex = 3 ) ]
		public string ApiKey { get; set; }
	}
}