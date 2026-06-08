using TechMove.GLMS.Interfaces;
using TechMove.GLMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// JWT token store — singleton so the token survives across requests
builder.Services.AddSingleton<ApiTokenStore>();

// HttpClient for the backend API
builder.Services.AddHttpClient<ApiService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

// Observer registrations — singletons so AuditLogObserver's trail persists for the app lifetime
builder.Services.AddSingleton<AuditLogObserver>();
builder.Services.AddSingleton<IContractObserver, EmailNotificationObserver>();
builder.Services.AddSingleton<IContractObserver>(sp => sp.GetRequiredService<AuditLogObserver>());
builder.Services.AddSingleton<IContractObserver, DashboardObserver>();

builder.Services.AddHttpClient<CurrencyService>();
builder.Services.AddScoped<CurrencyCalculationService>();
builder.Services.AddScoped<FileValidationService>();
builder.Services.AddScoped<WorkflowValidationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
