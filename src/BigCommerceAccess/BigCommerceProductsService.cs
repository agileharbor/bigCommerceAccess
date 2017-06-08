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
using Netco.Extensions;
using ServiceStack;

namespace BigCommerceAccess
{
	public class BigCommerceProductsService: BigCommerceServiceBase, IBigCommerceProductsService
	{
		private readonly WebRequestServices _webRequestServices;

		public BigCommerceProductsService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this._webRequestServices = new WebRequestServices( config, this.GetMarker() );
		}

		#region Get
		public List< BigCommerceProduct > GetProducts( bool includeExtendInfo = false )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint, marker ) );
				this.CreateApiDelay( productsWithinPage.Limits ).Wait(); //API requirement

				if( productsWithinPage.Response == null )
					break;

				this.FillProductsSkus( productsWithinPage.Response, marker );
				products.AddRange( productsWithinPage.Response );
				if( productsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			if( includeExtendInfo )
			{
				this.FillWeightUnit( products, marker );
				this.FillBrands( products, marker );
			}

			return products;
		}

		public async Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendInfo = false )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceProduct > >( BigCommerceCommand.GetProducts, endpoint, marker ) );
				await this.CreateApiDelay( productsWithinPage.Limits, token ); //API requirement

				if( productsWithinPage.Response == null )
					break;

				await this.FillProductsSkusAsync( productsWithinPage.Response, productsWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				products.AddRange( productsWithinPage.Response );
				if( productsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			if( includeExtendInfo )
			{
				await this.FillWeightUnitAsync( products, token, marker );
				await this.FillBrandsAsync( products, token, marker );
			}

			return products;
		}

		private void FillProductsSkus( IEnumerable< BigCommerceProduct > products, string marker )
		{
			foreach( var product in products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) ) )
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var options = ActionPolicies.Get.Get( () =>
						this._webRequestServices.GetResponse< List< BigCommerceProductOption > >( product.ProductOptionsReference.Url, endpoint, marker ) );
					this.CreateApiDelay( options.Limits ).Wait(); //API requirement

					if( options.Response == null )
						break;
					product.ProductOptions.AddRange( options.Response );
					if( options.Response.Count < RequestMaxLimit )
						break;
				}
			}
		}

		private async Task FillProductsSkusAsync( IEnumerable< BigCommerceProduct > products, bool isUnlimit, CancellationToken token, string marker )
		{
			var threadCount = isUnlimit ? MaxThreadsCount : 1;
			var skuProducts = products.Where( product => product.InventoryTracking.Equals( InventoryTrackingEnum.sku ) );
			await skuProducts.DoInBatchAsync( threadCount, async product =>
			{
				for( var i = 1; i < int.MaxValue; i++ )
				{
					var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
					var options = await ActionPolicies.GetAsync.Get( async () =>
						await this._webRequestServices.GetResponseAsync< List< BigCommerceProductOption > >( product.ProductOptionsReference.Url, endpoint, marker ) );
					await this.CreateApiDelay( options.Limits, token ); //API requirement

					if( options.Response == null )
						break;
					product.ProductOptions.AddRange( options.Response );
					if( options.Response.Count < RequestMaxLimit )
						break;
				}
			} );
		}

		private void FillWeightUnit( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var store = ActionPolicies.Get.Get( () =>
				this._webRequestServices.GetResponse< BigCommerceStore >( BigCommerceCommand.GetStore, string.Empty, marker ) );
			this.CreateApiDelay( store.Limits ).Wait(); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		private async Task FillWeightUnitAsync( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var store = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< BigCommerceStore >( BigCommerceCommand.GetStore, string.Empty, marker ) );
			await this.CreateApiDelay( store.Limits, token ); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		private void FillBrands( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var brands = new List< BigCommerceBrand >();
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var brandsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceBrand > >( BigCommerceCommand.GetBrands, endpoint, marker ) );
				this.CreateApiDelay( brandsWithinPage.Limits ).Wait(); //API requirement

				if( brandsWithinPage.Response == null )
					break;

				brands.AddRange( brandsWithinPage.Response );
				if( brandsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			this.FillBrandsForProducts( products, brands );
		}

		private async Task FillBrandsAsync( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var brands = new List< BigCommerceBrand >();
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var brandsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceBrand > >( BigCommerceCommand.GetBrands, endpoint, marker ) );
				await this.CreateApiDelay( brandsWithinPage.Limits, token ); //API requirement

				if( brandsWithinPage.Response == null )
					break;

				brands.AddRange( brandsWithinPage.Response );
				if( brandsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			this.FillBrandsForProducts( products, brands );
		}

		private void FillBrandsForProducts( IEnumerable< BigCommerceProduct > products, List< BigCommerceBrand > brands )
		{
			foreach( var product in products )
			{
				if( !product.BrandId.HasValue )
				{
					product.BrandName = null;
					continue;
				}

				var brand = brands.FirstOrDefault( x => x.Id == product.BrandId.Value );
				if( brand == null )
				{
					product.BrandName = null;
					continue;
				}

				product.BrandName = brand.Name;
			}
		}
		
		#endregion

		#region Update
		public void UpdateProducts( List< BigCommerceProduct > products )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProduct, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		public async Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent, marker ) );

				await this.CreateApiDelay( limit, token ); //API requirement
			}
		}

		public void UpdateProductOptions( List< BigCommerceProductOption > productOptions )
		{
			var marker = this.GetMarker();

			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProduct, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		public async Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			var marker = this.GetMarker();

			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProduct, endpoint, jsonContent, marker ) );
				await this.CreateApiDelay( limit, token ); //API requirement
			}
		}
		#endregion
	}
}