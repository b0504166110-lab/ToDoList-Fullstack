using Microsoft.EntityFrameworkCore;
using TodoApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync()).RequireAuthorization();

app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
}).RequireAuthorization();

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updated) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updated.Name ?? item.Name;
    item.IsComplete = updated.IsComplete ?? item.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
}).RequireAuthorization();

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/register",async(ToDoDbContext db,LoginRequest request) =>
{
    if(await db.Users.AnyAsync(u => u.UserName == request.UserName))
    {
return Results.Conflict("שם משתמש תפוס!");    }
    User user=new User()
    {
        UserName=request.UserName,
        Password=BCrypt.Net.BCrypt.HashPassword(request.Password)
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.MapPost("/login",async(ToDoDbContext db,LoginRequest request) =>
{
    var currentUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
if (currentUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, currentUser.Password))
{
    return Results.Unauthorized();
}
var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, currentUser.Id.ToString()),
        new Claim(ClaimTypes.Name, currentUser.UserName)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

    
});

app.Run();
record LoginRequest(string UserName, string Password);
