using Foodo.Domain.Enums;

namespace Foodo.Application.Models.Input.Customer
{
	public class ProductPaginationByCategoryInput
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public FoodCategory Category { get; set; }
	}
}