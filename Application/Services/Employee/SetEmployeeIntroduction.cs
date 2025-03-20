using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;

namespace Application.Services.Employee
{
    public class SetEmployeeIntroduction : BaseSvc<SetEmployeeIntroduction.Request, SetEmployeeIntroduction.Response>
    {
        public record Request(int EmployeeId, string Introduction);

        public record Response();

        public SetEmployeeIntroduction(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employeeDescription = await uow.Repository<EmployeeDescription>()
            .GetAsync(e => e.EmployeeId == req.EmployeeId);

            if (employeeDescription == null)
            {
                employeeDescription = new EmployeeDescription();
            }

            employeeDescription.Introduction = req.Introduction;

            uow.Repository<EmployeeDescription>().Update(employeeDescription);
            await uow.SaveChangesAsync();
            
            return new Response();
        }
    }
}