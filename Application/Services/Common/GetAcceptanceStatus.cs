using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Common
{
    public class GetAcceptanceStatus : BaseSvc<GetAcceptanceStatus.Request, GetAcceptanceStatus.Response>
    {
        private readonly IConfiguration _configuration;

        public record Request(int EmployeeId);
        public record Response(bool IsAccepted, string DocumentUrl);

        public GetAcceptanceStatus(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            _configuration = configuration;
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employeeAcceptance = await uow.Repository<EmployeeAcceptance>()
            .FindByNoTracking(x => x.EmployeeId == req.EmployeeId)
            .FirstOrDefaultAsync();

            return new Response(
                employeeAcceptance != null,
                _configuration.GetValue<string>("AcceptanceDocumentUrl")
            );
        }
    }
}