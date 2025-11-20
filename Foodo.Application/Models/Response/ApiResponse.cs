namespace Foodo.Application.Models.Response
{
	public class ApiResponse<T>
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public T? Data { get; set; }

		public static ApiResponse<T> Success(T data, string message = "")
		{
			return new ApiResponse<T>
			{
				IsSuccess = true,
				Message = message,
				Data = data
			};
		}

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
