using Common.Constants;

namespace BusinessObject.DTOs.Response;

/// <summary>
/// Generic base response for all API endpoints
/// </summary>
/// <typeparam name="T">The type of data in the response</typeparam>
public class BaseResponse<T>
{
    public string Message { get; set; } = "Successful";
    public StatusCodeEnum StatusCode { get; set; } = StatusCodeEnum.OK_200;
    public T? Data { get; set; }
    
    public BaseResponse()
    {
    }
    
    public BaseResponse(string message, StatusCodeEnum statusCode, T? data)
    {
        Message = message;
        StatusCode = statusCode;
        Data = data;
    }
    
    /// <summary>
    /// Create a successful response
    /// </summary>
    public static BaseResponse<T> Success(T? data, string message = "Successful")
    {
        return new BaseResponse<T>(message, StatusCodeEnum.OK_200, data);
    }
    
    /// <summary>
    /// Create an error response
    /// </summary>
    public static BaseResponse<T> Error(string message, StatusCodeEnum statusCode = StatusCodeEnum.BadRequest_400)
    {
        return new BaseResponse<T>(message, statusCode, default(T));
    }
}
