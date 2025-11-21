using Foodo.Application.Models.Dto;

namespace Foodo.API.Models.Request
{
	public class AttributeCreateRequest
	{
		public List<AttributeDto> Attributes { get; set; }= new List<AttributeDto>();
	}
}
