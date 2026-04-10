using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Projections;
using IronPdf;
using Microsoft.AspNetCore.Routing.Tree;
using MongoDB.Bson;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace FrameworkDriver_Api.src.Services
{
    public class RegisterService
    {
        private readonly IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection> _registerRepository;
        private readonly ILoadAllId<ObservationModel> _observation;
        private readonly IAddFilter<CompanyModel, CompanyModel> _company;
        private readonly QrInterface _qrService;
        private readonly IUpdateQr _qr;
        public RegisterService(
            IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection> registerRepository,
            QrInterface qrService,
            IUpdateQr qr,
            ILoadAllId<ObservationModel> observation,
            IAddFilter<CompanyModel, CompanyModel> company
            )
        {
            _registerRepository = registerRepository;
            _qrService = qrService;
            _qr = qr;
            _observation = observation;
            _company = company;
        }

        public async Task<string> AddRegisterAsync(RegisterDTO register)
        {
            var nextRegistroNumber = await _registerRepository.GetNextRegistroNumberAsync();
            var result = await _registerRepository.CreateAsync(new RegisterModel
            {
                IdClient = register.IdClient,
                IdCompany = register.IdCompany,
                IdUser = register.IdUser,
                StatusRegister = register.StatusRegister,
                RegistroNumber = nextRegistroNumber,
                CreatedAt = DateTime.UtcNow, // Guardar en UTC
            });

            var BaseUrl = $"{register.UrlRuta}/{result}";

            string Url = string.Empty; string Id = string.Empty;

            if (register.UrlRuta != string.Empty) (Url, Id) = await _qrService.GenerateQr(BaseUrl);

            var qrResponse = await UpdateQr(Url, Id, result);
            if (qrResponse == false) throw new Exception("No se pudo crear el codigo qr");
            return result;

        }

        public async Task<bool> UpdateQr(string url, string id, string idInsert)
        {
            return await _qr.UpdateQr(url, id, idInsert);
        }

        // hace un filtro del cliente y sale una lista de observaciones 
        public async Task<IEnumerable<ListRegistersProjection>> Filter(string filter)
        {
            return await _registerRepository.FilterData(filter);
        }

        public async Task<IEnumerable<RegisterObsCliProjection>> GetAllRegistersAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            return await _registerRepository.GetAllAsync(pageNumber, pageSize, idCompany);
        }

        public async Task<RegisterModel> GetRegisterByIdAsync(string id)
        {
            return await _registerRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateRegisterAsync(string id, RegisterModel register)
        {
            return await _registerRepository.UpdateAsync(id, register);
        }

        public async Task<bool> DeleteRegisterAsync(string id)
        {
            return await _registerRepository.DeleteAsync(id);
        }

        public async Task<byte[]> GeneratePDFAsync(string id, int page, int size)
        {
            // 0. Configurar Licencia (Puede ir en Program.cs también)
            QuestPDF.Settings.License = LicenseType.Community;

            // 1. Obtener datos de la base de datos
            var register = await _registerRepository.GetByIdAsync(id);
            var observations = await _observation.GetAllIdAsync(id, page, size);
            var companyData = await _company.GetByIdAsync(register.IdCompany);

            using var httpClient = new HttpClient();

            // 2. Pre-descargar el Logo de la Empresa
            byte[]? logoBytes = null;
            if (!string.IsNullOrEmpty(companyData.LogoUrl))
            {
                try { logoBytes = await httpClient.GetByteArrayAsync(companyData.LogoUrl); }
                catch { /* Log error o dejar null si falla */ }
            }

            // 3. Pre-descargar las fotos de las observaciones
            // Creamos un diccionario para asociar la URL de Cloudinary con sus bytes
            var photoCache = new Dictionary<string, byte[]?>();
            foreach (var obs in observations)
            {
                if (obs.Photos != null)
                {
                    foreach (var p in obs.Photos)
                    {
                        if (!photoCache.ContainsKey(p.Photo))
                        {
                            try
                            {
                                var b = await httpClient.GetByteArrayAsync(p.Photo);
                                photoCache[p.Photo] = b;
                            }
                            catch { photoCache[p.Photo] = null; }
                        }
                    }
                }
            }

            // 4. Definir estilos reutilizables
            Func<IContainer, IContainer> headerStyle = container =>
                container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);

            Func<IContainer, IContainer> rowStyle = container =>
                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);

            // 5. Generar el documento
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    // ENCABEZADO
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(companyData.Name).FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"NIT: {companyData.NIT}").FontSize(9);
                            col.Item().Text(companyData.Address).FontSize(9);
                            col.Item().Text($"{companyData.Email} | {companyData.Phone}").FontSize(9);
                        });

                        if (logoBytes != null)
                        {
                            row.ConstantItem(80).AlignRight().Image(logoBytes);
                        }
                    });

                    // CONTENIDO
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().PaddingTop(10).PaddingBottom(5).Text("Información del Registro").FontSize(14).SemiBold().Underline();

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text(t => { t.Span("Estado: ").SemiBold(); t.Span(register.StatusRegister.ToString() ?? "N/A"); });
                                column.Item().Text(t => { t.Span("N° Registro: ").SemiBold(); t.Span(register.RegistroNumber); });
                            });
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text(t => { t.Span("Fecha: ").SemiBold(); t.Span(register.CreatedAt.ToString("dd/MM/yyyy")); });
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingBottom(5).Text("Observaciones Detalladas").FontSize(14).SemiBold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(90);  // Fecha
                                columns.RelativeColumn(2);   // Descripción
                                columns.RelativeColumn(1);   // Fotos
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(headerStyle).Text("Fecha");
                                header.Cell().Element(headerStyle).Text("Descripción");
                                header.Cell().Element(headerStyle).Text("Evidencia");
                            });

                            foreach (var obs in observations)
                            {
                                table.Cell().Element(rowStyle).Text(obs.CreatedAt.ToString("yyyy-MM-dd"));
                                table.Cell().Element(rowStyle).Text(obs.Description ?? "Sin descripción");

                                // Celda de Fotos
                                table.Cell().Element(rowStyle).Column(colFotos =>
                                {
                                    if (obs.Photos != null && obs.Photos.Any())
                                    {
                                        foreach (var p in obs.Photos)
                                        {
                                            if (photoCache.TryGetValue(p.Photo, out byte[]? b) && b != null)
                                            {
                                                colFotos.Item().PaddingBottom(5).MaxWidth(60).Hyperlink(p.Photo).Column(c =>
                                                {
                                                    c.Item().Image(b);
                                        
                                                    // Opcional: Agregar un texto pequeño que diga "Ver original" 
                                                    // para que el usuario sepa que es clicleable
                                                    c.Item().AlignCenter().Text("Ver original")
                                                            .FontSize(7)
                                                            .FontColor(Colors.Blue.Medium)
                                                            .Underline();
                                                });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        colFotos.Item().Text("Sin fotos").Italic().FontSize(8);
                                    }
                                });
                            }
                        });
                    });

                    // PIE DE PÁGINA
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
        private async Task<byte[]?> DownloadImageAsync(string imageUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetByteArrayAsync(imageUrl);
            }
            catch
            {
                // Si la imagen falla, puedes retornar null o una imagen por defecto
                return null;
            }
        }
    }
}