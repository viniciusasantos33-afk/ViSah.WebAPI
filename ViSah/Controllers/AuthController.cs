using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViSah.Services;
using ViSah.Interfaces;

namespace ViSah.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name) || string.IsNullOrWhiteSpace(r.Password) || r.Password.Length < 8 ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(r.Email))
        {
            return BadRequest(new
                { message = "Nome, e-mail válido e senha com ao menos 8 caracteres são obrigatórios." });
        }

        var token = await auth.Register(r);
        if (token is null)
        {
            return Conflict(new { message = "E-mail já cadastrado." });
        }

        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var result = await auth.Login(r);
        if (result.Token is null)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(new { token = result.Token });
    }

    [Authorize, HttpPost("logout")]
    public IActionResult Logout() => Ok(new { message = "Sessão encerrada. Descarte o token no cliente." });
}