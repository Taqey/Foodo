using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Products.GetProduct.GetProduct
{
	public class GetProductQuery : IRequest<ApiResponse<ProductBaseDto>>
	{
		public int productId { get; set; }
	}
}
