using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication.DTOs;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        public IConfiguration Configuration { get; set; }
        private IStudentsDbService _studentsDbService;
        
        public AuthController(IConfiguration configuration, IStudentsDbService studentsDbService)
        {
            Configuration = configuration;
            _studentsDbService = studentsDbService;
        }
        
        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            var student = _studentsDbService.Login(request);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Login),
                new Claim(ClaimTypes.Name, student.FirstName + " " + student.LastName),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken=Guid.NewGuid()
            });
        }
    }
}