using System.Collections.Generic;
using System.Linq;
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
	public class BigCommerceProductsService: BigCommerceServiceBase, IBigCommerceProductsService
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
			var products = this.CollectProductsFromAllPages();
			this.FillProductsSkus( products );

			return products;
		}

		public async Task< IEnumerable< BigCommerceProduct > > GetProductsAsync()
		{
			var products = await this.CollectProductsFromAllPagesAsync();
			await this.FillProductsSkusAsync( products );

			return products;
		}

		private IList< BigCommerceProduct > CollectProductsFromAllPages()
		{
			var products = new List< BigCommerceProduct >();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< IList< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint ) );
				this.CreateApiDelay().Wait(); //API requirement

				if( productsWithinPage == null )
					break;
				products.AddRange( productsWithinPage );
				if( productsWithinPage.Count < RequestMaxLimit )
					break;
			}

			return products;
		}

		private async Task< IList< BigCommerceProduct > > CollectProductsFromAllPagesAsync()
		{
			var products = new List< BigCommerceProduct >();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< IList< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint ) );
				this.CreateApiDelay().Wait(); //API requirement

				if( productsWithinPage == null )
					break;
				products.AddRange( productsWithinPage );
				if( productsWithinPage.Count < RequestMaxLimit )
					break;
			}

			return products;
		}

		private void FillProductsSkus( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) ) )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var options = ActionPolicies.Get.Get( () =>
						this._webRequestServices.GetResponse< IList< BigCommerceProductOption > >( product.ProductOptionsReference.Url, endpoint ) );
					this.CreateApiDelay().Wait(); //API requirement

					if( options == null )
						break;
					product.ProductOptions.AddRange( options );
					if( options.Count < RequestMaxLimit )
						break;
				}
			}
		}

		private async Task FillProductsSkusAsync( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) ) )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var options = await ActionPolicies.GetAsync.Get( async () =>
						await this._webRequestServices.GetResponseAsync< IList< BigCommerceProductOption > >( product.ProductOptionsReference.Url, endpoint ) );
					this.CreateApiDelay().Wait(); //API requirement

					if( options == null )
						break;
					product.ProductOptions.AddRange( options );
					if( options.Count < RequestMaxLimit )
						break;
				}
			}
		}
		#endregion

		#region Update
		public void UpdateProducts( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products )
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
		}

		public async Task UpdateProductsAsync( IEnumerable< BigCommerceProduct > products )
		{
			foreach( var product in products )
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
		}

		public void UpdateProductOptions( IEnumerable< BigCommerceProductOption > productOptions )
		{
			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				ActionPolicies.Submit.Do( () =>
				{
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}
		}

		public async Task UpdateProductOptionsAsync( IEnumerable< BigCommerceProductOption > productOptions )
		{
			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				await ActionPolicies.SubmitAsync.Do( async () =>
				{
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

					//API requirement
					this.CreateApiDelay().Wait();
				} );
			}
		}
		#endregion
	}
}