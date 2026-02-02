using AuthSandbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<JwtService>();


// Dependency Injection.
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


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


app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
}).RequireAuthorization();

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync()).RequireAuthorization();

app.Run();

public record LoginRequest(string Username, string Password);
