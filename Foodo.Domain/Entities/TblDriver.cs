using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Entities
{
	public class TblDriver
	{
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Gender { get; set; }
		public DateOnly BirthDate { get; set; }
		public string NationalId {  get; set; }

	}
}
