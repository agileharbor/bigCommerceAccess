namespace BigCommerceAccess.Models.Configuration
{
	internal class BigCommerceCommandParamName
	{
		public static readonly BigCommerceCommandParamName Unknown = new BigCommerceCommandParamName( string.Empty );
		public static readonly BigCommerceCommandParamName Limit = new BigCommerceCommandParamName( "limit" );
		public static readonly BigCommerceCommandParamName Page = new BigCommerceCommandParamName( "page" );

		private BigCommerceCommandParamName( string name )
		{
			this.Name = name;
		}

		public string Name { get; private set; }
	}
}