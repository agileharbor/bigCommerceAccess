using BigCommerceAccess.Misc;
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
