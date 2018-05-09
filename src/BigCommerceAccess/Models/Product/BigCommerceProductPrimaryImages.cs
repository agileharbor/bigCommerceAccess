using System;
using System.Runtime.Serialization;

namespace BigCommerceAccess.Models.Product
{
	[ DataContract ]
	public class BigCommerceProductPrimaryImages
	{
		[ DataMember( Name = "standard_url" ) ]
		public string StandardUrl{ get; set; }

		public override int GetHashCode()
		{
			return this.StandardUrl.GetHashCode();
		}

		private bool Equals( BigCommerceProductPrimaryImages other )
		{
			return string.Equals( other.StandardUrl, this.StandardUrl, StringComparison.InvariantCultureIgnoreCase );
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			return obj is BigCommerceProductPrimaryImages && this.Equals( ( BigCommerceProductPrimaryImages )obj );
		}
	}
}