namespace Foodo.Domain.Entities
{
	public class TblProductCategory
	{
		public int productcategoryid { get; set; }

		public int productid { get; set; }
		public int categoryid { get; set; }
		public virtual TblProduct Product { get; set; }
		public virtual TblCategoryOfProduct Category { get; set; }

	}
}
