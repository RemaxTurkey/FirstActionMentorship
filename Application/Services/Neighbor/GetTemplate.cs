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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

            // Doğum yeri için ilk harfi büyük ve doğru -da/-de eki ile formatla
            string formattedBirthPlace = FormatCityWithLocativeCase(employee.BirthPlace ?? string.Empty);

            // HTML içeriğinde değişiklikler yap
            template = template.Replace("#Fullname#", employee.NameSurname);
            template = template.Replace("#BirthDate#", employee.BirthDate?.Year.ToString() ?? string.Empty);
            
            // Sadece şehir ismini, -da/-de eksiz olarak büyük harfle başlatarak ekle
            string birthPlaceOnly = string.IsNullOrEmpty(employee.BirthPlace) 
                ? string.Empty 
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.BirthPlace.ToLower());
            template = template.Replace("#BornCity#", birthPlaceOnly);
            
            template = template.Replace("#University#", employee.Education?.ToString() ?? string.Empty);
            template = template.Replace("#Department#", employeeAttribute?.Title?.ToString() ?? string.Empty);
            template = template.Replace("#Phone#", $"{employee.MobileCodeWork}{employee.MobileNoWork}");
            template = template.Replace("#Email#", employee.Email ?? string.Empty);

            byte[] result = null;

            if (req.TypeId == 1)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                    {
                        // Ana belge bölümü ekle
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.Body = new Body();
                        
                        // Sayfa formatı oluştur
                        SectionProperties sectionProps = new SectionProperties();
                        sectionProps.AppendChild(new PageSize() { Width = 12240, Height = 15840 }); // A4 boyutu
                        sectionProps.AppendChild(new PageMargin() { Top = 1440, Right = 1440, Bottom = 1440, Left = 1440 }); // 1 inç kenar boşlukları
                        body.AppendChild(sectionProps);

                        // Stil tanımları ekle
                        SetDocumentDefaults(mainPart);

                        // Boşluk ekle (üstten)
                        body.AppendChild(new Paragraph());

                        // Selamlama
                        body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { After = "120" }
                            ),
                            new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text("Merhaba Komşum,")
                            )
                        ));

                        // Giriş paragrafı
                        Paragraph introPara = new Paragraph();
                        introPara.AppendChild(new ParagraphProperties(
                            new SpacingBetweenLines() { After = "0" }
                        ));

                        // İlk kısım
                        Run introRun = new Run(
                            new RunProperties(new FontSize() { Val = "24" }),
                            new Text($"Ben {employee.NameSurname}. Artık aynı mahallede/sitede yaşayan bir komşunuz ve profesyonel bir ")
                        );
                        introPara.AppendChild(introRun);
                        
                        // RE/MAX vurgusu (kalın)
                        Run remaxRun = new Run(
                            new RunProperties(
                                new Bold(),
                                new FontSize() { Val = "24" }
                            ),
                            new Text("RE/MAX Gayrimenkul Danışmanı")
                        );
                        introPara.AppendChild(remaxRun);
                        
                        // Kalan metin
                        Run remainingRun = new Run(
                            new RunProperties(new FontSize() { Val = "24" }),
                            new Text(" olarak hizmet veriyorum. Sizlere en hızlı ve en yakın şekilde gayrimenkul hizmetleri sunmaya hazırım.")
                        );
                        introPara.AppendChild(remainingRun);
                        body.AppendChild(introPara);

                        // Kişisel bilgiler paragrafı
                        string birthYear = employee.BirthDate?.Year.ToString() ?? string.Empty;
                        string education = employee.Education?.ToString() ?? string.Empty;
                        string department = employeeAttribute?.Title?.ToString() ?? string.Empty;
                        
                        string personalInfo = $"{birthYear} yılında {formattedBirthPlace} doğdum. Eğitimimi tamamladıktan sonra, gayrimenkul sektörüne ilgi duydum. Bugüne kadar birçok eğitim ve sertifika aldım ve artık sizlere bu alanda hizmet veriyorum.";
                        body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { After = "120" }
                            ),
                            new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text(personalInfo)
                            )
                        ));

                        // Yardımcı olabileceğim konular
                        body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { After = "120" }
                            ),
                            new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text("Size şu konularda yardımcı olabilirim:")
                            )
                        ));

                        // Liste maddeleri
                        List<string> listItems = new List<string>()
                        {
                            "Gayrimenkulünüzün değerini belirleme",
                            "Satış veya kiralama süreçlerinde hızlı ve güvenilir hizmet",
                            "Piyasa analizi ve yatırım danışmanlığı",
                            "Bürokratik işlemler konusunda rehberlik"
                        };

                        foreach (var item in listItems)
                        {
                            Paragraph listParagraph = new Paragraph(
                                new ParagraphProperties(
                                    new SpacingBetweenLines() { After = "0" },
                                    new Indentation() { Left = "720", Hanging = "360" }
                                )
                            );

                            Run bulletRun = new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text("• ")
                            );
                            listParagraph.AppendChild(bulletRun);

                            Run itemRun = new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text(item)
                            );
                            listParagraph.AppendChild(itemRun);

                            body.AppendChild(listParagraph);
                        }

                        // İletişim paragrafı
                        string phone = $"{employee.MobileCodeWork}{employee.MobileNoWork}";
                        string email = employee.Email ?? string.Empty;
                        string contactInfo = $"Bir komşunuz ve gayrimenkul danışmanınız olarak, ihtiyaç duyduğunuz her an bana ulaşabilirsiniz. Size en yakın ve hızlı hizmeti sunmaktan memnuniyet duyarım. İletişim bilgilerim: {phone}, {email}.";
                        body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { After = "240" }
                            ),
                            new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text(contactInfo)
                            )
                        ));

                        // İmza bölümü
                        body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { After = "0" }
                            ),
                            new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text("Sevgi ve Saygılarımla,")
                            )
                        ));
                        body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { After = "0" }
                            ),
                            new Run(
                                new RunProperties(new FontSize() { Val = "24" }),
                                new Text(employee.NameSurname)
                            )
                        ));
                        body.AppendChild(new Paragraph(
                            new Run(
                                new RunProperties(
                                    new Bold(),
                                    new FontSize() { Val = "24" }
                                ),
                                new Text("RE/MAX Gayrimenkul Danışmanı")
                            )
                        ));

                        // Belgeyi kaydet
                        mainPart.Document.Save();
                    }

                    memoryStream.Position = 0;
                    result = memoryStream.ToArray();
                }
            }
            else
            {
                // HTML şablonundaki doğum yeri cümlesini formatla
                template = FixBirthPlaceInHtml(template, birthPlaceOnly, DetermineLocativeSuffix(birthPlaceOnly));
                
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

        /// <summary>
        /// HTML içindeki doğum yeri cümlesini düzeltir
        /// </summary>
        private string FixBirthPlaceInHtml(string html, string cityName, string suffix)
        {
            if (string.IsNullOrEmpty(cityName))
                return html;

            // #BirthDate# yılında #BornCity#'de doğdum. -> "{year} yılında {city}'da/de doğdum."
            // Metindeki bu kalıbı bul ve değiştir
            string pattern = @"(\d+)\s+yılında\s+" + cityName + @"['']de\s+doğdum\.";
            string replacement = "$1 yılında " + cityName + "'" + suffix + " doğdum.";

            return Regex.Replace(html, pattern, replacement, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Şehir ismini formatlar: İlk harfi büyük yapar ve Türkçe dil kurallarına göre doğru bulunma durumu (-da/-de) ekini uygular
        /// </summary>
        /// <param name="cityName">Formatlanacak şehir ismi</param>
        /// <returns>Formatlanmış şehir ismi: İlk harf büyük ve doğru -da/-de eki ile</returns>
        private string FormatCityWithLocativeCase(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return string.Empty;

            // İlk harfi büyük yap
            string formattedCity = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cityName.ToLower());
            
            // Doğru bulunma durumu ekini (-da/-de) belirle
            string suffix = DetermineLocativeSuffix(formattedCity);
            
            // Şehir adı apostrofla bitiyorsa ('de şeklinde), direkt döndür
            if (formattedCity.EndsWith("'"))
                return formattedCity + suffix;
                
            // Normal şehir isimleri için 'da/de şeklinde ekle
            return formattedCity + "'" + suffix;
        }
        
        /// <summary>
        /// Türkçe büyük ünlü uyumu kurallarına göre bir kelime için uygun -da/-de ekini belirler
        /// </summary>
        /// <param name="word">İncelenecek kelime</param>
        /// <returns>Uygun ek: "da" veya "de"</returns>
        private string DetermineLocativeSuffix(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return "da"; // Varsayılan
                
            // Kelimeyi normalize et (küçük harfe çevir ve apostrof varsa kaldır)
            string normalizedWord = word.ToLower().TrimEnd('\'');
            
            // Kalın ünlüler: a, ı, o, u
            // İnce ünlüler: e, i, ö, ü
            char[] backVowels = new char[] { 'a', 'ı', 'o', 'u' };
            
            // Kelimede son bulunan ünlüyü bul
            char lastVowel = '\0';
            
            for (int i = normalizedWord.Length - 1; i >= 0; i--)
            {
                char c = normalizedWord[i];
                if ("aeıioöuü".Contains(c))
                {
                    lastVowel = c;
                    break;
                }
            }
            
            // Eğer ünlü bulunamazsa, varsayılan olarak 'da' döndür
            if (lastVowel == '\0')
                return "da";
                
            // Kalın ünlü ise 'da', ince ünlü ise 'de' döndür
            return backVowels.Contains(lastVowel) ? "da" : "de";
        }

        private void SetDocumentDefaults(MainDocumentPart mainPart)
        {
            // Font ve stil ayarları
            StyleDefinitionsPart stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles();

            // Normal stil
            Style normalStyle = new Style() { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true };
            normalStyle.AppendChild(new StyleName() { Val = "Normal" });
            normalStyle.AppendChild(new PrimaryStyle());

            // Font ayarları - Times New Roman
            StyleRunProperties normalProps = new StyleRunProperties();
            normalProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            normalProps.AppendChild(new FontSize() { Val = "24" }); // 12pt
            normalStyle.AppendChild(normalProps);

            stylesPart.Styles.AppendChild(normalStyle);
            stylesPart.Styles.Save();
        }
    }
}