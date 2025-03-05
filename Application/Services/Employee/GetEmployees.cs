using Application.Services.Base;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Employee
{
    public class GetEmployees : BaseSvc<GetEmployees.Request, GetEmployees.Response>
    {
        public GetEmployees(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public record Request;
        public record Response(List<EmployeeDto> Employees);
        
        public record EmployeeDto(
            int Id,
            string NameSurname,
            string Email
        );

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employees = await uow.Repository<Data.Entities.dbo.Employee>()
                .FindByNoTracking(x => x.Id > 0)
                .Take(5)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.NameSurname ?? string.Empty,
                    e.Email ?? string.Empty
                ))
                .ToListAsync();

            var contents = await uow.Repository<Data.Entities.Content>()
                .FindByNoTracking(x => x.Id > 0)
                .Take(5)
                .ToListAsync();
            
            return new Response(employees);
        }
    }
}