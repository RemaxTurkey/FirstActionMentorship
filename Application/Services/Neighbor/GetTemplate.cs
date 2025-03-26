using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;
using System.Net.Http;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Application.Services.Neighbor
{
    public class GetTemplate : BaseSvc<GetTemplate.Request, byte[]>
    {
        public record Request(int Id, int EmployeeId, int TypeId);

        public GetTemplate(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<byte[]> _InvokeAsync(GenericUoW uow, Request req)
        {
            string templateFileName = $"template_{req.Id}.html";

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateFileName);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"{templateFileName} dosyası bulunamadı.");
            }

            var employee = await uow.Repository<Data.Entities.dbo.Employee>()
            .FindByNoTracking(x => x.Id == req.EmployeeId)
            .Select(x => new Data.Entities.dbo.Employee()
            {
                Id = x.Id,
                NameSurname = x.NameSurname,
                BirthDate = x.BirthDate,
                BirthPlace = x.BirthPlace,
                Education = x.Education,
                MobileCodeWork = x.MobileCodeWork,
                MobileNoWork = x.MobileNoWork,
                Email = x.Email
            }).FirstOrDefaultAsync();

            if (employee == null)
            {
                throw new Exception("Employee not found");
            }

            var employeeAttribute = await uow.Repository<EmployeeAttribute>()
            .FindByNoTracking(x => x.Id == employee.EducationalId
                && x.AttributeType == EmployeeAttributeType.Educational)
            .FirstOrDefaultAsync();

            var template = await File.ReadAllTextAsync(templatePath);

            template = template.Replace("#Fullname#", employee.NameSurname);
            template = template.Replace("#BirthDate#", employee.BirthDate?.Year.ToString() ?? string.Empty);
            template = template.Replace("#BornCity#", employee.BirthPlace ?? string.Empty);
            template = template.Replace("#University#", employee.Education?.ToString() ?? string.Empty);
            template = template.Replace("#Department#", employeeAttribute?.Title?.ToString() ?? string.Empty);
            template = template.Replace("#Phone#", $"{employee.MobileCodeWork}{employee.MobileNoWork}");
            template = template.Replace("#Email#", employee.Email ?? string.Empty);

            byte[] result = null;

            if (req.TypeId == 1)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                    {
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());

                        // HTML içeriğini satır satır ekleyelim
                        Paragraph paragraph = body.AppendChild(new Paragraph());
                        Run run = paragraph.AppendChild(new Run());
                        run.AppendChild(new Text("Başlık: "));

                        Paragraph htmlParagraph = body.AppendChild(new Paragraph());
                        Run htmlRun = htmlParagraph.AppendChild(new Run());
                        htmlRun.AppendChild(new Text(template));

                        mainPart.Document.Save();
                    }

                    memoryStream.Position = 0;
                    result = memoryStream.ToArray();
                }
            }
            else
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip
                };

                using var client = new HttpClient(handler);
                client.BaseAddress = new Uri("http://172.16.81.50/api/home");

                var response = await client.PostAsync("",
                new StringContent("html=" + System.Web.HttpUtility.UrlEncode(template), Encoding.UTF8,
                        "application/x-www-form-urlencoded"));

                if (response.IsSuccessStatusCode)
                    result = await response.Content.ReadAsByteArrayAsync();
            }

            return result;
        }
    }
}