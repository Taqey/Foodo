using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.AddProductAttribute
{
	public class AddProductAttributeCommand : IRequest<ApiResponse>
	{
		public int ProductId { get; set; }
		public AttributeCreateInput attributes { get; set; }
	}
}
