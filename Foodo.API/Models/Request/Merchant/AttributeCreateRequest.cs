using Foodo.Application.Models.Dto;

namespace Foodo.API.Models.Request.Merchant
{
	public class AttributeCreateRequest
	{
		public List<AttributeDto> Attributes { get; set; }= new List<AttributeDto>();
	}
}
