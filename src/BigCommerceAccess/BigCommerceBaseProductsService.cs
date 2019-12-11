using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Services;
using CuttingEdge.Conditions;
using Netco.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BigCommerceAccess
{
	abstract class BigCommerceBaseProductsService : BigCommerceServiceBase
	{
		protected readonly WebRequestServices _webRequestServices;

		public BigCommerceBaseProductsService( WebRequestServices services )
		{
			Condition.Requires( services, "services" ).IsNotNull();

			this._webRequestServices = services;
		}

		protected virtual void FillWeightUnit( IEnumerable< BigCommerceProduct > products, string marker )
		{
			var store = ActionPolicies.Get.Get( () =>
				this._webRequestServices.GetResponse< BigCommerceStore >( BigCommerceCommand.GetStoreV2_OAuth, string.Empty, marker ) );
			this.CreateApiDelay( store.Limits ).Wait(); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		protected virtual async Task FillWeightUnitAsync( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
		{
			var store = await ActionPolicies.GetAsync.Get( async () =>
				await this._webRequestServices.GetResponseAsync< BigCommerceStore >( BigCommerceCommand.GetStoreV2_OAuth, string.Empty, marker ) );
			await this.CreateApiDelay( store.Limits, token ); //API requirement

			foreach( var product in products )
			{
				product.WeightUnit = store.Response.WeightUnits;
			}
		}

		protected virtual void FillBrands( IEnumerable< BigCommerceProduct > products, string marker )
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

		protected virtual async Task FillBrandsAsync( IEnumerable< BigCommerceProduct > products, CancellationToken token, string marker )
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

		protected void FillProductsSkus( IEnumerable< BigCommerceProduct > products, string marker )
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

		protected async Task FillProductsSkusAsync( IEnumerable< BigCommerceProduct > products, bool isUnlimit, CancellationToken token, string marker )
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

		protected void FillBrandsForProducts( IEnumerable< BigCommerceProduct > products, List< BigCommerceBrand > brands )
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
	}
}
