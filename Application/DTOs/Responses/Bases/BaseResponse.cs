using Domain.Enums;

namespace Application.DTOs.Responses.Bases
{
	public class BaseResponse<T>
	{
		public string? Message { get; set; }
		public StatusCodes StatusCode { get; set; }
		public T? Data { get; set; }

		public BaseResponse() { }

		public BaseResponse(string? message, StatusCodes statusCode, T? data)
		{
			Message = message;
			StatusCode = statusCode;
			Data = data;
		}
	}
}
