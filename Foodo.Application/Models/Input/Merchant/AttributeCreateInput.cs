using Foodo.Application.Models.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Input.Merchant
{
	public class AttributeCreateInput
	{
		public List<AttributeDto> Attributes { get; set; } = new List<AttributeDto>();

	}
}
