using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

using Microsoft.Extensions.FileProviders;

using Microsoft.AspNetCore.Http.Features;
using LibraryManagementSystem.Data;
using Models.DTOModel;
using NuGet.Protocol.Plugins;
using Stripe.Climate;
using Stripe;
using Utility;
using Models.DTOModel.JWT;

var builder = WebApplication.CreateBuilder(args);

// Set up your configuration
builder.Configuration.AddJsonFile("appsettings.json");
var Configuration = builder.Configuration;
SharedUtilityConnection.AppConfiguration = Configuration;

builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));
// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("V1", new OpenApiInfo
    {
        Version = "V1",
        Title = "CompCorePro",
        Description = "CompCorePro WebAPI"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
});
});
builder.Services.AddCors(op =>
{
    op.AddPolicy("AddCors", po =>
    {
        po.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
     .AddJwtBearer(options =>
     {
         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             ValidIssuer = SharedUtilityConnection.AppConfiguration["JWT:ValidIssuer"],
             ValidAudience = SharedUtilityConnection.AppConfiguration["JWT:ValidAudience"],
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SharedUtilityConnection.AppConfiguration["JWT:Secret"]))
         };
     });








var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
        options.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/V1/swagger.json", "CompCorePro APIs");
    });
    app.UseCors("AddCors");
    //using (var scope = app.Services.CreateScope())
    //{
    //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //    db.Database.Migrate();
    //}
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/wwwroot"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

