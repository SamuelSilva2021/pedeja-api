using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Common.Interfaces;
using PedejaApp.Application.Common.Models;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorageService,
        ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<FileUploadResult>> UploadFile(
        IFormFile file,
        [FromForm] string folder = "products")
    {
        try
        {
            if (file == null)
            {
                return BadRequest(new FileUploadResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Nenhum arquivo foi fornecido."
                });
            }

            _logger.LogInformation("Uploading file: {FileName}, Size: {Size} bytes", 
                file.FileName, file.Length);

            var result = await _fileStorageService.UploadImageAsync(file, folder);

            if (result.IsSuccess)
            {
                _logger.LogInformation("File uploaded successfully: {FilePath}", result.FilePath);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("File upload failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            
            return StatusCode(500, new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Erro interno do servidor durante o upload."
            });
        }
    }

    [HttpPost("upload-multiple")]
    public async Task<ActionResult<List<FileUploadResult>>> UploadMultipleFiles(
        List<IFormFile> files,
        [FromForm] string folder = "products")
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new { error = "Nenhum arquivo foi fornecido." });
            }

            if (files.Count > 10)
            {
                return BadRequest(new { error = "Máximo de 10 arquivos por vez." });
            }

            var results = new List<FileUploadResult>();

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    var result = await _fileStorageService.UploadImageAsync(file, folder);
                    results.Add(result);
                }
            }

            _logger.LogInformation("Uploaded {Count} files", results.Count(r => r.IsSuccess));

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple files");
            
            return StatusCode(500, new { error = "Erro interno do servidor durante o upload." });
        }
    }

    [HttpDelete("delete")]
    public async Task<ActionResult> DeleteFile([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest(new { error = "Caminho do arquivo é obrigatório." });
            }

            var result = await _fileStorageService.DeleteFileAsync(filePath);

            if (result)
            {
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return Ok(new { message = "Arquivo excluído com sucesso." });
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return NotFound(new { error = "Arquivo não encontrado." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return StatusCode(500, new { error = "Erro interno do servidor durante a exclusão." });
        }
    }

    [HttpGet("url")]
    [AllowAnonymous]
    public ActionResult<string> GetFileUrl([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest(new { error = "Caminho do arquivo é obrigatório." });
            }

            var url = _fileStorageService.GetFileUrl(filePath);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL: {FilePath}", filePath);
            return StatusCode(500, new { error = "Erro interno do servidor." });
        }
    }

    [HttpGet("variants")]
    [AllowAnonymous]
    public async Task<ActionResult<List<string>>> GetImageVariants([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest(new { error = "Caminho do arquivo é obrigatório." });
            }

            var variants = await _fileStorageService.GetImageVariantsAsync(filePath);
            return Ok(variants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image variants: {FilePath}", filePath);
            return StatusCode(500, new { error = "Erro interno do servidor." });
        }
    }
}
