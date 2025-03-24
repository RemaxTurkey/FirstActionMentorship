using System;
using System.Threading.Tasks;
using Application.Attributes;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;

namespace Application.Services.Employee
{
    public class GetEmployeeIntroduction : BaseSvc<GetEmployeeIntroduction.Request, GetEmployeeIntroduction.Response>
    {
        public record Request
        {
            public int EmployeeId { get; set; }
        }

        public record Response
        {
            public string Introduction { get; set; }
        }

        public GetEmployeeIntroduction(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Cache("GetEmployeeIntroduction_{EmployeeId}", 3600)]
        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employeeDescription = await uow.Repository<EmployeeDescription>()
                .GetAsync(e => e.EmployeeId == req.EmployeeId 
                               && e.Active == true && e.LanguageId == 1);

            if (employeeDescription == null)
            {
                return new Response
                {
                    Introduction = string.Empty
                };
            }

            return new Response
            {
                Introduction = employeeDescription.Introduction ?? string.Empty
            };
        }
    }
} 