using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PopcartSeller.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Add session services
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.IsEssential = true; 
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
