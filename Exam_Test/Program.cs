using Exam_Test.Data;
using Exam_Test.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity + Roles
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>() // ✅ Required for RoleManager
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ VERY IMPORTANT
app.UseAuthentication();   // 🔥 MUST COME BEFORE AUTHORIZATION
app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ✅ Seed Roles + Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await RoleSeeder.SeedRoles(services);
    await AdminSeeder.SeedAdmin(services);

    // Seed Modules
    var context = services.GetRequiredService<ApplicationDbContext>();

    if (!context.Modules.Any())
    {
        context.Modules.AddRange(
            new Exam_Test.Models.Module { Name = "Module 1" },
            new Exam_Test.Models.Module { Name = "Module 2" },
            new Exam_Test.Models.Module { Name = "Module 3" }
        );
        await context.SaveChangesAsync();
    }
}

app.Run();