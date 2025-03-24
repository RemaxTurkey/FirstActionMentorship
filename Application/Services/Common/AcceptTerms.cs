using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Common
{
    public class AcceptTerms : BaseSvc<AcceptTerms.Request, AcceptTerms.Response>
    {
        public AcceptTerms(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employeeAcceptance = await uow.Repository<EmployeeAcceptance>()
            .FindByNoTracking(x => x.EmployeeId == req.EmployeeId)
            .FirstOrDefaultAsync();

            if (employeeAcceptance == null)
            {
                employeeAcceptance = new EmployeeAcceptance { EmployeeId = req.EmployeeId, AcceptanceDate = DateTime.Now, IsActive = true};
                await uow.Repository<EmployeeAcceptance>().AddAsync(employeeAcceptance);
                await uow.SaveChangesAsync();
            }

            return new Response(true);
        }

        public record Request(int EmployeeId);
        public record Response(bool IsSuccess);
    }
}