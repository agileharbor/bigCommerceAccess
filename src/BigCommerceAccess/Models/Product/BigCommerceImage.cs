using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceImage
	{
		[ DataMember( Name = "url_standard" ) ]
		public string UrlStandard{ get; set; }

		[ DataMember( Name = "url_zoom" ) ]
		public string UrlZoom{ get; set; }

		[ DataMember( Name = "url_thumbnail" ) ]
		public string UrlThumbnail{ get; set; }

		[ DataMember( Name = "url_tiny" ) ]
		public string UrlTiny{ get; set; }

		[ DataMember( Name = "image_file" ) ]
		public string ImageFile{ get; set; }
	}
}