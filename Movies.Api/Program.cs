using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api;
using Movies.Api.Middleware;
using Movies.Application;
using Movies.Application.Database;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"]!,
        ValidateIssuer = true,
        ValidAudience = config["Jwt:Audience"]!,
        ValidateAudience = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthConstants.AdminUserPolicyName,
        policy => policy.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
    options.AddPolicy(AuthConstants.TrustedMemberPolicyName, policy => policy.RequireAssertion(context =>
        context.User.HasClaim(match => match is { Type: AuthConstants.AdminUserClaimName, Value: "true" }) ||
        context.User.HasClaim(match => match is { Type: AuthConstants.TrustedMemberClaimName, Value: "true" })
    ));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddDatabase(config.GetConnectionString("MoviesDatabase")!);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();