using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Enums
{
	public enum OrderState
	{
		Pending,
		Processing,
		Completed,
		Cancelled,
		OutForDelivery
	}
}
