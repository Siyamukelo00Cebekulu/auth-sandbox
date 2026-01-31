using AuthSandbox;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<JwtService>();

var jwtService = new JwtService();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtService.GetKey()
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/login", (LoginRequest request, JwtService jwt) =>
{
    if (!Validate(request.Username, request.Password))
        return Results.Unauthorized();

    var token = jwt.GenerateToken(request.Username);
    return Results.Ok(new { token });
});

bool Validate(string username, string password)
{
    return username == "admin" && password == "1234";
}

app.MapGet("/secret", () =>
{
    return "You are authenticated!";
})
.RequireAuthorization();

app.Run();

public record LoginRequest(string Username, string Password);
