using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.RemoveProductAttribute
{
	public class RemoveProductAttributeCommand : IRequest<ApiResponse>
	{
		public int ProductId { get; set; }
		public List<int> Attributes { get; set; } = new List<int>();
	}
}
