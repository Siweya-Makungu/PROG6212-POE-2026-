using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<RaceDayDbContext>(options=>options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDatabase")));
builder.Services.AddScoped<TokenService>();
var jwtKey=builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Set Jwt:Key using dotnet user-secrets or environment variables before starting.");
if(Encoding.UTF8.GetByteCount(jwtKey)<32)throw new InvalidOperationException("Jwt:Key must be at least 32 bytes.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options=>
{
 options.TokenValidationParameters=new TokenValidationParameters
 {
  ValidateIssuer=false,ValidateAudience=false,ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),ValidateLifetime=true,ClockSkew=TimeSpan.FromMinutes(1)
 };
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{
 c.SwaggerDoc("v1",new OpenApiInfo{Title="RaceDay API",Version="v1",Description="South African running, walking and cycling event management"});
 c.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme{Description="Enter JWT token only (Swagger adds Bearer prefix)",Name="Authorization",In=ParameterLocation.Header,Type=SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT"});
 c.AddSecurityRequirement(new OpenApiSecurityRequirement{{new OpenApiSecurityScheme{Reference=new OpenApiReference{Type=ReferenceType.SecurityScheme,Id="Bearer"}},Array.Empty<string>()}});
 var xml=Path.Combine(AppContext.BaseDirectory,$"{typeof(Program).Assembly.GetName().Name}.xml");if(File.Exists(xml))c.IncludeXmlComments(xml);
});
var app=builder.Build();
app.UseSwagger();app.UseSwaggerUI();
app.UseHttpsRedirection();app.UseAuthentication();app.UseAuthorization();app.MapControllers();
app.Run();
public partial class Program {}
