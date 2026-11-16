using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ViSah.Data;
using ViSah.Models;

namespace ViSah.Services;

public record LoginRequest(string Email, string Password);