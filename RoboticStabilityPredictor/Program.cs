using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoboticStabilityPredictor.Data;
using RoboticStabilityPredictor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add stability calculation service registration
builder.Services.AddScoped<StabilityCalculationService>();

// Add Persistent Excel service registration
builder.Services.AddSingleton<PersistentExcelService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Add after app creation
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Automatically apply migrations on startup
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    await RoleInitializer.SeedRolesAsync(services);
    
    // Initialize PersistentExcelService to create Excel file with seeded data
    var excelService = app.Services.GetRequiredService<PersistentExcelService>();
    Console.WriteLine($"Excel file initialized at: {excelService.GetExcelFilePath()}");
}

app.Run();