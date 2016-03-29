using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		public List< BigCommerceProduct > GetProducts()
		{
			var products = new List< BigCommerceProduct >();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint ) );
				this.CreateApiDelay().Wait(); //API requirement

				if( productsWithinPage == null )
					break;

				this.FillProductsSkus( productsWithinPage );
				products.AddRange( productsWithinPage );
				if( productsWithinPage.Count < RequestMaxLimit )
					break;
			}

			return products;
		}

		public async Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token )
		{
			var products = new List< BigCommerceProduct >();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint ) );
				await this.CreateApiDelay( token ); //API requirement

				if( productsWithinPage == null )
					break;

				await this.FillProductsSkusAsync( productsWithinPage, token );
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
						this._webRequestServices.GetResponse< List< BigCommerceProductOption > >( product.ProductOptionsReference.Url, endpoint ) );
					this.CreateApiDelay().Wait(); //API requirement

					if( options == null )
						break;
					product.ProductOptions.AddRange( options );
					if( options.Count < RequestMaxLimit )
						break;
				}
			}
		}

		private async Task FillProductsSkusAsync( IEnumerable< BigCommerceProduct > products, CancellationToken token )
		{
			foreach( var product in products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) ) )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var options = await ActionPolicies.GetAsync.Get( async () =>
						await this._webRequestServices.GetResponseAsync< List< BigCommerceProductOption > >( product.ProductOptionsReference.Url, endpoint ) );
					await this.CreateApiDelay( token ); //API requirement

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
		public void UpdateProducts( List< BigCommerceProduct > products )
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

		public async Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token )
		{
			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				await ActionPolicies.SubmitAsync.Do( async () =>
				{
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

					//API requirement
					await this.CreateApiDelay( token );
				} );
			}
		}

		public void UpdateProductOptions( List< BigCommerceProductOption > productOptions )
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

		public async Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				await ActionPolicies.SubmitAsync.Do( async () =>
				{
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent );

					//API requirement
					await this.CreateApiDelay( token );
				} );
			}
		}
		#endregion
	}
}