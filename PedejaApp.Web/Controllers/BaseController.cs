using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Interfaces;
using System.Net;

namespace PedejaApp.Web.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// BuildResponse 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="serviceResponse"></param>
        /// <param name="response"></param>
        /// <param name="errorHandler"></param>
        /// <param name="eTagFromObject"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public ActionResult BuildResponse<R>(ResponseDTO<R> serviceResponse, dynamic response,
            Func<IList<ErrorDTO>, object>? errorHandler = null, object? eTagFromObject = null, string? etag = null)
        {
            return serviceResponse.Code switch
            {
                StatusCodes.Status200OK => response is null ? Ok() : Ok(response),
                StatusCodes.Status201Created => Created("", response),
                StatusCodes.Status204NoContent => NoContent(),
                StatusCodes.Status206PartialContent => StatusCode(StatusCodes.Status206PartialContent, response),
                StatusCodes.Status400BadRequest => BadRequest(GetResponseError(serviceResponse, errorHandler)),
                StatusCodes.Status401Unauthorized => Unauthorized(GetResponseError(serviceResponse, errorHandler)),
                StatusCodes.Status404NotFound => NotFound(GetResponseError(serviceResponse, errorHandler)),
                _ => DefaultResponse(serviceResponse, response)
            };
        }

        /// <summary>
        /// BuildResponse
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="serviceResponse"></param>
        /// <param name="errorHandler"></param>
        /// <param name="eTagFromObject"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public ActionResult BuildResponse<R>(ResponseDTO<R> serviceResponse,
            Func<IList<ErrorDTO>, object>? errorHandler = null, object? eTagFromObject = null, string? etag = null)
        {
            return serviceResponse.Code switch
            {
                StatusCodes.Status200OK => serviceResponse.Data is null ? Ok() : Ok(serviceResponse.Data),
                StatusCodes.Status204NoContent => NoContent(),
                StatusCodes.Status206PartialContent =>
                    StatusCode(StatusCodes.Status206PartialContent, serviceResponse.Data),
                StatusCodes.Status400BadRequest => BadRequest(GetResponseError(serviceResponse, errorHandler)),
                StatusCodes.Status401Unauthorized => Unauthorized(GetResponseError(serviceResponse, errorHandler)),
                StatusCodes.Status404NotFound => NotFound(GetResponseError(serviceResponse, errorHandler)),
                _ => DefaultResponse(serviceResponse, serviceResponse.Data)
            };
        }

        /// <summary>
        /// Build Empty Response
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="serviceResponse"></param>
        /// <returns></returns>
        public ActionResult BuildEmptyResponse<R>(ResponseDTO<R> serviceResponse,
            Func<IList<ErrorDTO>, object>? errorHandler = null) =>
            serviceResponse.Code switch
            {
                StatusCodes.Status200OK => Ok(),
                StatusCodes.Status204NoContent => NoContent(),
                StatusCodes.Status206PartialContent => StatusCode(StatusCodes.Status206PartialContent),
                StatusCodes.Status400BadRequest => BadRequest(GetResponseError(serviceResponse, errorHandler)),
                StatusCodes.Status401Unauthorized => Unauthorized(GetResponseError(serviceResponse, errorHandler)),
                StatusCodes.Status404NotFound => NotFound(GetResponseError(serviceResponse, errorHandler)),
                _ => DefaultResponse(serviceResponse)
            };

        protected virtual object ParseResponseErrors(IList<ErrorDTO> errors) =>
            errors is not null ? errors : new List<ErrorDTO>();

        private ObjectResult DefaultResponse<R, T>(ResponseDTO<R> serviceResponse, T data,
            Func<IList<ErrorDTO>, object>? errorHandler = null)
        {
            var objectResponse =
                new ObjectResult(serviceResponse.Succeeded ? data : GetResponseError(serviceResponse, errorHandler))
                {
                    StatusCode = GetResponseStatusCode(serviceResponse)
                };

            return objectResponse;
        }

        private ObjectResult DefaultResponse<R>(ResponseDTO<R> serviceResponse,
            Func<IList<ErrorDTO>, object>? errorHandler = null) =>
            new ObjectResult(serviceResponse.Succeeded
                ? new EmptyResult()
                : GetResponseError(serviceResponse, errorHandler))
            {
                StatusCode = GetResponseStatusCode(serviceResponse)
            };

        private object GetResponseError<R>(ResponseDTO<R> serviceResponse,
            Func<IList<ErrorDTO>, object>? errorHandler = null!) =>
            errorHandler is null ? ParseResponseErrors(serviceResponse.Errors) : errorHandler.Invoke(serviceResponse.Errors);

        private static int GetResponseStatusCode<R>(ResponseDTO<R> serviceResponse) =>
            Enum.IsDefined(typeof(HttpStatusCode), serviceResponse.Code)
                ? serviceResponse.Code
                : (int)HttpStatusCode.InternalServerError;

        public static void AddLocationHeader(HttpContext context, string location)
        {
            context.Response.Headers.Location = location;
        }

        public static string GetUserAgent(HttpContext context) => context?.Request?.Headers.UserAgent.ToString() ?? "";

        #region Access Control Operations

        /// <summary>
        /// Operation Insert
        /// </summary>
        public const string OPERATION_INSERT = "INSERT";

        /// <summary>
        /// Operation Select
        /// </summary>
        public const string OPERATION_SELECT = "SELECT";

        /// <summary>
        /// Operation Update
        /// </summary>
        public const string OPERATION_UPDATE = "UPDATE";

        /// <summary>
        /// Operation Delete
        /// </summary>
        public const string OPERATION_DELETE = "DELETE";

        /// <summary>
        /// Operation Deferral
        /// </summary>
        public const string OPERATION_DEFERRAL = "DEFERRAL";

        /// <summary>
        /// Operation Activation
        /// </summary>
        public const string OPERATION_ACTIVATION = "ACTIVATION";

        /// <summary>
        /// Operation Reversal
        /// </summary>
        public const string OPERATION_REVERSAL = "REVERSAL";

        /// <summary>
        /// Operation Import
        /// </summary>
        public const string OPERATION_IMPORT = "IMPORT";

        /// <summary>
        /// Operation Cancellation
        /// </summary>
        public const string OPERATION_CANCELLATION = "CANCELLATION";

        #endregion
    }
}
