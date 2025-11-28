using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Entities
{
	public class TblProductPhoto
	{
		public int Id { get; set; }
		public int ProductId { get; set; }

		public string Url { get; set; }
		public bool isMain { get; set; }
		public virtual TblProduct TblProduct { get; set; }
	}
}
