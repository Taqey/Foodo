using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.RemoveProductCategory
{
	public class RemoveProductCategoryCommand : IRequest<ApiResponse>
	{
		public ProductCategoryInput categoryInput { get; set; }
	}
}
