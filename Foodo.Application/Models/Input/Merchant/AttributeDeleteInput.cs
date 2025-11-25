using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Input.Merchant
{
	public class AttributeDeleteInput
	{
		public List<int> Attributes { get; set; } = new List<int>();
	}
}
