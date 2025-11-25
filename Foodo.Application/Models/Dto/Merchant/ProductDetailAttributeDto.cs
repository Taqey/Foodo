using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto.Merchant
	{
		public class ProductDetailAttributeDto
		{
			public int Id { get; set; }
			public string AttributeName { get; set; }
			public string AttributeValue { get; set; }
			public string MeasurementUnit { get; set; }
		}

	}
