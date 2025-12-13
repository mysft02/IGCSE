using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using IGCSE.Middleware;
using BusinessObject;
using BusinessObject.Model;
using IGCSE.Extensions;


// Ensure wwwroot exists BEFORE building the web application to avoid DirectoryNotFoundException
var preBuildWebRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(preBuildWebRoot))
{
    Directory.CreateDirectory(preBuildWebRoot);
}

var builder = WebApplication.CreateBuilder(args);

// Set configuration for static API request classes
BusinessObject.Payload.Request.TrelloApiRequest.SetConfiguration(builder.Configuration);
BusinessObject.Payload.Request.PayOS.PayOSApiRequest.SetConfiguration(builder.Configuration);
BusinessObject.Payload.Request.OpenAI.OpenApiRequest.SetConfiguration(builder.Configuration);

builder.Services.AddCors(options =>
{
    // Policy for SignalR and APIs with credentials
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:5174", "http://160.250.132.11:5173" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR with access tokens
    });
    
    // Fallback policy for APIs without credentials (if needed)
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var connectionString = builder.Configuration.GetConnectionString("DbConnection");
// Connect Database
builder.Services.AddDbContext<IGCSEContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 34))
    ));

// Đăng ký dịch vụ thông qua extension methods
builder.Services
    .AddRepositoryServices()
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddBackgroundTaskServices();

builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IGCSE", Version = "V1" });

    c.EnableAnnotations();


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
          new OpenApiSecurityScheme
          {
              Reference = new OpenApiReference
              {
                  Type = ReferenceType.SecurityScheme,
                  Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,
          },
          new string[] {}
      }
    });
});

// SetUp Specification for password
builder.Services.AddIdentity<Account, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<IGCSEContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(1);
});

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        ),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        RequireSignedTokens = true
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/notificationHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });


var app = builder.Build();

// Khởi tạo các role khi ứng dụng chạy
// using (var scope = app.Services.CreateScope())
// {
//     var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
//
//     // Danh sách các role cần tạo
//     string[] roleNames = { "Admin", "Parent", "Student", "Teacher", "Manager" };
//
//     foreach (var roleName in roleNames)
//     {
//         if (!await roleManager.RoleExistsAsync(roleName))
//         {
//             await roleManager.CreateAsync(new IdentityRole(roleName));
//         }
//     }
//
//     // Tạo tài khoản Admin mặc định nếu chưa có, hoặc cập nhật role nếu đã có
//     var adminUser = await userManager.FindByNameAsync("admin");
//     if (adminUser == null)
//     {
//         adminUser = new Account
//         {
//             UserName = "admin",
//             Email = "admin@example.com",
//             Name = "System Administrator",
//             Address = "Admin Address",
//             Phone = "0123456789",
//             Status = true,
//             DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30))
//         };
//
//         var result = await userManager.CreateAsync(adminUser, "A123456789a!");
//         if (result.Succeeded)
//         {
//             await userManager.AddToRoleAsync(adminUser, "Admin");
//         }
//     }
//     else
//     {
//         // Tài khoản admin đã tồn tại, kiểm tra và cập nhật role
//         var currentRoles = await userManager.GetRolesAsync(adminUser);
//         if (!currentRoles.Contains("Admin"))
//         {
//             // Xóa tất cả role cũ
//             if (currentRoles.Any())
//             {
//                 await userManager.RemoveFromRolesAsync(adminUser, currentRoles);
//             }
//             // Thêm role Admin
//             await userManager.AddToRoleAsync(adminUser, "Admin");
//         }
//     }
// }

app.UseRouting();
app.UseCors("AllowSpecificOrigins"); // Use policy with credentials support
app.UseGlobalExceptionHandling();

//app.UseCustomJwtBearer();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Ensure wwwroot exists to avoid DirectoryNotFoundException when serving static files
var webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IGCSE");
    c.RoutePrefix = "swagger";
});


app.MapControllers();

// Map SignalR Hub
app.MapHub<IGCSE.Hubs.ChatHub>("/chatHub");

app.Run();
