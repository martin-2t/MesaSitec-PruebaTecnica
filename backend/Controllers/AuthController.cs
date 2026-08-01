using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var respuesta = await _authService.LoginAsync(request);

        if (respuesta == null)
        {
            return Unauthorized(new
            {
                codigo = "NO_AUTENTICADO"
            });
        }

        return Ok(respuesta);
    }
}