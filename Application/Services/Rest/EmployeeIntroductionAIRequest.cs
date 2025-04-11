using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Common;
using Application.Services.Base;
using Application.UnitOfWorks;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Data.Entities.dbo;

namespace Application.Services.Rest
{
    public class EmployeeIntroductionAIRequest : BaseSvc<EmployeeIntroductionAIRequest.Request, EmployeeIntroductionAIRequest.Response>
    {
        private readonly RemaxySettings _appSettings;
        private readonly HttpClient _httpClient;

        public EmployeeIntroductionAIRequest(IServiceProvider serviceProvider, IOptions<RemaxySettings> appSettings) : base(serviceProvider)
        {
            _appSettings = appSettings.Value;
            _httpClient = new HttpClient();
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employee = await uow.Repository<Data.Entities.dbo.Employee>()
                .FindByNoTracking(x => x.Id == req.EmployeeId)
                .FirstAsync();
                
            var employeeAttribute = await uow.Repository<EmployeeAttribute>()
                .FindByNoTracking(x => x.Id == employee.EducationalId
                    && x.AttributeType == EmployeeAttributeType.Educational)
                .FirstOrDefaultAsync();

            var experienceYear = await uow.DbContext.Database
                .SqlQuery<EmployeeWorkingDays>(
                    $"SELECT * FROM EmployeeWorkingDays WHERE EmployeeId = {req.EmployeeId}")
                .FirstOrDefaultAsync();

            var employeeProfession = await uow.Repository<EmployeeProfession>()
                .FindByNoTracking(x => x.EmployeeId == req.EmployeeId)
                .Select(x => x.ProfessionId)
                .ToListAsync();

            string[] professionNames = new string[0];
            if (employeeProfession.Count > 0)
            {
                professionNames = await uow.Repository<Profession>()
                    .FindByNoTracking(x => employeeProfession.Contains(x.Id))
                    .Select(x => x.ProfessionName)
                    .ToArrayAsync();
            }

            string[] neighborhoodNames = new string[0];
            var employeeSpecialtyArea = await uow.Repository<EmployeeSpecialtyArea>()
                .FindByNoTracking(x => x.EmployeeId == req.EmployeeId)
                .Select(x => x.NeighborhoodId)
                .ToListAsync();

            if (employeeSpecialtyArea.Count > 0)
            {
                neighborhoodNames = await uow.Repository<Neighborhood>()
                    .FindByNoTracking(x => employeeSpecialtyArea.Contains(x.Id))
                    .Select(x => x.NeighborhoodName)
                    .ToArrayAsync();
            }

            var workingYearCalculated = experienceYear?.WorkingDays / 365.0;
            var workingYear = workingYearCalculated.HasValue ? 
                (int?)Math.Ceiling(workingYearCalculated.Value) : null;
            if (workingYear.HasValue && workingYear.Value == 0)
                workingYear = 1;

            var apiUrl = _appSettings.ApiUrl;

            var profileDto = new ProfileDescriptionDto
            {
                nameSurname = employee.NameSurname ?? string.Empty,
                birthPlace = employee.BirthPlace ?? string.Empty,
                birthDate = employee.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                areaOfExpertise = professionNames.ToArray(),
                regionOfExpertise = neighborhoodNames.ToArray(),
                experienceYear = workingYear,
                maritalStatus = employee.MaritalStatus == 1 ? true : false,
                numberOfChild = employee.ChildrenCount ?? 0,
                schoolName = string.Empty,
                educationDepartment = employeeAttribute?.Title ?? string.Empty
            };

            var content = new StringContent(
                JsonSerializer.Serialize(profileDto),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.Token);

            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<string>(responseContent);
                return new Response(){Text = result };
            }
            
            return new Response { Text = string.Empty };
        }

        public record Request(int EmployeeId);

        public record Response
        {
            public string Text { get; set; }
        }

        public class ProfileDescriptionDto
        {
            public string nameSurname { get; set; }
            public string birthPlace { get; set; }
            public string birthDate { get; set; }
            public string[] areaOfExpertise { get; set; }
            public string[] regionOfExpertise { get; set; }
            public int? experienceYear { get; set; }
            public bool? maritalStatus { get; set; }
            public int? numberOfChild { get; set; }
            public string schoolName { get; set; }
            public string educationDepartment { get; set; }
        }
    }
}