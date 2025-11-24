using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto
{
	public class PaginationDto<T>
	{
		public int TotalPages {  get; set; }
		public int TotalItems  { get; set; }
		public List<T> Items { get; set; }= new List<T>();

	}
}
