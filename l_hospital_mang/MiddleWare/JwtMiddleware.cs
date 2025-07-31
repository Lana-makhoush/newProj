using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using l_hospital_mang.Data; 
using Microsoft.EntityFrameworkCore;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context, AppDbContext dbContext)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userEmail = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
                var tokenIssuedAt = jwtToken.ValidFrom;

                var doctor = await dbContext.Doctorss.FirstOrDefaultAsync(d => d.Email == userEmail);

                if (doctor == null || doctor.LastLoginAt > tokenIssuedAt)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized - invalid or expired token.");
                    return;
                }

                
                context.Items["Doctor"] = doctor;
            }
            catch
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized - token validation failed.");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized - token missing.");
            return;
        }

        await _next(context);
    }
}
