using System.Globalization;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Order
{
	[ DataContract ]
	public class BigCommerceOrderCoupon : BigCommerceObjectBase
	{
		[ DataMember( Name = "code" ) ]
		public string Code { get; set; }

		[ DataMember( Name = "type" ) ]
		public string Type { get; set; }

		[ DataMember( Name = "discount" ) ]
		public string DiscountValue { get; set; }
		public decimal Discount
		{
			get
			{	
				decimal discountAmount;
				decimal.TryParse( this.DiscountValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out discountAmount );
				return discountAmount;
			}
		}
	}
}
