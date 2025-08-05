using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using l_hospital_mang.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Doctor", policy => policy.RequireRole("Doctor"));
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(new
        {
            StatusCode = 400,
            Message = "Validation failed.",
            Errors = errors
        });
    };
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };

    // ? ??? ?? ????? ????? ?????? ?????? ?? SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // ???? ?? ????? ???? ??? Hub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/delay"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = async context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Invalid or expired token."
                });

                await context.Response.WriteAsync(result);
            }
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Token is missing or unauthorized."
                });

                await context.Response.WriteAsync(result);
            }
        }
    };
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

var app = builder.Build();

async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Doctor", "Manager", "Secretary", "Receptionist", "Patient", "Driver" , "LabDoctor", "RadiographyDoctor" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRoles(roleManager);

    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.SeedAmbulanceCars(context); 
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<AmbulanceHub>("/ambulanceHub");
app.MapHub<DelayNotificationHub>("/hubs/delay");


app.Run();
