using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class CustomControllerBase : ControllerBase
{
    protected virtual ObjectResult BadRequest(string status, string message)
    {
        return Problem(
            detail: message,
            statusCode: StatusCodes.Status400BadRequest,
            title: status
        );
    }
}
