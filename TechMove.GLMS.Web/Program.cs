using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Services.Strategies;
using TechMove.GLMS.Core.Services.Factories;
using TechMove.GLMS.Core.Services.Observers;
using TechMove.GLMS.Core.Services;
using TechMove.GLMS.Core.Services.CurrencyExchange;

var builder = WebApplication.CreateBuilder(args);

//EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Strategy Pattern: registration for currency conversion strategies
builder.Services.AddScoped<ICurrencyStrategy, UsdToZarStrategy>();
builder.Services.AddScoped<ICurrencyStrategy, EurToZarStrategy>();
builder.Services.AddScoped<ICurrencyStrategy, GbpToZarStrategy>();
builder.Services.AddScoped<CurrencyStrategyResolver>();

//Factory Pattern: Contract creation with service level defaults
builder.Services.AddScoped<IContractFactory, ContractFactory>();

//Observer Pattern: status change notifications
builder.Services.AddSingleton<IContractObserver, AuditLogObserver>();
builder.Services.AddSingleton<IContractObserver, ExpiredContractGuardObserver>();
builder.Services.AddSingleton<IContractSubject, ContractStatusNotifier>();

// Validation and workflow services
builder.Services.AddScoped<IContractValidator, ContractValidator>();
builder.Services.AddScoped<IContractStatusService, ContractStatusService>();

// File handling service (PDF uploads, downloads, validation)
builder.Services.AddScoped<IFileService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new FileService(env.WebRootPath);
});

//External Currency API integration
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient(CurrencyExchangeService.HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://open.er-api.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "TechMove.GLMS/1.0");
});
// Disk-backed fallback so the cached rate survives app restart
builder.Services.AddSingleton<IFallbackRateStore>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new FileFallbackRateStore(env.ContentRootPath);
});
builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
