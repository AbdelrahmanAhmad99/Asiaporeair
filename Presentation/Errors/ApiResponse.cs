namespace Presentation.Errors
{
    public class ApiResponse
	{
        public int StatusCode { get; set; }

		public string? Message { get; set; }
        
        public object? Data { get; set; }

		public ApiResponse(int statusCode, string? message = null)
		{
			StatusCode = statusCode;
			Message = message ?? GetDefaultMessageForStatusCode(statusCode);
		}

		public ApiResponse(int statusCode, string? message, object? data)
		{
			StatusCode = statusCode;
			Message = message ?? GetDefaultMessageForStatusCode(statusCode);
			Data = data;
		}
        private string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "The request could not be processed. Please check your input.",
                401 => "Authentication is required to proceed.",
                404 => "We could not find the resource you are looking for.",
                500 => "Something went wrong on our end. Please try again later.",
                _ => null,
            };
        }
    }
}