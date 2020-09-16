using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Services;
using Netco.ActionPolicyServices;
using Netco.Extensions;
using ServiceStack;

namespace BigCommerceAccess
{
	sealed class BigCommerceProductsServiceV3 : BigCommerceBaseProductsService, IBigCommerceProductsService
	{
		private const string _inventoryTrackingByOption = "variant";

		public BigCommerceProductsServiceV3( WebRequestServices services ) : base( services )
		{
		}

		#region Get
		public List< BigCommerceProduct > GetProducts( bool includeExtendedInfo )
		{
			var mainEndpoint = "?include=variants,images";
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var productsWithinPage = ActionPolicy.Handle< Exception >().Retry( ActionPolicies.RetryCount, ( ex, retryAttempt ) =>
				{
					if ( PageAdjuster.TryAdjustPageIfResponseTooLarge( new PageInfo( i, this.RequestMaxLimit ), this.RequestMinLimit, ex, out var newPageInfo ) )
					{
						i = newPageInfo.Index;
						this.RequestMaxLimit = newPageInfo.Size;
					}

					ActionPolicies.LogRetryAndWait( ex, retryAttempt );
				} ).Get( () => {
					var endpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
					return this._webRequestServices.GetResponse< BigCommerceProductInfoData >( BigCommerceCommand.GetProductsV3, endpoint, marker ); 
				} );
				
				this.CreateApiDelay( productsWithinPage.Limits ).Wait(); //API requirement

				if( productsWithinPage.Response == null )
					break;

				foreach( var product in productsWithinPage.Response.Data )
				{
					var productImage = product.Images.FirstOrDefault( img => img.IsThumbnail );

					products.Add( new BigCommerceProduct
					{
						Id = product.Id,
						InventoryTracking = this.ToCompatibleWithV2InventoryTrackingEnum( product.InventoryTracking ),
						Upc = product.Upc,
						Sku = product.Sku,
						Name = product.Name,
						Description = product.Description,
						Price = product.Price,
						SalePrice = product.SalePrice,
						RetailPrice = product.RetailPrice,
						CostPrice = product.CostPrice,
						Weight = product.Weight,
						BrandId = product.BrandId,
						Quantity = product.Quantity,
						ImageUrls = new BigCommerceProductPrimaryImages()
						{ 
							StandardUrl = productImage != null ? productImage.UrlStandard : string.Empty
						},
						ProductOptions = product.Variants.Select( x => new BigCommerceProductOption
						{
							Id = x.Id,
							ProductId = x.ProductId,
							Sku = x.Sku,
							Quantity = x.Quantity,
							Upc = x.Upc,
							Price = x.Price,
							CostPrice = x.CostPrice,
							Weight = x.Weight,
							ImageFile = x.ImageUrl
						} ).ToList()
					} );
				}

				if( productsWithinPage.Response.Data.Count < RequestMaxLimit )
					break;
			}

			if( includeExtendedInfo )
			{
				base.FillWeightUnit( products, marker );
				base.FillBrands( products, marker );
			}

			return products;
		}

		public async Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendedInfo )
		{
			var mainEndpoint = "?include=variants,images";
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var productsWithinPage = await ActionPolicyAsync.Handle< Exception >().RetryAsync( ActionPolicies.RetryCount, ( ex, retryAttempt ) =>
				{
					if ( PageAdjuster.TryAdjustPageIfResponseTooLarge( new PageInfo( i, this.RequestMaxLimit ), this.RequestMinLimit, ex, out var newPageInfo ) )
					{
						i = newPageInfo.Index;
						this.RequestMaxLimit = newPageInfo.Size;
					}

					return ActionPolicies.LogRetryAndWaitAsync( ex, retryAttempt );
				} ).Get( () => {
					var endpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
					return this._webRequestServices.GetResponseAsync< BigCommerceProductInfoData >( BigCommerceCommand.GetProductsV3, endpoint, marker ); 
				} );
				
				await this.CreateApiDelay( productsWithinPage.Limits, token ); //API requirement

				if( productsWithinPage.Response == null )
					break;

				foreach( var product in productsWithinPage.Response.Data )
				{
					var productImage = product.Images.FirstOrDefault( img => img.IsThumbnail );

					products.Add( new BigCommerceProduct
					{
						Id = product.Id,
						Sku = product.Sku,
						InventoryTracking = this.ToCompatibleWithV2InventoryTrackingEnum( product.InventoryTracking ),
						Upc = product.Upc,
						Name = product.Name,
						Description = product.Description,
						Price = product.Price,
						SalePrice = product.SalePrice,
						RetailPrice = product.RetailPrice,
						CostPrice = product.CostPrice,
						Weight = product.Weight,
						BrandId = product.BrandId,
						Quantity = product.Quantity,
						ImageUrls = new BigCommerceProductPrimaryImages()
						{ 
							StandardUrl = productImage != null ? productImage.UrlStandard : string.Empty
						},
						ProductOptions = product.Variants.Select( x => new BigCommerceProductOption
						{
							Id = x.Id,
							ProductId = x.ProductId,
							Sku = x.Sku,
							Quantity = x.Quantity,
							Upc = x.Upc,
							Price = x.Price,
							CostPrice = x.CostPrice,
							Weight = x.Weight,
							ImageFile = x.ImageUrl
						} ).ToList()
					} );
				}

				if( productsWithinPage.Response.Data.Count < RequestMaxLimit )
					break;
			}

			if( includeExtendedInfo )
			{
				await base.FillWeightUnitAsync( products, token, marker );
				await base.FillBrandsAsync( products, token, marker );
			}

			return products;
		}
		#endregion

		#region Update
		public void UpdateProductOptions( List< BigCommerceProductOption > productOptions )
		{
			var marker = this.GetMarker();

			foreach( var productOption in productOptions )
			{
				var endpoint = string.Format("/{0}/variants/{1}", productOption.ProductId, productOption.Id );
				var jsonContent = new { inventory_level = productOption.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProductsV3, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		public async Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			var marker = this.GetMarker();

			await productOptions.DoInBatchAsync( MaxThreadsCount, async productOption =>
			{
				var endpoint = string.Format("/{0}/variants/{1}", productOption.ProductId, productOption.Id );
				var jsonContent = new { inventory_level = productOption.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductsV3, endpoint, jsonContent, marker ) );
				await this.CreateApiDelay( limit, token ); //API requirement
			} );
		}

		public void UpdateProducts( List< BigCommerceProduct > products )
		{
			var marker = this.GetMarker();

			foreach( var product in products )
			{
				var endpoint = string.Format("/{0}", product.Id);
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = ActionPolicies.Submit.Get( () =>
					this._webRequestServices.PutData( BigCommerceCommand.UpdateProductsV3, endpoint, jsonContent, marker ) );
				this.CreateApiDelay( limit ).Wait(); //API requirement
			}
		}

		public async Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token )
		{
			var marker = this.GetMarker();

			await products.DoInBatchAsync( MaxThreadsCount, async product =>
			{
				var endpoint = string.Format("/{0}", product.Id);
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductsV3, endpoint, jsonContent, marker ) );

				await this.CreateApiDelay( limit, token ); //API requirement
			} );
		}

		#endregion

		#region Misc
		private InventoryTrackingEnum ToCompatibleWithV2InventoryTrackingEnum( string inventoryTracking )
		{
			if ( string.IsNullOrWhiteSpace( inventoryTracking ) )
			{
				return InventoryTrackingEnum.none;
			}

			if ( inventoryTracking.Equals( _inventoryTrackingByOption ) )
			{
				return InventoryTrackingEnum.sku;
			}

			return InventoryTrackingEnum.simple;
		}
		#endregion
	}
}