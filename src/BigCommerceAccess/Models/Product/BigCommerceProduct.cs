using System.Collections.Generic;
using System.Runtime.Serialization;
using Netco.Extensions;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProduct: BigCommerceProductBase
	{
		[ DataMember( Name = "inventory_tracking" ) ]
		public InventoryTrackingEnum InventoryTracking{ get; set; }

		[ DataMember( Name = "skus" ) ]
		public BigCommerceReferenceObject ProductOptionsReference{ get; set; }

		public List< BigCommerceProductOption > ProductOptions{ get; set; }

		[ DataMember( Name = "upc" ) ]
		public string Upc{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[ DataMember( Name = "description" ) ]
		public string Description{ get; set; }

		[ DataMember( Name = "price" ) ]
		public decimal? Price{ get; set; }

		[ DataMember( Name = "sale_price" ) ]
		public decimal? SalePrice{ get; set; }

		[ DataMember( Name = "retail_price" ) ]
		public decimal? RetailPrice{ get; set; }

		[ DataMember( Name = "cost_price" ) ]
		public decimal? CostPrice{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public decimal? Weight{ get; set; }

		public string WeightUnit{ get; set; }

		[ DataMember( Name = "brand_id" ) ]
		public long? BrandId{ get; set; }

		public string BrandName{ get; set; }

		[ DataMember( Name = "primary_image" ) ]
		public BigCommerceProductPrimaryImages ImageUrls{ get; set; }

		public BigCommerceProduct()
		{
			this.ProductOptions = new List< BigCommerceProductOption >();
		}

		public override int GetHashCode()
		{
			var hashcode = this.InventoryTracking.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.ProductOptions.GetEnumerableHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Upc.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Name.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Description.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Price.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.SalePrice.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.RetailPrice.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.CostPrice.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Weight.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.WeightUnit.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.BrandId.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.BrandName.GetHashCode();
			//hashcode = ( hashcode * 397 ) ^ this.ImageUrls.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Quantity.GetHashCode();
			hashcode = ( hashcode * 397 ) ^ this.Sku.GetHashCode();

			return hashcode;
		}

		private bool Equals( BigCommerceProduct other )
		{
			var t1 = Equals( other.InventoryTracking, this.InventoryTracking );
			var t2 = other.ProductOptions.ListEqual( this.ProductOptions );
			var t3 = Equals( other.Upc, this.Upc );
			var t4 = Equals( other.Name, this.Name );
			var t5 = Equals( other.Description, this.Description );
			var t6 = Equals( other.Price, this.Price );
			var t7 = Equals( other.SalePrice, this.SalePrice );
			var t8 = Equals( other.RetailPrice, this.RetailPrice );
			var t9 = Equals( other.CostPrice, this.CostPrice );
			var t10 = Equals( other.Weight, this.Weight );
			var t11 = Equals( other.WeightUnit, this.WeightUnit );
			var t12 = Equals( other.BrandId, this.BrandId );
			var t13 = Equals( other.BrandName, this.BrandName );
			var t14 = Equals( other.Quantity, this.Quantity );
			var t15 = Equals( other.Sku, this.Sku );
			//var t16 = Equals(other.ImageUrls, this.ImageUrls);

			var t = t1 && t2 && t3 && t4 && t5 && t6 && t7 && t8 && t9 && t10 && t11 && t12 && t13 && t14 && t15/* && t16*/;
			return t;
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			return obj is BigCommerceProduct && this.Equals( ( BigCommerceProduct )obj );
		}
	}
}