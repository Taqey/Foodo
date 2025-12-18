using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.UpdateProduct
{
	public class UpdateProductCommand : IRequest<ApiResponse>
	{
		public int productId { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
	}
}
