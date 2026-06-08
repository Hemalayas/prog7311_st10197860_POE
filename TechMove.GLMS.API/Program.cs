using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Interfaces;
using TechMove.GLMS.API.Repositories;
using TechMove.GLMS.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5001");

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// Services
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// Observer pattern — singletons so AuditLogObserver trail persists for the app lifetime
builder.Services.AddSingleton<AuditLogObserver>();
builder.Services.AddSingleton<IContractObserver, EmailNotificationObserver>();
builder.Services.AddSingleton<IContractObserver>(sp => sp.GetRequiredService<AuditLogObserver>());
builder.Services.AddSingleton<IContractObserver, DashboardObserver>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TechMove GLMS API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

// Auto-run EF Core migrations and seed demo data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (!db.Clients.Any())
    {
        var client1 = new TechMove.GLMS.API.Models.Client
        {
            Name = "Apex Freight Solutions",
            ContactDetails = "operations@apexfreight.co.za",
            Region = "Gauteng"
        };
        var client2 = new TechMove.GLMS.API.Models.Client
        {
            Name = "Cape Cargo International",
            ContactDetails = "logistics@capecargo.co.za",
            Region = "Western Cape"
        };
        var client3 = new TechMove.GLMS.API.Models.Client
        {
            Name = "Durban Dockside Logistics",
            ContactDetails = "admin@durbandock.co.za",
            Region = "KwaZulu-Natal"
        };
        db.Clients.AddRange(client1, client2, client3);
        await db.SaveChangesAsync();

        var contract1 = new TechMove.GLMS.API.Models.Contract
        {
            ClientId = client1.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Status = TechMove.GLMS.API.Models.ContractStatus.Active,
            ServiceLevel = "Premium — 24/7 support, guaranteed SLA"
        };
        var contract2 = new TechMove.GLMS.API.Models.Contract
        {
            ClientId = client2.Id,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2027, 2, 28),
            Status = TechMove.GLMS.API.Models.ContractStatus.Draft,
            ServiceLevel = "Standard — business hours support"
        };
        var contract3 = new TechMove.GLMS.API.Models.Contract
        {
            ClientId = client3.Id,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Status = TechMove.GLMS.API.Models.ContractStatus.Expired,
            ServiceLevel = "Basic — email support only"
        };
        var contract4 = new TechMove.GLMS.API.Models.Contract
        {
            ClientId = client1.Id,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2027, 5, 31),
            Status = TechMove.GLMS.API.Models.ContractStatus.Active,
            ServiceLevel = "Enterprise — dedicated account manager"
        };
        db.Contracts.AddRange(contract1, contract2, contract3, contract4);
        await db.SaveChangesAsync();

        db.ServiceRequests.AddRange(
            new TechMove.GLMS.API.Models.ServiceRequest
            {
                ContractId = contract1.Id,
                Description = "International freight shipment — Johannesburg to Frankfurt",
                ServiceLevel = "Standard Freight Handling — door-to-door land/air/sea transport",
                Cost = 4200.00m,
                CostZAR = 77700.00m,
                Status = TechMove.GLMS.API.Models.ServiceRequestStatus.Approved
            },
            new TechMove.GLMS.API.Models.ServiceRequest
            {
                ContractId = contract1.Id,
                Description = "Customs clearance for imported electronics batch",
                ServiceLevel = "Customs Clearance Processing — import/export documentation and duties",
                Cost = 850.00m,
                CostZAR = 15725.00m,
                Status = TechMove.GLMS.API.Models.ServiceRequestStatus.Pending
            },
            new TechMove.GLMS.API.Models.ServiceRequest
            {
                ContractId = contract4.Id,
                Description = "Warehousing for seasonal stock — 6 month storage",
                ServiceLevel = "Warehouse Storage & Handling — receiving, storage, and dispatch",
                Cost = 1500.00m,
                CostZAR = 27750.00m,
                Status = TechMove.GLMS.API.Models.ServiceRequestStatus.Pending
            }
        );
        await db.SaveChangesAsync();
    }
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechMove GLMS API v1"));

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
