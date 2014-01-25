using System.Collections.Generic;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Services;
using CuttingEdge.Conditions;
using ServiceStack;

namespace BigCommerceAccess
{
	public class BigCommerceProductsService : BigCommerceServiceBase, IBigCommerceProductsService
	{
		private readonly WebRequestServices _webRequestServices;

		public BigCommerceProductsService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this._webRequestServices = new WebRequestServices( config );
		}

		#region Get
		public IEnumerable< BigCommerceProduct > GetProducts()
		{
			IList< BigCommerceProduct > products;
			var productsCount = this.GetProductsCount();

			if( productsCount > RequestMaxLimit )
				products = this.CollectProductsFromAllPages( productsCount );
			else
				products = this.CollectProductsFromSinglePage();

			return products;
		}

		public async Task< IEnumerable< BigCommerceProduct > > GetProductsAsync()
		{
			IList< BigCommerceProduct > products;
			var productsCount = await this.GetProductsCountAsync();

			if( productsCount > RequestMaxLimit )
				products = await this.CollectProductsFromAllPagesAsync( productsCount );
			else
				products = await this.CollectProductsFromSinglePageAsync();


			return products;
		}

		private IList< BigCommerceProduct > CollectProductsFromAllPages( int productsCount )
		{
			var pagesCount = this.CalculatePagesCount( productsCount );
			var products = new List< BigCommerceProduct >();

			for( var i = 0; i < pagesCount; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i + 1, RequestMaxLimit ) );

				ActionPolicies.Submit.Do( () =>
				{
					var productsWithinPage = this._webRequestServices.GetResponse< IList< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint );
					products.AddRange( productsWithinPage );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}

			return products;
		}

		private IList< BigCommerceProduct > CollectProductsFromSinglePage()
		{
			IList< BigCommerceProduct > products = null;
			var endpoint = ParamsBuilder.CreateGetSinglePageParams( new BigCommerceCommandConfig( RequestMaxLimit ) );

			ActionPolicies.Submit.Do( () =>
			{
				products = this._webRequestServices.GetResponse< IList< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint );

				//API requirement
				this.CreateApiDelay().Wait();
			} );

			return products;
		}

		private async Task< IList< BigCommerceProduct > > CollectProductsFromAllPagesAsync( int productsCount )
		{
			var pagesCount = this.CalculatePagesCount( productsCount );
			var products = new List< BigCommerceProduct >();

			for( var i = 0; i < pagesCount; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i + 1, RequestMaxLimit ) );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var productsWithinPage = await this._webRequestServices.GetResponseAsync< IList< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint );
					products.AddRange( productsWithinPage );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}

			return products;
		}

		private async Task< IList< BigCommerceProduct > > CollectProductsFromSinglePageAsync()
		{
			IList< BigCommerceProduct > products = null;
			var endpoint = ParamsBuilder.CreateGetSinglePageParams( new BigCommerceCommandConfig( RequestMaxLimit ) );

			await ActionPolicies.GetAsync.Do( async () =>
			{
				products = await this._webRequestServices.GetResponseAsync< IList< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint );

				//API requirement
				this.CreateApiDelay().Wait();
			} );

			return products;
		}
		#endregion

		#region Update
		public void UpdateProducts( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products )
			{
				var productToUpdate = product;
				this.UpdateProductQuantity( productToUpdate );
			}
		}

		public async Task UpdateProductsAsync( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products )
			{
				var productToUpdate = product;
				await this.UpdateProductQuantityAsync( productToUpdate );
			}
		}

		private void UpdateProductQuantity( BigCommerceProduct product )
		{
			var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
			var jsonContent = new { inventory_level = product.Quantity }.ToJson();

			ActionPolicies.Submit.Do( () =>
			{
				this._webRequestServices.PutData( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

				//API requirement
				this.CreateApiDelay().Wait();
			} );
		}

		private async Task UpdateProductQuantityAsync( BigCommerceProduct product )
		{
			var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
			var jsonContent = new { inventory_level = product.Quantity }.ToJson();

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );
				//API requirement
				this.CreateApiDelay().Wait();
			} );

			//API requirement
			this.CreateApiDelay().Wait();
		}
		#endregion

		#region Count
		private int GetProductsCount()
		{
			var count = 0;
			ActionPolicies.Get.Do( () =>
			{
				count = this._webRequestServices.GetResponse< ProductsCount >( BigCommerceCommand.GetProductsCount, ParamsBuilder.EmptyParams ).Count;

				//API requirement
				this.CreateApiDelay().Wait();
			} );
			return count;
		}

		private async Task< int > GetProductsCountAsync()
		{
			var count = 0;
			await ActionPolicies.GetAsync.Do( async () =>
			{
				count = ( await this._webRequestServices.GetResponseAsync< ProductsCount >( BigCommerceCommand.GetProductsCount, ParamsBuilder.EmptyParams ) ).Count;

				//API requirement
				this.CreateApiDelay().Wait();
			} );
			return count;
		}
		#endregion
	}
}