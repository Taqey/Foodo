using Foodo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Entities
{
	public class TblCategoryOfProduct
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public virtual ICollection<TblProductCategory> ProductCategories { get; set; }= new List<TblProductCategory>();
	}
}
