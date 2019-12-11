using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Services;
using Netco.Extensions;
using ServiceStack;

namespace BigCommerceAccess
{
	sealed class BigCommerceProductsServiceV2OAuth : BigCommerceBaseProductsService, IBigCommerceProductsService
	{
		public BigCommerceProductsServiceV2OAuth( WebRequestServices services ) : base( services )
		{ }

		#region Get
		public List< BigCommerceProduct > GetProducts( bool includeExtendInfo )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				endpoint += includeExtendInfo ? ParamsBuilder.GetFieldsForProductSync() : ParamsBuilder.GetFieldsForInventorySync();
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
				base.FillWeightUnit( products, marker );
				base.FillBrands( products, marker );
			}

			return products;
		}

		public async Task< List< BigCommerceProduct > > GetProductsAsync( CancellationToken token, bool includeExtendedInfo )
		{
			var products = new List< BigCommerceProduct >();
			var marker = this.GetMarker();

			for( var i = 1; i < int.MaxValue; i++ )
			{
				var endpoint = ParamsBuilder.CreateGetNextPageParams( new BigCommerceCommandConfig( i, RequestMaxLimit ) );
				endpoint += includeExtendedInfo ? ParamsBuilder.GetFieldsForProductSync() : ParamsBuilder.GetFieldsForInventorySync();
				var productsWithinPage = await ActionPolicies.GetAsync.Get( async () =>
					await base._webRequestServices.GetResponseAsync< List< BigCommerceProduct > >( BigCommerceCommand.GetProductsV2_OAuth, endpoint, marker ) );
				await this.CreateApiDelay( productsWithinPage.Limits, token ); //API requirement

				if( productsWithinPage.Response == null )
					break;

				await this.FillProductsSkusAsync( productsWithinPage.Response, productsWithinPage.Limits.IsUnlimitedCallsCount, token, marker );
				products.AddRange( productsWithinPage.Response );
				if( productsWithinPage.Response.Count < RequestMaxLimit )
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
		public void UpdateProducts( List< BigCommerceProduct > products )
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

		public async Task UpdateProductsAsync( List< BigCommerceProduct > products, CancellationToken token )
		{
			var marker = this.GetMarker();

			await products.DoInBatchAsync( 20, async product =>
			{
				var endpoint = ParamsBuilder.CreateProductUpdateEndpoint( product.Id );
				var jsonContent = new { inventory_level = product.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductV2_OAuth, endpoint, jsonContent, marker ) );

				await this.CreateApiDelay( limit, token ); //API requirement
			} );
		}

		public void UpdateProductOptions(List<BigCommerceProductOption> productOptions)
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

		public async Task UpdateProductOptionsAsync( List< BigCommerceProductOption > productOptions, CancellationToken token )
		{
			var marker = this.GetMarker();

			await productOptions.DoInBatchAsync( 20, async option =>
			{
				var endpoint = ParamsBuilder.CreateProductOptionUpdateEndpoint( option.ProductId, option.Id );
				var jsonContent = new { inventory_level = option.Quantity }.ToJson();

				var limit = await ActionPolicies.SubmitAsync.Get( async () =>
					await this._webRequestServices.PutDataAsync( BigCommerceCommand.UpdateProductV2_OAuth, endpoint, jsonContent, marker ) );
				await this.CreateApiDelay( limit, token ); //API requirement
			} );
		}
		#endregion
	}
}
