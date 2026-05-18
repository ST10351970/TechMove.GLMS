using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;
using TechMove.GLMS.Core.Services.Strategies;
using TechMove.GLMS.Core.Services.Factories;
using TechMove.GLMS.Core.Services.Observers;
using TechMove.GLMS.Core.Services;

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
