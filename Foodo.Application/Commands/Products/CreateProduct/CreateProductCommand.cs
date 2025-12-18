using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Commands.Products.CreateProduct
{
	public class CreateProductCommand : IRequest<ApiResponse<CreateProductDto>>
	{
		public string Id { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
		public ICollection<AttributeDto>? Attributes { get; set; } = new List<AttributeDto>();
		public ICollection<FoodCategory>? Categories { get; set; } = new List<FoodCategory>();
	}
}
