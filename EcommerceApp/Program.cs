using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

// Crear roles, productos y usuarios por defecto
try
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!db.Products.Any())
        {
            db.Products.AddRange(new List<Product>
            {
                new Product { Name = "NVIDIA GeForce RTX 4090", Description = "Tarjeta gráfica de gama alta con 24GB GDDR6X, ideal para 4K y ray tracing.", Price = 1899.99m, Stock = 12, Category = "GPU" },
                new Product { Name = "AMD Ryzen 9 7950X3D", Description = "Procesador de 16 núcleos con 3D V-Cache, líder en rendimiento gaming.", Price = 749.99m, Stock = 20, Category = "CPU" },
                new Product { Name = "Corsair Vengeance DDR5 32GB 6000MHz", Description = "Kit de memoria RAM DDR5 (2x16GB) de alta velocidad con perfil XMP 3.0.", Price = 189.99m, Stock = 35, Category = "RAM" },
                new Product { Name = "Samsung 990 PRO 2TB NVMe", Description = "SSD PCIe 4.0 con velocidades de lectura de hasta 7450 MB/s.", Price = 219.99m, Stock = 40, Category = "Almacenamiento" },
                new Product { Name = "ASUS ROG Strix B650E-F Gaming WiFi", Description = "Placa madre AM5 con WiFi 6E, PCIe 5.0 y soporte DDR5.", Price = 329.99m, Stock = 15, Category = "Placa Madre" },
                new Product { Name = "Corsair RM850x 850W 80+ Gold", Description = "Fuente de poder totalmente modular, certificación 80+ Gold.", Price = 149.99m, Stock = 25, Category = "Fuente" },
                new Product { Name = "NZXT Kraken Elite 360 RGB", Description = "Refrigeración líquida AIO de 360mm con pantalla LCD integrada.", Price = 289.99m, Stock = 18, Category = "Refrigeración" },
                new Product { Name = "Lian Li O11 Dynamic EVO", Description = "Gabinete mid-tower de vidrio templado con excelente flujo de aire.", Price = 179.99m, Stock = 22, Category = "Gabinete" },
                new Product { Name = "LG UltraGear 27GP950 4K 144Hz", Description = "Monitor gaming 27\" 4K UHD con 144Hz y Nvidia G-Sync.", Price = 799.99m, Stock = 10, Category = "Monitor" },
                new Product { Name = "Logitech G Pro X Superlight 2", Description = "Mouse inalámbrico ultraliviano de 60g, sensor HERO 32K.", Price = 159.99m, Stock = 50, Category = "Periférico" },
            });
            db.SaveChanges();
        }
    }

    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingUser = await userManager.FindByEmailAsync("juan@gmail.com");
        if (existingUser == null)
        {
            var newUser = new ApplicationUser
            {
                UserName = "juan@gmail.com",
                Email = "juan@gmail.com",
                FullName = "Juan",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(newUser, "123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, "Admin");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("============== ERROR DE BASE DE DATOS ==============");
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
    Console.WriteLine("====================================================");
}

app.Run();