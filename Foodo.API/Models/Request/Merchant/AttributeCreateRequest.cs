using Foodo.Application.Models.Dto;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Merchant
{
	public class AttributeCreateRequest
	{
		[Required]
		public List<AttributeDto> Attributes { get; set; } = new List<AttributeDto>();
	}
}
