using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models;
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

			this.FillProductsSkus( products );

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

			await this.FillProductsSkusAsync( products );

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

		private void FillProductsSkus( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) ) )
			{
				var pageNumber = 1;
				var hasMoreOptions = false;
				var p = product;

				do
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( pageNumber++, RequestMaxLimit ) );
					ActionPolicies.Get.Do( () =>
					{
						var options = this._webRequestServices.GetResponse< IList< BigCommerceProductOption > >( p.ProductOptionsReference.Url, endpoint );

						if( options != null )
							p.ProductOptions.AddRange( options );

						hasMoreOptions = options != null && options.Count == RequestMaxLimit;

						//API requirement
						this.CreateApiDelay().Wait();
					} );
				} while( hasMoreOptions );
			}
		}

		private async Task FillProductsSkusAsync( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) ) )
			{
				var pageNumber = 1;
				var hasMoreOptions = true;
				var p = product;

				do
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( pageNumber++, RequestMaxLimit ) );
					await ActionPolicies.GetAsync.Do( async () =>
					{
						var options = await this._webRequestServices.GetResponseAsync< IList< BigCommerceProductOption > >( p.ProductOptionsReference.Url, endpoint );

						if( options != null )
							p.ProductOptions.AddRange( options );

						hasMoreOptions = options != null && options.Count == RequestMaxLimit;

						//API requirement
						this.CreateApiDelay().Wait();
					} );
				} while( hasMoreOptions );
			}
		}
		#endregion

		#region Update
		public void UpdateProducts( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products )
				this.UpdateProductQuantity( product );
		}

		public async Task UpdateProductsAsync( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products )
				await this.UpdateProductQuantityAsync( product );
		}

		public void UpdateProductOptions( IEnumerable< BigCommerceProductOption > productOptions )
		{
			foreach( var option in productOptions )
				this.UpdateProductOptionQuantity( option );
		}

		public async Task UpdateProductOptionsAsync( IEnumerable< BigCommerceProductOption > productOptions )
		{
			foreach( var option in productOptions )
				await this.UpdateProductOptionQuantityAsync( option );
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
		}

		private void UpdateProductOptionQuantity( BigCommerceProductOption productOption )
		{
			var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( productOption.ProductId, productOption.Id );
			var jsonContent = new { inventory_level = productOption.Quantity }.ToJson();

			ActionPolicies.Submit.Do( () =>
			{
				this._webRequestServices.PutData( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

				//API requirement
				this.CreateApiDelay().Wait();
			} );
		}

		private async Task UpdateProductOptionQuantityAsync( BigCommerceProductOption productOption )
		{
			var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( productOption.ProductId, productOption.Id );
			var jsonContent = new { inventory_level = productOption.Quantity }.ToJson();

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

				//API requirement
				this.CreateApiDelay().Wait();
			} );
		}
		#endregion

		#region Count
		private int GetProductsCount()
		{
			var count = 0;
			ActionPolicies.Get.Do( () =>
			{
				count = this._webRequestServices.GetResponse< BigCommerceItemsCount >( BigCommerceCommand.GetProductsCount, ParamsBuilder.EmptyParams ).Count;

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
				count = ( await this._webRequestServices.GetResponseAsync< BigCommerceItemsCount >( BigCommerceCommand.GetProductsCount, ParamsBuilder.EmptyParams ) ).Count;

				//API requirement
				this.CreateApiDelay().Wait();
			} );
			return count;
		}
		#endregion
	}
}