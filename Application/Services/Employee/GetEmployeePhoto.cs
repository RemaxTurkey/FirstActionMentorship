using System;
using System.Threading.Tasks;
using Application.Attributes;
using Application.Common;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;
using Microsoft.Extensions.Options;

namespace Application.Services.Employee
{
    public class GetEmployeePhoto : BaseSvc<GetEmployeePhoto.Request, GetEmployeePhoto.Response>
    {
        private readonly IOptions<AppSettings> _appSettings;

        public record Request
        {
            public int EmployeeId { get; set; }
        }

        public record Response
        {
            public string PhotoUrl { get; set; }
        }

        public GetEmployeePhoto(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings) : base(serviceProvider)
        {
            _appSettings = appSettings;
        }
        
        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employee = await uow.Repository<Data.Entities.dbo.Employee>().GetByIdAsync(req.EmployeeId);

            if (employee == null)
            {
                return new Response
                {
                    PhotoUrl = string.Empty
                };
            }

            string photoUrl = employee.Photo;
            
            if (!string.IsNullOrEmpty(photoUrl) && !photoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                photoUrl = GenerateEmployeePhotoUrl(_appSettings.Value.PhotoUploadUrl + "employee", photoUrl);
            }

            return new Response
            {
                PhotoUrl = photoUrl ?? string.Empty
            };
        }

        private string GenerateEmployeePhotoUrl(string baseUrl, string photoPath)
        {
            if (string.IsNullOrEmpty(photoPath))
                return string.Empty;
                
            return $"{baseUrl.TrimEnd('/')}/{photoPath.TrimStart('/')}".Replace("\\", "/");
        }
    }
} 