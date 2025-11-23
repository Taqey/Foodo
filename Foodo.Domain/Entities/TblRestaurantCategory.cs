using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Entities
{
	public class TblRestaurantCategory
	{
		public int restaurantcategoryid { get; set; }
		public string restaurantid { get; set; }
		public int categoryid { get; set; }
		public virtual TblMerchant Restaurant { get; set; }
		public virtual TblCategoryOfRestaurant Category { get; set; }
	}
}
