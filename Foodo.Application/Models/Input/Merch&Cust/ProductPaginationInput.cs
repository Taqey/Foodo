using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Input;

public class ProductPaginationInput
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public string? UserId { get; set; } = null;
	public string? OrderBy { get; set; }=null;
}
