using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Command;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Services;
using CuttingEdge.Conditions;
using Netco.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BigCommerceAccess
{
	abstract class BigCommerceBaseCategoriesService : BigCommerceServiceBase
	{
		protected readonly WebRequestServices _webRequestServices;

		public BigCommerceBaseCategoriesService( WebRequestServices services )
		{
			Condition.Requires( services, "services" ).IsNotNull();

			this._webRequestServices = services;
		}


	
	}
}
