namespace Foodo.Application.Models.Response
{
	// Generic version مع Data
	public class ApiResponse<T>
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public T? Data { get; set; }

		// Factory method للنجاح
		public static ApiResponse<T> Success(T data, string message = "")
		{
			return new ApiResponse<T>
			{
				IsSuccess = true,
				Message = message,
				Data = data
			};
		}

		// Factory method للفشل
		public static ApiResponse<T> Failure(string message)
		{
			return new ApiResponse<T>
			{
				IsSuccess = false,
				Message = message,
				Data = default
			};
		}
	}

	// Non-generic version لما مش محتاج ترجع Data
	public class ApiResponse
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }

		public static ApiResponse Success(string message = "")
			=> new ApiResponse { IsSuccess = true, Message = message };

		public static ApiResponse Failure(string message)
			=> new ApiResponse { IsSuccess = false, Message = message };
	}
}
