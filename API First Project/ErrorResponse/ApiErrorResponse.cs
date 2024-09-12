namespace API_First_Project.ErrorResponse
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Errors { get; set; }
        public ApiErrorResponse(int statusCode, string message, Dictionary<string, string> errors = null!)
        {
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }
    }

}
