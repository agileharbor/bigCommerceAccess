using CuttingEdge.Conditions;

namespace BigCommerceAccess.Models.Configuration
{
	internal class BigCommerceCommandConfig
	{
		public int Page { get; private set; }
		public int Limit { get; private set; }

		public BigCommerceCommandConfig( int page, int limit )
			: this( limit )
		{
			Condition.Requires( page, "page" ).IsGreaterThan( 0 );
			Condition.Requires( limit, "limit" ).IsGreaterThan( 0 );

			this.Page = page;
		}

		public BigCommerceCommandConfig( int limit )
		{
			Condition.Requires( limit, "limit" ).IsGreaterThan( 0 );

			this.Limit = limit;
		}
	}
}