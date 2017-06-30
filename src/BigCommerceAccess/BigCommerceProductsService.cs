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
		private readonly APIVersion _apiVersion;

		public BigCommerceProductsService( BigCommerceConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this._webRequestServices = new WebRequestServices( config, this.GetMarker() );
			this._apiVersion = config.GetAPIVersion();
		}

		#region Get
		public List< BigCommerceProduct > GetProducts( bool includeExtendInfo = false )
		{
			return this._apiVersion == APIVersion.V2 ? this.GetProductsV2( includeExtendInfo ) : this.GetProductsV3( includeExtendInfo );
		}

		public Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendInfo = false )
		{
			return this._apiVersion == APIVersion.V2 ? this.GetProductsV2Async( token, includeExtendInfo ) : this.GetProductsV3Async( token, includeExtendInfo );
		}

		private List< BigCommerceProduct > GetProductsV2( bool includeExtendInfo = false )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceProduct > >( BigCommerceCommand.GetProductsV2, endpoint, marker ) );
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
				this.FillWeightUnitV2( products, marker );
				this.FillBrandsV2( products, marker );
			}

			return products;
		}

		private async Task< List< BigCommerceProduct > > GetProductsV2Async( CancellationToken token, bool includeExtendInfo = false )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceProduct > >( BigCommerceCommand.GetProductsV2, endpoint, marker ) );
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
				await this.FillWeightUnitV2Async( products, token, marker );
				await this.FillBrandsV2Async( products, token, marker );
			}

			return products;
		}

		private List< BigCommerceProduct > GetProductsV3( bool includeExtendInfo = false )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceProduct > >( BigCommerceCommand.GetProductsV2_OAuth, endpoint, marker ) );
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
				this.FillWeightUnitV3( products, marker );
				this.FillBrandsV3( products, marker );
			}

			return products;
		}
		
		private async Task< List< BigCommerceProduct > > GetProductsV3Async( CancellationToken token, bool includeExtendInfo = false )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceProduct > >( BigCommerceCommand.GetProductsV2_OAuth, endpoint, marker ) );
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
				await this.FillWeightUnitV3Async( products, token, marker );
				await this.FillBrandsV3Async( products, token, marker );
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

		private List< BigCommerceProductInfo > GetProductsInfo()
		{
			var mainEndpoint = "?include=variants";
			var products = new List< BigCommerceProductInfo >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< BigCommerceProductInfoData >( BigCommerceCommand.GetProductsInfoV3, endpoint, marker ) );
				this.CreateApiDelay( productsWithinPage.Limits ).Wait(); //API requirement

				if( productsWithinPage.Response == null )
					break;

				products.AddRange( productsWithinPage.Response.Data );
				if( productsWithinPage.Response.Data.Count < RequestMaxLimit )
					break;
			}

			return products;
		}
		
		private async Task< List< BigCommerceProductInfo > > GetProductsInfoAsync( CancellationToken token )
		{
			var mainEndpoint = "?include=variants";
			var products = new List< BigCommerceProductInfo >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< BigCommerceProductInfoData >( BigCommerceCommand.GetProductsInfoV3, endpoint, marker ) );
				await this.CreateApiDelay( productsWithinPage.Limits, token ); //API requirement

				if( productsWithinPage.Response == null )
					break;

				products.AddRange( productsWithinPage.Response.Data );
				if( productsWithinPage.Response.Data.Count < RequestMaxLimit )
					break;
			}

			return products;
		}

		private void FillWeightUnitV2( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var store = ActionPolicies.Get.Get( () =>
				this._webRequestServices.GetResponse< BigCommerceStore >( BigCommerceCommand.GetStoreV2, string.Empty, marker ) );
			this.CreateApiDelay( store.Limits ).Wait(); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		private async Task FillWeightUnitV2Async( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var store = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< BigCommerceStore >( BigCommerceCommand.GetStoreV2, string.Empty, marker ) );
			await this.CreateApiDelay( store.Limits, token ); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		private void FillWeightUnitV3( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var store = ActionPolicies.Get.Get( () =>
				this._webRequestServices.GetResponse< BigCommerceStore >( BigCommerceCommand.GetStoreV2_OAuth, string.Empty, marker ) );
			this.CreateApiDelay( store.Limits ).Wait(); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		private async Task FillWeightUnitV3Async( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var store = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< BigCommerceStore >( BigCommerceCommand.GetStoreV2_OAuth, string.Empty, marker ) );
			await this.CreateApiDelay( store.Limits, token ); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		private void FillBrandsV2( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var brands = new List< BigCommerceBrand >();
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var brandsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceBrand > >( BigCommerceCommand.GetBrandsV2, endpoint, marker ) );
				this.CreateApiDelay( brandsWithinPage.Limits ).Wait(); //API requirement

				if( brandsWithinPage.Response == null )
					break;

				brands.AddRange( brandsWithinPage.Response );
				if( brandsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			this.FillBrandsForProducts( products, brands );
		}

		private async Task FillBrandsV2Async( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var brands = new List< BigCommerceBrand >();
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var brandsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceBrand > >( BigCommerceCommand.GetBrandsV2, endpoint, marker ) );
				await this.CreateApiDelay( brandsWithinPage.Limits, token ); //API requirement

				if( brandsWithinPage.Response == null )
					break;

				brands.AddRange( brandsWithinPage.Response );
				if( brandsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			this.FillBrandsForProducts( products, brands );
		}

		private void FillBrandsV3( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var brands = new List< BigCommerceBrand >();
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var brandsWithinPage = ActionPolicies.Get.Get( () =>
					this._webRequestServices.GetResponse< List< BigCommerceBrand > >( BigCommerceCommand.GetBrandsV2_OAuth, endpoint, marker ) );
				this.CreateApiDelay( brandsWithinPage.Limits ).Wait(); //API requirement

				if( brandsWithinPage.Response == null )
					break;

				brands.AddRange( brandsWithinPage.Response );
				if( brandsWithinPage.Response.Count < RequestMaxLimit )
					break;
			}

			this.FillBrandsForProducts( products, brands );
		}

		private async Task FillBrandsV3Async( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var brands = new List< BigCommerceBrand >();
			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				var brandsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< List< BigCommerceBrand > >( BigCommerceCommand.GetBrandsV2_OAuth, endpoint, marker ) );
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
			if( this._apiVersion == APIVersion.V2 )
				this.UpdateProductsV2( products );
			else
				this.UpdateProductsV3( products );
		}

		public Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token )
		{
			return this._apiVersion == APIVersion.V2 ? this.UpdateProductsV2Async( products, token ) : this.UpdateProductsV3Async( products, token );
		}

		private void UpdateProductsV2( List< BigCommerceProduct > products )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProductV2, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		private void UpdateProductsV3( List< BigCommerceProduct > products )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProductV2_OAuth, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		private async Task UpdateProductsV2Async( List< BigCommerceProduct > products, CancellationToken token )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductV2, endpoint, jsonContent, marker ) );

				await this.CreateApiDelay( limit, token ); //API requirement
			}
		}

		private async Task UpdateProductsV3Async( List< BigCommerceProduct > products, CancellationToken token )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductV2_OAuth, endpoint, jsonContent, marker ) );

				await this.CreateApiDelay( limit, token ); //API requirement
			}
		}

		public void UpdateProductOptions( List< BigCommerceProductOption > productOptions )
		{
			if( this._apiVersion == APIVersion.V2 )
				this.UpdateProductOptionsV2( productOptions );
			else
				this.UpdateProductOptionsV3( productOptions );
		}

		public Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			return this._apiVersion == APIVersion.V2 ? this.UpdateProductOptionsV2Async( productOptions, token ) : this.UpdateProductOptionsV3Async( productOptions, token );
		}

		private void UpdateProductOptionsV2( List< BigCommerceProductOption > productOptions )
		{
			var marker = this.GetMarker();

			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProductV2, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		public void UpdateProductOptionsV3( List< BigCommerceProductOption > productOptions )
		{
			var marker = this.GetMarker();

			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProductV2_OAuth, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		private async Task UpdateProductOptionsV2Async( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			var marker = this.GetMarker();

			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductV2, endpoint, jsonContent, marker ) );
				await this.CreateApiDelay( limit, token ); //API requirement
			}
		}

		public async Task UpdateProductOptionsV3Async( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			var marker = this.GetMarker();

			foreach( var option in productOptions )
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductV2_OAuth, endpoint, jsonContent, marker ) );
				await this.CreateApiDelay( limit, token ); //API requirement
			}
		}
		#endregion
	}
}