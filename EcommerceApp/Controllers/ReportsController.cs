using EcommerceApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ============================================
        // VISTA PRINCIPAL DE REPORTES
        // ============================================
        public async Task<IActionResult> Index()
        {
            await CargarDatosReportes();
            return View();
        }

        // ============================================
        // GENERAR PDF
        // ============================================
        public async Task<IActionResult> DownloadPdf()
        {
            await CargarDatosReportes();
            var pdfBytes = GenerarPdfReporte();
            string nombreArchivo = $"Reporte-NexHard-{DateTime.Now:yyyyMMdd-HHmm}.pdf";
            return File(pdfBytes, "application/pdf", nombreArchivo);
        }

        // ============================================
        // CARGAR TODOS LOS DATOS
        // ============================================
        private async Task CargarDatosReportes()
        {
            var orders = await _context.Orders.Include(o => o.Details).ToListAsync();

            ViewBag.TotalVentas = orders.Sum(o => o.Total);
            ViewBag.TotalOrdenes = orders.Count;
            ViewBag.TicketPromedio = orders.Any() ? orders.Average(o => o.Total) : 0;
            ViewBag.TotalProductosVendidos = orders.SelectMany(o => o.Details).Sum(d => d.Quantity);

            var hoy = DateTime.UtcNow.Date;
            var ventasHoy = orders.Where(o => o.Fecha.Date == hoy).ToList();
            ViewBag.VentasHoy = ventasHoy.Sum(o => o.Total);
            ViewBag.OrdenesHoy = ventasHoy.Count;

            ViewBag.Pendientes = orders.Count(o => o.Estado == "Pendiente");
            ViewBag.Pagados = orders.Count(o => o.Estado == "Pagado");
            ViewBag.Enviados = orders.Count(o => o.Estado == "Enviado");
            ViewBag.Entregados = orders.Count(o => o.Estado == "Entregado");
            ViewBag.Cancelados = orders.Count(o => o.Estado == "Cancelado");

            var topProductos = orders
                .SelectMany(o => o.Details)
                .GroupBy(d => new { d.ProductId, d.ProductName })
                .Select(g => new
                {
                    Nombre = g.Key.ProductName,
                    CantidadVendida = g.Sum(d => d.Quantity),
                    IngresosTotales = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(10)
                .ToList();
            ViewBag.TopProductos = topProductos;

            var topClientes = orders
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    TotalCompras = g.Count(),
                    TotalGastado = g.Sum(o => o.Total),
                    Email = _context.Users.FirstOrDefault(u => u.Id == g.Key)!.Email ?? "N/A",
                    Nombre = _context.Users.FirstOrDefault(u => u.Id == g.Key)!.FullName ?? ""
                })
                .OrderByDescending(c => c.TotalGastado)
                .Take(5)
                .ToList();
            ViewBag.TopClientes = topClientes;

            var ventasPorCategoria = orders
                .SelectMany(o => o.Details)
                .Join(_context.Products, d => d.ProductId, p => p.Id,
                    (d, p) => new { p.Category, d.Quantity, d.Subtotal })
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Cantidad = g.Sum(x => x.Quantity),
                    Ingresos = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(c => c.Ingresos)
                .ToList();
            ViewBag.VentasPorCategoria = ventasPorCategoria;

            var hace30dias = DateTime.UtcNow.Date.AddDays(-29);
            var ventas30Dias = Enumerable.Range(0, 30)
                .Select(i => hace30dias.AddDays(i))
                .Select(fecha => new
                {
                    Fecha = fecha.ToString("dd/MM"),
                    Total = orders.Where(o => o.Fecha.Date == fecha).Sum(o => o.Total)
                })
                .ToList();
            ViewBag.Ventas30Dias = ventas30Dias;

            var metodosPago = orders
                .GroupBy(o => o.MetodoPago)
                .Select(g => new
                {
                    Metodo = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(o => o.Total)
                })
                .ToList();
            ViewBag.MetodosPago = metodosPago;
        }

        // ============================================
        // GENERAR PDF (SIN ARIAL + CON LOGO + ESTILOS)
        // ============================================
        private byte[] GenerarPdfReporte()
        {
            var topProductos = ViewBag.TopProductos as IEnumerable<dynamic>;
            var topClientes = ViewBag.TopClientes as IEnumerable<dynamic>;
            var ventasPorCategoria = ViewBag.VentasPorCategoria as IEnumerable<dynamic>;
            var metodosPago = ViewBag.MetodosPago as IEnumerable<dynamic>;

            // Cargar el logo desde wwwroot/images/logo.png
            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            byte[]? logoBytes = System.IO.File.Exists(logoPath)
                ? System.IO.File.ReadAllBytes(logoPath)
                : null;

            // Colores
            string colorPrimary = "#00e0ff";
            string colorSecondary = "#a020f0";
            string colorDark = "#1a1f2e";
            string colorGray = "#666666";
            string colorLightGray = "#f5f5f5";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    // NO usar Arial - dejar la fuente por defecto (Lato)
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // ========== ENCABEZADO ==========
                    page.Header().Column(col =>
                    {
                        col.Item().Background(colorDark).Padding(20).Row(row =>
                        {
                            // LOGO (a la izquierda)
                            if (logoBytes != null)
                            {
                                row.ConstantItem(90).AlignMiddle().Image(logoBytes).FitArea();
                            }

                            // TÍTULOS (al centro/derecha)
                            row.RelativeItem().PaddingLeft(15).AlignMiddle().Column(c =>
                            {
                                c.Item().Text("NexHard")
                                    .FontSize(24).Bold().FontColor(colorPrimary);
                                c.Item().Text("Reporte de Ventas y Estadísticas")
                                    .FontSize(11).FontColor("#ffffff");
                            });

                            // FECHA (a la derecha)
                            row.ConstantItem(160).AlignRight().AlignMiddle().Column(c =>
                            {
                                c.Item().Text($"Generado:").FontSize(8).FontColor("#aaaaaa");
                                c.Item().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontSize(10).Bold().FontColor("#ffffff");
                            });
                        });

                        col.Item().Background(colorPrimary).Height(4);
                    });

                    // ========== CONTENIDO ==========
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(18);

                        // ========== KPI CARDS ==========
                        col.Item().Text("RESUMEN GENERAL")
                            .FontSize(13).Bold().FontColor(colorDark);

                        col.Item().Row(row =>
                        {
                            // KPI 1
                            row.RelativeItem().Background(colorLightGray).Padding(12).Column(c =>
                            {
                                c.Item().Text("Ventas Totales")
                                    .FontSize(8).FontColor(colorGray);
                                c.Item().Text($"Bs {ViewBag.TotalVentas:N2}")
                                    .FontSize(14).Bold().FontColor(colorPrimary);
                            });

                            row.ConstantItem(8);

                            // KPI 2
                            row.RelativeItem().Background(colorLightGray).Padding(12).Column(c =>
                            {
                                c.Item().Text("Total Pedidos")
                                    .FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.TotalOrdenes}")
                                    .FontSize(14).Bold().FontColor(colorPrimary);
                            });

                            row.ConstantItem(8);

                            // KPI 3
                            row.RelativeItem().Background(colorLightGray).Padding(12).Column(c =>
                            {
                                c.Item().Text("Ticket Promedio")
                                    .FontSize(8).FontColor(colorGray);
                                c.Item().Text($"Bs {ViewBag.TicketPromedio:N2}")
                                    .FontSize(14).Bold().FontColor(colorPrimary);
                            });

                            row.ConstantItem(8);

                            // KPI 4
                            row.RelativeItem().Background(colorLightGray).Padding(12).Column(c =>
                            {
                                c.Item().Text("Productos Vendidos")
                                    .FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.TotalProductosVendidos}")
                                    .FontSize(14).Bold().FontColor(colorPrimary);
                            });
                        });

                        // ========== PEDIDOS POR ESTADO ==========
                        col.Item().PaddingTop(5).Text("PEDIDOS POR ESTADO")
                            .FontSize(13).Bold().FontColor(colorDark);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor("#ffc107").Background("#fff9e6").Padding(10).Column(c =>
                            {
                                c.Item().Text("Pendientes").FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.Pendientes}").FontSize(16).Bold().FontColor("#cc9900");
                            });
                            row.ConstantItem(6);
                            row.RelativeItem().Border(1).BorderColor("#00a0cc").Background("#e6f9ff").Padding(10).Column(c =>
                            {
                                c.Item().Text("Pagados").FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.Pagados}").FontSize(16).Bold().FontColor("#00a0cc");
                            });
                            row.ConstantItem(6);
                            row.RelativeItem().Border(1).BorderColor("#ff9800").Background("#fff3e0").Padding(10).Column(c =>
                            {
                                c.Item().Text("Enviados").FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.Enviados}").FontSize(16).Bold().FontColor("#ff9800");
                            });
                            row.ConstantItem(6);
                            row.RelativeItem().Border(1).BorderColor("#00a050").Background("#e6ffee").Padding(10).Column(c =>
                            {
                                c.Item().Text("Entregados").FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.Entregados}").FontSize(16).Bold().FontColor("#00a050");
                            });
                            row.ConstantItem(6);
                            row.RelativeItem().Border(1).BorderColor("#ff4444").Background("#ffe6e6").Padding(10).Column(c =>
                            {
                                c.Item().Text("Cancelados").FontSize(8).FontColor(colorGray);
                                c.Item().Text($"{ViewBag.Cancelados}").FontSize(16).Bold().FontColor("#ff4444");
                            });
                        });

                        // ========== TOP PRODUCTOS ==========
                        col.Item().PaddingTop(5).Text("TOP 10 PRODUCTOS MÁS VENDIDOS")
                            .FontSize(13).Bold().FontColor(colorDark);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(colorPrimary).Padding(7).Text("#").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorPrimary).Padding(7).Text("Producto").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorPrimary).Padding(7).Text("Cantidad").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorPrimary).Padding(7).Text("Ingresos").Bold().FontColor("#ffffff");
                            });

                            if (topProductos != null)
                            {
                                int rank = 1;
                                foreach (var prod in topProductos)
                                {
                                    var bg = rank % 2 == 0 ? "#ffffff" : colorLightGray;
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{rank}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{prod.Nombre}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{prod.CantidadVendida}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"Bs {prod.IngresosTotales:N2}");
                                    rank++;
                                }
                            }
                        });

                        // ========== TOP CLIENTES ==========
                        col.Item().PaddingTop(8).Text("TOP 5 MEJORES CLIENTES")
                            .FontSize(13).Bold().FontColor(colorDark);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(colorSecondary).Padding(7).Text("#").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorSecondary).Padding(7).Text("Cliente").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorSecondary).Padding(7).Text("Compras").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorSecondary).Padding(7).Text("Total Gastado").Bold().FontColor("#ffffff");
                            });

                            if (topClientes != null)
                            {
                                int rank = 1;
                                foreach (var cli in topClientes)
                                {
                                    var bg = rank % 2 == 0 ? "#ffffff" : colorLightGray;
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{rank}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{cli.Email}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{cli.TotalCompras}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"Bs {cli.TotalGastado:N2}");
                                    rank++;
                                }
                            }
                        });

                        // ========== VENTAS POR CATEGORÍA ==========
                        col.Item().PaddingTop(8).Text("VENTAS POR CATEGORÍA")
                            .FontSize(13).Bold().FontColor(colorDark);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(colorDark).Padding(7).Text("Categoría").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorDark).Padding(7).Text("Cantidad").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorDark).Padding(7).Text("Ingresos").Bold().FontColor("#ffffff");
                            });

                            if (ventasPorCategoria != null)
                            {
                                int rank = 1;
                                foreach (var cat in ventasPorCategoria)
                                {
                                    var bg = rank % 2 == 0 ? "#ffffff" : colorLightGray;
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{cat.Categoria}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{cat.Cantidad}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"Bs {cat.Ingresos:N2}");
                                    rank++;
                                }
                            }
                        });

                        // ========== MÉTODOS DE PAGO ==========
                        col.Item().PaddingTop(8).Text("MÉTODOS DE PAGO")
                            .FontSize(13).Bold().FontColor(colorDark);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(colorDark).Padding(7).Text("Método").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorDark).Padding(7).Text("Cantidad").Bold().FontColor("#ffffff");
                                header.Cell().Background(colorDark).Padding(7).Text("Total").Bold().FontColor("#ffffff");
                            });

                            if (metodosPago != null)
                            {
                                int rank = 1;
                                foreach (var met in metodosPago)
                                {
                                    var bg = rank % 2 == 0 ? "#ffffff" : colorLightGray;
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{met.Metodo}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"{met.Cantidad}");
                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#e0e0e0").Padding(7).Text($"Bs {met.Total:N2}");
                                    rank++;
                                }
                            }
                        });
                    });

                    // ========== PIE DE PÁGINA ==========
                    page.Footer().Column(col =>
                    {
                        col.Item().Background(colorPrimary).Height(3);
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text("NexHard - Sistema de Gestión de Componentes de PC")
                                .FontSize(8).FontColor(colorGray);

                            row.ConstantItem(150).AlignRight().Text(text =>
                            {
                                text.Span("Página ").FontSize(8).FontColor(colorGray);
                                text.CurrentPageNumber().FontSize(8).Bold();
                                text.Span(" de ").FontSize(8).FontColor(colorGray);
                                text.TotalPages().FontSize(8).Bold();
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}