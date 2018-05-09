using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProductOption: BigCommerceProductBase
	{
		[ DataMember( Name = "product_id" ) ]
		public long ProductId{ get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "price" ) ]
		public decimal? Price{ get; set; }

		[ DataMember( Name = "adjusted_price" ) ]
		public decimal? AdjustedPrice{ get; set; }

		[ DataMember( Name = "cost_price" ) ]
		public decimal? CostPrice{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public decimal? Weight{ get; set; }

		[ DataMember( Name = "adjusted_weight" ) ]
		public decimal? AdjustedWeight{ get; set; }

		[ DataMember( Name = "image_file" ) ]
		public string ImageFile{ get; set; }

		public override int GetHashCode()
		{
			var hashcode = this.Quantity.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Sku.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.ProductId.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Upc.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Price.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.AdjustedPrice.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.CostPrice.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Weight.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.AdjustedWeight.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.ImageFile.GetHashCode();

			return hashcode;
		}

		private bool Equals( BigCommerceProductOption other )
		{
			return other.Quantity.Equals( this.Quantity ) &&
			       other.Sku.Equals( this.Sku ) &&
			       other.ProductId.Equals( this.ProductId ) &&
			       other.Upc.Equals( this.Upc ) &&
			       other.Price.Equals( this.Price ) &&
			       //other.AdjustedPrice.Equals( this.AdjustedPrice ) &&
			       other.CostPrice.Equals( this.CostPrice ) &&
			       other.Weight.Equals( this.Weight ) &&
			       //other.AdjustedWeight.Equals( this.AdjustedWeight ) &&
			       other.ImageFile.Equals( this.ImageFile );
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			return obj is BigCommerceProductOption && this.Equals( ( BigCommerceProductOption )obj );
		}
	}
}