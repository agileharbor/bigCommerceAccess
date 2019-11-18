﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	sealed class BigCommerceProductsServiceV3 : BigCommerceBaseProductsService, IBigCommerceProductsService
	{
		private readonly BigCommerceProductsServiceV2OAuth _productsServiceV2OAuth;

		public BigCommerceProductsServiceV3( WebRequestServices services, BigCommerceProductsServiceV2OAuth productsServiceV2OAuth ) : base( services )
		{
			Condition.Requires( productsServiceV2OAuth, "_productsServiceV2OAuth" ).IsNotNull();

			this._productsServiceV2OAuth = productsServiceV2OAuth;
		}

		#region Get
		public List< BigCommerceProduct > GetProducts( bool includeExtendedInfo )
		{
			var mainEndpoint = "?include=variants";
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var productsWithinPage = ActionPolicies.Get.Get( () =>
					base._webRequestServices.GetResponse< BigCommerceProductInfoData >( BigCommerceCommand.GetProductsV3, endpoint, marker ) );
				this.CreateApiDelay( productsWithinPage.Limits ).Wait(); //API requirement

				if( productsWithinPage.Response == null )
					break;

				foreach( var product in productsWithinPage.Response.Data )
					products.Add( new BigCommerceProduct
					{
						Id = product.Id,
						InventoryTracking = product.InventoryTracking.ToEnum( InventoryTrackingEnum.none ),
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
						ImageUrls = new BigCommerceProductPrimaryImages { StandardUrl = product.Images.FirstOrDefault() != null ? product.Images.FirstOrDefault().UrlStandard : string.Empty },
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

				if( productsWithinPage.Response.Data.Count < RequestMaxLimit )
					break;
			}

			if( includeExtendedInfo )
			{
				this._productsServiceV2OAuth.FillWeightUnit( products, marker );
				this._productsServiceV2OAuth.FillBrands( products, marker );
			}

			return products;
		}

		public async Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendedInfo )
		{
			var mainEndpoint = "?include=variants";
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = mainEndpoint.ConcatParams( ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) ) );
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await this._webRequestServices.GetResponseAsync< BigCommerceProductInfoData >( BigCommerceCommand.GetProductsV3, endpoint, marker ) );
				await this.CreateApiDelay( productsWithinPage.Limits, token ); //API requirement

				if( productsWithinPage.Response == null )
					break;

				foreach( var product in productsWithinPage.Response.Data )
					products.Add( new BigCommerceProduct
					{
						Id = product.Id,
						InventoryTracking = product.InventoryTracking.ToEnum( InventoryTrackingEnum.none ),
						Upc = product.Upc,
						Name = product.Name,
						Description = product.Description,
						Price = product.Price,
						SalePrice = product.SalePrice,
						RetailPrice = product.RetailPrice,
						CostPrice = product.CostPrice,
						Weight = product.Weight,
						BrandId = product.BrandId,
						ImageUrls = new BigCommerceProductPrimaryImages { StandardUrl = product.Images.FirstOrDefault() != null ? product.Images.FirstOrDefault().UrlStandard : string.Empty },
						ProductOptions = product.Variants.Select( x => new BigCommerceProductOption
						{
							ProductId = x.ProductId,
							Upc = x.Upc,
							Price = x.Price,
							CostPrice = x.CostPrice,
							Weight = x.Weight,
							ImageFile = x.ImageUrl
						} ).ToList()
					} );

				if( productsWithinPage.Response.Data.Count < RequestMaxLimit )
					break;
			}

			if( includeExtendedInfo )
			{
				await this._productsServiceV2OAuth.FillWeightUnitAsync( products, token, marker );
				await this._productsServiceV2OAuth.FillBrandsAsync( products, token, marker );
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

			await productOptions.DoInBatchAsync( 20, async productOption =>
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

			await products.DoInBatchAsync( 20, async product =>
			{
				var endpoint = string.Format("/{0}", product.Id);
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductsV3, endpoint, jsonContent, marker ) );

				await this.CreateApiDelay( limit, token ); //API requirement
			} );
		}

		#endregion
	}
}