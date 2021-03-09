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

		public string GetStoreName()
		{
			var marker = this.GetMarker();
			return base.GetStoreName(marker);

		}

		public string GetStoreDomain()
		{
			var marker = this.GetMarker();
			return base.GetDomain(marker);

		}

		public string GetStoreSafeURL()
		{
			var marker = this.GetMarker();
			return base.GetSecureURL(marker);

		}
		public List<BigCommerceProduct> GetProducts(bool includeExtendedInfo)
		{
			var mainEndpoint = "?include=variants,images";
			var products = new List<BigCommerceProduct>();
			var marker = this.GetMarker();

			for (var i = 1; i < int.MaxValue; i++)
			{
				var endpoint = mainEndpoint.ConcatParams(ParamsBuilder.CreateGetNextPageParams(new BigCommerceCommandConfig(i, RequestMaxLimit)));
				var productsWithinPage = ActionPolicy.Handle<Exception>().Retry(ActionPolicies.RetryCount, (ex, retryAttempt) =>
				{
					if (PageAdjuster.TryAdjustPageIfResponseTooLarge(new PageInfo(i, this.RequestMaxLimit), this.RequestMinLimit, ex, out var newPageInfo))
					{
						i = newPageInfo.Index;
						this.RequestMaxLimit = newPageInfo.Size;
					}

					ActionPolicies.LogRetryAndWait(ex, marker, endpoint, retryAttempt);
				}).Get(() => {
					return this._webRequestServices.GetResponseByRelativeUrl<BigCommerceProductInfoData>(BigCommerceCommand.GetProductsV3, endpoint, marker);
				});

				this.CreateApiDelay(productsWithinPage.Limits).Wait(); //API requirement

				if (productsWithinPage.Response == null)
					break;

				foreach (var product in productsWithinPage.Response.Data)
				{
					var productImageThumbnail = product.Images.FirstOrDefault(img => img.IsThumbnail);

					var additional_images = product.Images;

					var custom_url = product.Product_URL;

					products.Add(new BigCommerceProduct
					{
						Id = product.Id,
						InventoryTracking = this.ToCompatibleWithV2InventoryTrackingEnum(product.InventoryTracking),
						Upc = product.Upc,
						Sku = product.Sku,
						Name = product.Name,
						Description = product.Description,
						Price = product.Price,
						IsVisible = product.IsVisible,
						Type = product.Type,
						SalePrice = product.SalePrice,
						RetailPrice = product.RetailPrice,
						CostPrice = product.CostPrice,
						Weight = product.Weight,
						BrandId = product.BrandId,
						Quantity = product.Quantity,
						Product_URL = custom_url.ProductURL,
						Categories = product.Categories,
						ThumbnailImageURL = new BigCommerceProductPrimaryImages()
						{
							StandardUrl = productImageThumbnail != null ? productImageThumbnail.UrlStandard : string.Empty
						},
						ProductOptions = product.Variants.Select(x => new BigCommerceProductOption
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
						}).ToList(),
						Main_Images = product.Images.Select(y => new BigCommerceImage
						{
							UrlStandard = y.UrlStandard,
							IsThumbnail = y.IsThumbnail
						}).ToList()
					}) ;
				}

				if (productsWithinPage.Response.Data.Count < RequestMaxLimit)
					break;
			}

			if (includeExtendedInfo)
			{
				base.FillWeightUnit(products, marker);
				base.FillBrands(products, marker);
			}

			return products;
		}

		public async Task<List<BigCommerceProduct>> GetProductsAsync(CancellationToken token, bool includeExtendedInfo)
		{
			var mainEndpoint = "?include=variants,images";
			var products = new List<BigCommerceProduct>();
			var marker = this.GetMarker();

			for (var i = 1; i < int.MaxValue; i++)
			{
				var endpoint = mainEndpoint.ConcatParams(ParamsBuilder.CreateGetNextPageParams(new BigCommerceCommandConfig(i, RequestMaxLimit)));
				var productsWithinPage = await ActionPolicyAsync.Handle<Exception>().RetryAsync(ActionPolicies.RetryCount, (ex, retryAttempt) =>
				{
					if (PageAdjuster.TryAdjustPageIfResponseTooLarge(new PageInfo(i, this.RequestMaxLimit), this.RequestMinLimit, ex, out var newPageInfo))
					{
						i = newPageInfo.Index;
						this.RequestMaxLimit = newPageInfo.Size;
					}

					return ActionPolicies.LogRetryAndWaitAsync(ex, marker, endpoint, retryAttempt);
				}).Get(() => {
					return this._webRequestServices.GetResponseByRelativeUrlAsync<BigCommerceProductInfoData>(BigCommerceCommand.GetProductsV3, endpoint, marker);
				});

				await this.CreateApiDelay(productsWithinPage.Limits, token); //API requirement

				if (productsWithinPage.Response == null)
					break;

				foreach (var product in productsWithinPage.Response.Data)
				{
					var productImageThumbnail = product.Images.FirstOrDefault(img => img.IsThumbnail);

					var additional_images = product.Images;

					products.Add(new BigCommerceProduct
					{
						Id = product.Id,
						Sku = product.Sku,
						InventoryTracking = this.ToCompatibleWithV2InventoryTrackingEnum(product.InventoryTracking),
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
						ThumbnailImageURL = new BigCommerceProductPrimaryImages()
						{
							StandardUrl = productImageThumbnail != null ? productImageThumbnail.UrlStandard : string.Empty
						},
						ProductOptions = product.Variants.Select(x => new BigCommerceProductOption
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
						}).ToList(),
						Main_Images = product.Images.Select(y => new BigCommerceImage
						{
							UrlStandard = y.UrlStandard,
							IsThumbnail = y.IsThumbnail
						}).ToList()
					});
				}

				if (productsWithinPage.Response.Data.Count < RequestMaxLimit)
					break;
			}

			if (includeExtendedInfo)
			{
				await base.FillWeightUnitAsync(products, token, marker);
				await base.FillBrandsAsync(products, token, marker);
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

				var limit = ActionPolicies.Submit( marker, endpoint ).Get( () =>
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

				var limit = await ActionPolicies.SubmitAsync( marker, endpoint ).Get( async () =>
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

				var limit = ActionPolicies.Submit( marker, endpoint ).Get( () =>
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

				var limit = await ActionPolicies.SubmitAsync( marker, endpoint ).Get( async () =>
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