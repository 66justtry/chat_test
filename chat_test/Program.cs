using chat_test;
using chat_test.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var people = new List<User>
 {
    new User("tom", "11111"),
    new User("bob", "5555"),
    new User("sam", "22222")
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>(); // Устанавливаем сервис для получения Id пользователя

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
options.TokenValidationParameters = new TokenValidationParameters
{
ValidateIssuer = true,
ValidIssuer = AuthOptions.ISSUER,
ValidateAudience = true,
ValidAudience = AuthOptions.AUDIENCE,
ValidateLifetime = true,
IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
ValidateIssuerSigningKey = true
};

options.Events = new JwtBearerEvents
{
OnMessageReceived = context =>
{
var accessToken = context.Request.Query["access_token"];

// если запрос направлен хабу
var path = context.HttpContext.Request.Path;
if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
{
// получаем токен из строки запроса
context.Token = accessToken;
}
return Task.CompletedTask;
}
};
});

builder.Services.AddSignalR();

var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();   // добавление middleware аутентификации 
app.UseAuthorization();   // добавление middleware авторизации 


app.MapPost("/login", (User loginModel) =>
{
// находим пользователя 
User? person = people.FirstOrDefault(p => p.login == loginModel.login);
    // если пользователь не найден, отправляем статусный код 401
    if (person is null) 
    {
        //loginModel.chats = "all";
        people.Add(loginModel);
        person = loginModel;
    } 

var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, person.login) };
// создаем JWT-токен
var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

// формируем ответ
var response = new
{
access_token = encodedJwt,
username = person.login
//chats = person.chats
};

return Results.Json(response);
});

app.MapHub<ChatHub>("/chat");
app.Run();


public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // издатель токена
    public const string AUDIENCE = "MyAuthClient"; // потребитель токена
    const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
