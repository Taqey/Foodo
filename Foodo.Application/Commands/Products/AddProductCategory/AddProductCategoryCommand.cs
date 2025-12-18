using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.AddProductCategory
{
	public class AddProductCategoryCommand : IRequest<ApiResponse>
	{
		public ProductCategoryInput input { get; set; }
	}
}
