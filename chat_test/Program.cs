using chat_test;
using chat_test.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var people = new List<User>
 {
    new User("tom", "11111", "pass", "all friends bro"),
    new User("bob", "5555"),
    new User("sam", "22222")
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>(); // ������������� ������ ��� ��������� Id ������������
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ���� ������ ��������� ����
var path = context.HttpContext.Request.Path;
if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
{
// �������� ����� �� ������ �������
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

app.UseAuthentication();   // ���������� middleware �������������� 
app.UseAuthorization();   // ���������� middleware ����������� 


app.MapPost("/login", (User loginModel) =>
{
// ������� ������������ 
User? person = people.FirstOrDefault(p => p.login == loginModel.login);
    
    if (person is null) 
    {
        //loginModel.chats = "all";
        people.Add(new User(loginModel.login, loginModel.username, loginModel.password, loginModel.chats));
    }
    person = people.FirstOrDefault(p => p.login == loginModel.login);

    var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, person.login) };
// ������� JWT-�����
var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

// ��������� �����
var response = new
{
access_token = encodedJwt,
username = person.login,
chats = person.chats
};

return Results.Json(response);
});

app.MapHub<ChatHub>("/chat");
app.Run();


public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // �������� ������
    public const string AUDIENCE = "MyAuthClient"; // ����������� ������
    const string KEY = "mysupersecret_secretkey!123";   // ���� ��� ��������
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
