using Microsoft.AspNetCore.Mvc;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Converte ResultDto para ActionResult padronizado
        /// </summary>
        public static ActionResult ToActionResult<T>(this ControllerBase controller, ResultDto<T> result,
            string successMessage = "Operação realizada com sucesso")
        {
            if (result.IsSuccess)
                return controller.Ok(ApiResponse<T>.SuccessResponse(result.Data!, successMessage));

            // Determina o status code baseado na mensagem de erro
            if (result.Error?.ToLower().Contains("não encontrada") == true ||
                result.Error?.ToLower().Contains("não encontrado") == true)
            {
                return controller.NotFound(ApiResponse<T>.ErrorResponse(result.Error));
            }

            if (result.Error?.ToLower().Contains("não autorizado") == true ||
                result.Error?.ToLower().Contains("acesso negado") == true)
            {
                return controller.Unauthorized(ApiResponse<T>.ErrorResponse(result.Error));
            }

            // Erro de validação/regra de negócio
            return controller.BadRequest(ApiResponse<T>.ErrorResponse(result.Error!));
        }

        /// <summary>
        /// Para operações que retornam apenas sucesso/falha
        /// </summary>
        public static ActionResult ToActionResult(this ControllerBase controller, ResultDto<bool> result,
            string successMessage = "Operação realizada com sucesso")
        {
            if (result.IsSuccess)
                return controller.Ok(ApiResponse.SuccessResponse(successMessage));

            if (result.Error?.ToLower().Contains("não encontrada") == true ||
                result.Error?.ToLower().Contains("não encontrado") == true)
                return controller.NotFound(ApiResponse.ErrorResponse(result.Error));

            if (result.Error?.ToLower().Contains("não autorizado") == true ||
                result.Error?.ToLower().Contains("acesso negado") == true)
                return controller.Unauthorized(ApiResponse.ErrorResponse(result.Error));

            return controller.BadRequest(ApiResponse.ErrorResponse(result.Error!));
        }
    }
}
