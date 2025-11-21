using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto
{
	public class AttributeDto
	{
		public int ProductDetailAttributeId { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }
		public string MeasurementUnit { get; set; }
	}
}
