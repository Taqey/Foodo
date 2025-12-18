using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.DeleteProduct
{
	public class DeleteProductCommand : IRequest<ApiResponse>
	{
		public int productId { get; set; }
		public string UserId { get; set; }

	}
}
