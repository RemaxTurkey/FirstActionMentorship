using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            .GetAsync(e => e.EmployeeId == req.EmployeeId 
                           && e.LanguageId == 1
                           && e.Active == true);

            if (employeeDescription == null)
            {
                employeeDescription = new EmployeeDescription
                {
                    Introduction = req.Introduction,
                    EmployeeId = req.EmployeeId,
                    LanguageId = 1,
                    Active = true
                };

                await uow.Repository<EmployeeDescription>().AddAsync(employeeDescription);
                await uow.SaveChangesAsync();

                await RemoveCache(req.EmployeeId);

                return new Response();
            }

            employeeDescription.Introduction = req.Introduction;

            uow.Repository<EmployeeDescription>().Update(employeeDescription);
            await uow.SaveChangesAsync();
            
            await RemoveCache(req.EmployeeId);

            return new Response();
        }

        public async Task RemoveCache(int employeeId)
        {
            await CacheManager.RemoveAsync("GetEmployeeIntroduction_" + employeeId);
        }
    }
}