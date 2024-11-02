using Contractors.DbContractorsAuctioneerEF;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Models;
using Contractors.Services;
using ContractorsAuctioneer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddTransient<DatabaseInitializer>();

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IVerificationService, VerificationService>();
// Request
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IRequestStatusService, RequestStatusService>();
builder.Services.AddSingleton<IHostedService, RequestCheckService>();
// Client
builder.Services.AddScoped<IClientService, ClientService>();
// Region
builder.Services.AddScoped<IRegionService, RegionService>();
// Contractor
builder.Services.AddScoped<IContractorService, ContractorService>();
// BidOfContractor
builder.Services.AddScoped<IBidOfContractorService, BidOfContractorService>();
builder.Services.AddScoped<IBidStatusService, BidStatusService>();
builder.Services.AddSingleton<IHostedService, BidOfContractorCheckService>();

// Project
builder.Services.AddScoped<IProjectService, ProjectService>();
// FileAttachment
builder.Services.AddScoped<IFileAttachmentService, FileAttachmentService>();
// LoginHistory
builder.Services.AddTransient<ILastLoginHistoryService, LastLoginHistoryService>();
// Reject
builder.Services.AddTransient<IRejectService, RejectService>();




//----------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext and Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated for user: " + context.Principal.Identity.Name);
                return Task.CompletedTask;
            }
        };
    });

// Add this configuration in the ConfigureServices method
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath); // Include the XML comments
});

#region CORS

builder.Services.AddCors(o => o.AddPolicy(name: "MyPolicy", builder =>
{
    builder.WithOrigins("*") //.AllowAnyOrigin() //WithOrigins("http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader();
    //.AllowCredentials();
}));

#endregion
var app = builder.Build();

// Seed the database with users and roles
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Client"))
    {
        await roleManager.CreateAsync(new IdentityRole<int>("Client"));
    }

    var user = await userManager.FindByNameAsync("client");
    if (user == null)
    {
        user = new ApplicationUser { UserName = "client", Email = "client@example.com" };
        await userManager.CreateAsync(user, "Client@123");
        await userManager.AddToRoleAsync(user, "Client");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.OAuthClientId("swagger");
        options.OAuthAppName("Swagger UI");
    });
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(policyName: "MyPolicy");

app.MapControllers();

app.Run();

//Seed the database with users and roles
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

//    if (!await roleManager.RoleExistsAsync("Client"))
//    {
//        await roleManager.CreateAsync(new IdentityRole("Client"));
//    }

//    var user = await userManager.FindByNameAsync("client");
//    if (user == null)
//    {
//        user = new IdentityUser { UserName = "client", Email = "client@example.com" };
//        await userManager.CreateAsync(user, "Client@123");
//        await userManager.AddToRoleAsync(user, "Client");
//    }
//}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
//        options.OAuthClientId("swagger");
//        options.OAuthAppName("Swagger UI");
//    });
//}


//app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();

//app.Run();
