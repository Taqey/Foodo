using Foodo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Entities
{
	public class TblCategoryOfRestaurant
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public virtual ICollection<TblRestaurantCategory> RestaurantCategories { get; set; }= new List<TblRestaurantCategory>();


	}
}
