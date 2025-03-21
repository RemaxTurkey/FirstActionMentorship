using System;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;

namespace Application.Services.Employee
{
    public class GetEmployeeSocialMediaDetails : BaseSvc<GetEmployeeSocialMediaDetails.Request, GetEmployeeSocialMediaDetails.Response>
    {
        public record Request
        {
            public int EmployeeId { get; set; }
        }

        public record Response
        {
            public string Facebook { get; set; }
            public string Instagram { get; set; }
            public string Twitter { get; set; }
            public string LinkedIn { get; set; }
            public string Whatsapp { get; set; }
            public string Website { get; set; }
            public string Blogger { get; set; }
        }

        public GetEmployeeSocialMediaDetails(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employee = await uow.Repository<Data.Entities.dbo.Employee>().GetByIdAsync(req.EmployeeId);

            if (employee == null)
            {
                return new Response
                {
                    Facebook = string.Empty,
                    Instagram = string.Empty,
                    Twitter = string.Empty,
                    LinkedIn = string.Empty,
                    Whatsapp = string.Empty,
                    Website = string.Empty,
                    Blogger = string.Empty
                };
            }

            return new Response
            {
                Facebook = employee.Facebook ?? string.Empty,
                Instagram = employee.Instagram ?? string.Empty,
                Twitter = employee.Twitter ?? string.Empty,
                LinkedIn = employee.Linkedin ?? string.Empty,
                Whatsapp = employee.Whatsapp ?? string.Empty,
                Website = employee.Website ?? string.Empty,
                Blogger = employee.Blogger ?? string.Empty
            };
        }
    }
} 