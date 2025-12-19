using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Products.AddProductAttribute
{
	public class AddProductAttributeCommand : IRequest<ApiResponse>
	{
		public int ProductId { get; set; }
		public List<AttributeDto> Attributes { get; set; } = new List<AttributeDto>();
	}
}
