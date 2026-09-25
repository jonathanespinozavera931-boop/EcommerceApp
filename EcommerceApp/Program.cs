using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.HttpOverrides;

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
// Configuración de sesión para el carrito de compras
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ══════════════ FORWARDED HEADERS (para Render proxy) ══════════════
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false,
    KnownNetworks = { },
    KnownProxies = { }
});

// ══════════════ ANTI-CACHÉ PARA DESARROLLO Y PRODUCCIÓN ══════════════
app.Use(async (context, next) =>
{
    // No cachear HTML dinámico
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();        

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
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
        // ==================== CPUs (8) ====================
        new Product { Name = "AMD Ryzen 9 7950X3D", Description = "16 núcleos · 32 hilos · 5.7 GHz · 3D V-Cache", Price = 749.99m, Stock = 20, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Ryzen+9" },
        new Product { Name = "AMD Ryzen 7 7800X3D", Description = "8 núcleos · 16 hilos · 5.0 GHz · 3D V-Cache", Price = 449.99m, Stock = 25, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Ryzen+7" },
        new Product { Name = "AMD Ryzen 5 7600X", Description = "6 núcleos · 12 hilos · 5.3 GHz", Price = 249.99m, Stock = 30, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Ryzen+5" },
        new Product { Name = "AMD Ryzen 5 5600", Description = "6 núcleos · 12 hilos · 4.4 GHz · AM4", Price = 129.99m, Stock = 40, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Ryzen+5600" },
        new Product { Name = "Intel Core i9-14900K", Description = "24 núcleos · 32 hilos · 6.0 GHz Turbo", Price = 599.99m, Stock = 15, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=i9-14900K" },
        new Product { Name = "Intel Core i7-14700K", Description = "20 núcleos · 28 hilos · 5.6 GHz Turbo", Price = 419.99m, Stock = 18, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=i7-14700K" },
        new Product { Name = "Intel Core i5-14600K", Description = "14 núcleos · 20 hilos · 5.3 GHz Turbo", Price = 319.99m, Stock = 22, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=i5-14600K" },
        new Product { Name = "Intel Core i3-13100F", Description = "4 núcleos · 8 hilos · 4.5 GHz Turbo", Price = 109.99m, Stock = 35, Category = "CPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=i3-13100F" },

        // ==================== GPUs (10) ====================
        new Product { Name = "NVIDIA GeForce RTX 4090", Description = "24GB GDDR6X · Ray Tracing · 4K Ultra", Price = 1899.99m, Stock = 12, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+4090" },
        new Product { Name = "NVIDIA GeForce RTX 4080 Super", Description = "16GB GDDR6X · Ray Tracing · 4K", Price = 1099.99m, Stock = 15, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+4080" },
        new Product { Name = "NVIDIA GeForce RTX 4070 Ti Super", Description = "16GB GDDR6X · Ray Tracing · 1440p+", Price = 799.99m, Stock = 20, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+4070Ti" },
        new Product { Name = "NVIDIA GeForce RTX 4070 Super", Description = "12GB GDDR6X · Ray Tracing · 1440p", Price = 599.99m, Stock = 25, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+4070" },
        new Product { Name = "NVIDIA GeForce RTX 4060 Ti", Description = "8GB GDDR6 · Ray Tracing · 1080p+", Price = 399.99m, Stock = 30, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+4060Ti" },
        new Product { Name = "NVIDIA GeForce RTX 4060", Description = "8GB GDDR6 · Ray Tracing · 1080p", Price = 299.99m, Stock = 35, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+4060" },
        new Product { Name = "AMD Radeon RX 7900 XTX", Description = "24GB GDDR6 · RDNA 3 · 4K", Price = 999.99m, Stock = 18, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RX+7900XTX" },
        new Product { Name = "AMD Radeon RX 7800 XT", Description = "16GB GDDR6 · RDNA 3 · 1440p", Price = 499.99m, Stock = 22, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RX+7800XT" },
        new Product { Name = "AMD Radeon RX 7600", Description = "8GB GDDR6 · RDNA 3 · 1080p", Price = 269.99m, Stock = 28, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RX+7600" },
        new Product { Name = "NVIDIA GeForce RTX 3050", Description = "8GB GDDR6 · Ray Tracing · 1080p", Price = 199.99m, Stock = 40, Category = "GPU", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=RTX+3050" },

        // ==================== RAM (6) ====================
        new Product { Name = "Corsair Vengeance DDR5 32GB 6000MHz", Description = "2x16GB · RGB · XMP 3.0", Price = 189.99m, Stock = 35, Category = "RAM", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Corsair+32GB" },
        new Product { Name = "G.Skill Trident Z5 RGB 32GB 6400MHz", Description = "2x16GB · RGB · DDR5 Ultra", Price = 219.99m, Stock = 25, Category = "RAM", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=G.Skill+32GB" },
        new Product { Name = "Kingston Fury Beast DDR5 16GB 5200MHz", Description = "2x8GB · DDR5 · Plug and Play", Price = 89.99m, Stock = 40, Category = "RAM", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Kingston+16GB" },
        new Product { Name = "Corsair Vengeance LPX DDR4 16GB 3200MHz", Description = "2x8GB · DDR4 · Low Profile", Price = 59.99m, Stock = 50, Category = "RAM", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Vengeance+16GB" },
        new Product { Name = "G.Skill Ripjaws V 32GB 3600MHz", Description = "2x16GB · DDR4 · Rendimiento", Price = 109.99m, Stock = 30, Category = "RAM", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Ripjaws+32GB" },
        new Product { Name = "TeamGroup T-Force Delta RGB 16GB", Description = "2x8GB · DDR5 · RGB 120°", Price = 79.99m, Stock = 45, Category = "RAM", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Delta+RGB" },

        // ==================== Almacenamiento (8) ====================
        new Product { Name = "Samsung 990 PRO 2TB NVMe", Description = "PCIe 4.0 · 7450 MB/s · M.2", Price = 219.99m, Stock = 40, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=990+PRO+2TB" },
        new Product { Name = "Samsung 980 PRO 1TB NVMe", Description = "PCIe 4.0 · 7000 MB/s · M.2", Price = 129.99m, Stock = 45, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=980+PRO+1TB" },
        new Product { Name = "WD Black SN850X 2TB", Description = "PCIe 4.0 · 7300 MB/s · Gaming", Price = 199.99m, Stock = 35, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=SN850X+2TB" },
        new Product { Name = "Crucial P3 Plus 1TB", Description = "PCIe 4.0 · 5000 MB/s · M.2", Price = 89.99m, Stock = 50, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=P3+Plus+1TB" },
        new Product { Name = "Kingston NV2 500GB", Description = "PCIe 4.0 · 3500 MB/s · M.2", Price = 49.99m, Stock = 60, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=NV2+500GB" },
        new Product { Name = "Seagate Barracuda 2TB HDD", Description = "7200 RPM · 256MB Caché · SATA", Price = 59.99m, Stock = 30, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Barracuda+2TB" },
        new Product { Name = "WD Blue 4TB HDD", Description = "5400 RPM · 256MB Caché · SATA", Price = 99.99m, Stock = 25, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=WD+Blue+4TB" },
        new Product { Name = "Samsung T7 Portable 1TB", Description = "SSD Externo · USB 3.2 · 1050 MB/s", Price = 149.99m, Stock = 30, Category = "Almacenamiento", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=T7+1TB" },

        // ==================== Placas Madre (8) ====================
        new Product { Name = "ASUS ROG Strix B650E-F Gaming WiFi", Description = "AM5 · WiFi 6E · PCIe 5.0 · DDR5", Price = 329.99m, Stock = 15, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=ROG+B650E-F" },
        new Product { Name = "MSI MAG B650 Tomahawk WiFi", Description = "AM5 · WiFi 6E · DDR5 · PCIe 4.0", Price = 219.99m, Stock = 20, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=MSI+B650" },
        new Product { Name = "Gigabyte B650 AORUS Elite AX", Description = "AM5 · WiFi 6E · DDR5 · RGB", Price = 249.99m, Stock = 18, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=AORUS+B650" },
        new Product { Name = "ASRock B650M Pro RS", Description = "AM5 · Micro-ATX · DDR5", Price = 149.99m, Stock = 25, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=B650M+Pro+RS" },
        new Product { Name = "ASUS ROG Maximus Z790 Hero", Description = "LGA1700 · WiFi 6E · DDR5 · Premium", Price = 599.99m, Stock = 8, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Z790+Hero" },
        new Product { Name = "MSI MPG Z790 Edge WiFi", Description = "LGA1700 · WiFi 6E · DDR5 · ATX", Price = 349.99m, Stock = 12, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Z790+Edge" },
        new Product { Name = "Gigabyte B760M DS3H", Description = "LGA1700 · Micro-ATX · DDR4", Price = 129.99m, Stock = 30, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=B760M" },
        new Product { Name = "ASUS Prime B550M-A", Description = "AM4 · Micro-ATX · DDR4", Price = 99.99m, Stock = 35, Category = "Placa Madre", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=B550M-A" },

        // ==================== Fuentes (6) ====================
        new Product { Name = "Corsair RM850x 850W 80+ Gold", Description = "Modular · Gold · Silent", Price = 149.99m, Stock = 25, Category = "Fuente", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=RM850x" },
        new Product { Name = "Seasonic Focus GX-750 750W", Description = "Modular · Gold · Garantía 10 años", Price = 129.99m, Stock = 30, Category = "Fuente", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Focus+GX-750" },
        new Product { Name = "EVGA SuperNOVA 1000 G6", Description = "1000W · Modular · Gold · Premium", Price = 199.99m, Stock = 15, Category = "Fuente", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=EVGA+1000G6" },
        new Product { Name = "Cooler Master MWE 650 Bronze", Description = "650W · Bronze · Semi-modular", Price = 69.99m, Stock = 40, Category = "Fuente", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=MWE+650" },
        new Product { Name = "be quiet! Straight Power 11 850W", Description = "850W · Modular · Gold · Silent Wings", Price = 169.99m, Stock = 20, Category = "Fuente", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Straight+Power" },
        new Product { Name = "Thermaltake Toughpower GF1 750W", Description = "750W · Modular · Gold · RGB", Price = 109.99m, Stock = 28, Category = "Fuente", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Toughpower" },

        // ==================== Refrigeración (6) ====================
        new Product { Name = "NZXT Kraken Elite 360 RGB", Description = "AIO · 360mm · LCD · RGB", Price = 289.99m, Stock = 18, Category = "Refrigeración", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Kraken+360" },
        new Product { Name = "Corsair iCUE H150i Elite", Description = "AIO · 360mm · LCD · RGB", Price = 229.99m, Stock = 22, Category = "Refrigeración", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=H150i+Elite" },
        new Product { Name = "Lian Li Galahad II 360", Description = "AIO · 360mm · RGB · Alta performance", Price = 179.99m, Stock = 25, Category = "Refrigeración", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Galahad+II" },
        new Product { Name = "Arctic Liquid Freezer II 240", Description = "AIO · 240mm · Silent · Eficiente", Price = 109.99m, Stock = 30, Category = "Refrigeración", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Arctic+240" },
        new Product { Name = "Noctua NH-D15", Description = "Aire · Dual Tower · Silencioso", Price = 109.99m, Stock = 28, Category = "Refrigeración", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=NH-D15" },
        new Product { Name = "Cooler Master Hyper 212", Description = "Aire · 120mm · RGB · Económico", Price = 39.99m, Stock = 50, Category = "Refrigeración", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Hyper+212" },

        // ==================== Gabinetes (6) ====================
        new Product { Name = "Lian Li O11 Dynamic EVO", Description = "Mid-Tower · Vidrio templado · Premium", Price = 179.99m, Stock = 22, Category = "Gabinete", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=O11+Dynamic" },
        new Product { Name = "NZXT H7 Flow", Description = "Mid-Tower · Airflow · Mesh frontal", Price = 129.99m, Stock = 28, Category = "Gabinete", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=NZXT+H7" },
        new Product { Name = "Corsair 4000D Airflow", Description = "Mid-Tower · Airflow · Cristal templado", Price = 99.99m, Stock = 35, Category = "Gabinete", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=4000D" },
        new Product { Name = "Fractal Design Meshify 2", Description = "Mid-Tower · Mesh · Modular", Price = 159.99m, Stock = 20, Category = "Gabinete", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=Meshify+2" },
        new Product { Name = "Cooler Master MasterBox TD500", Description = "Mid-Tower · Mesh · RGB", Price = 119.99m, Stock = 25, Category = "Gabinete", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=TD500" },
        new Product { Name = "Phanteks Eclipse P400A", Description = "Mid-Tower · Mesh · Airflow", Price = 89.99m, Stock = 30, Category = "Gabinete", ImageUrl = "https://placehold.co/600x600/1a1f2e/00F0FF?text=P400A" },

        // ==================== Periféricos (12) ====================
        new Product { Name = "Logitech G Pro X Superlight 2", Description = "Mouse inalámbrico · 60g · HERO 32K", Price = 159.99m, Stock = 50, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=G+Pro+X" },
        new Product { Name = "Razer DeathAdder V3 Pro", Description = "Mouse inalámbrico · 63g · Focus Pro 30K", Price = 149.99m, Stock = 45, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=DeathAdder" },
        new Product { Name = "Logitech G502 X Plus", Description = "Mouse inalámbrico · RGB · LIGHTSPEED", Price = 129.99m, Stock = 40, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=G502+X" },
        new Product { Name = "Razer BlackWidow V4 Pro", Description = "Teclado mecánico · RGB · Verde Razer", Price = 229.99m, Stock = 25, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=BlackWidow" },
        new Product { Name = "Corsair K70 RGB Pro", Description = "Teclado mecánico · RGB · Cherry MX", Price = 169.99m, Stock = 30, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=K70+RGB" },
        new Product { Name = "HyperX Alloy Origins", Description = "Teclado mecánico · RGB · Compact", Price = 109.99m, Stock = 40, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Alloy+Origins" },
        new Product { Name = "SteelSeries Arctis Nova Pro", Description = "Headset premium · ANC · Dual Wireless", Price = 349.99m, Stock = 15, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Arctis+Nova" },
        new Product { Name = "HyperX Cloud II", Description = "Headset · 7.1 Surround · Memory Foam", Price = 99.99m, Stock = 45, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Cloud+II" },
        new Product { Name = "Razer Kraken V3", Description = "Headset · THX Spatial · RGB", Price = 79.99m, Stock = 50, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Kraken+V3" },
        new Product { Name = "Logitech G Pro X Headset", Description = "Headset · Blue VO!CE · Pro-G", Price = 129.99m, Stock = 35, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=G+Pro+X+HS" },
        new Product { Name = "Corsair MM300 Mousepad XL", Description = "Mousepad extendido · Tela · Anti-deslizante", Price = 29.99m, Stock = 60, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=MM300" },
        new Product { Name = "Razer Goliathus Extended", Description = "Mousepad extendido · RGB Chroma", Price = 39.99m, Stock = 55, Category = "Periférico", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Goliathus" },

        // ==================== Monitores (6) ====================
        new Product { Name = "LG UltraGear 27GP950 4K", Description = "27\" · 4K UHD · 144Hz · IPS · G-Sync", Price = 799.99m, Stock = 10, Category = "Monitor", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=LG+4K+144Hz" },
        new Product { Name = "Samsung Odyssey G9 49\"", Description = "49\" · DQHD · 240Hz · Curvo · QLED", Price = 1299.99m, Stock = 5, Category = "Monitor", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Odyssey+G9" },
        new Product { Name = "ASUS ROG Swift PG279QM", Description = "27\" · QHD · 240Hz · IPS · G-Sync", Price = 749.99m, Stock = 8, Category = "Monitor", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=ROG+PG279QM" },
        new Product { Name = "MSI Optix MAG274QRF", Description = "27\" · QHD · 165Hz · IPS · HDR", Price = 399.99m, Stock = 15, Category = "Monitor", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=MSI+MAG274" },
        new Product { Name = "Dell S2721DGF 27\"", Description = "27\" · QHD · 165Hz · IPS · FreeSync", Price = 379.99m, Stock = 18, Category = "Monitor", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=Dell+S2721" },
        new Product { Name = "AOC 24G2 24\"", Description = "24\" · FHD · 144Hz · IPS · FreeSync", Price = 199.99m, Stock = 25, Category = "Monitor", ImageUrl = "https://placehold.co/600x600/1a1f2e/FF003C?text=AOC+24G2" },
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
                FullName = "Juan (Admin)",
                EmailConfirmed = true
            };
            // CONTRASEÑA DEL ADMIN
            var result = await userManager.CreateAsync(newUser, "875353515");
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