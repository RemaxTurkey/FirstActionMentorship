using System;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;

namespace Application.Services.Employee;

public class GetEmployeeVideoLink : BaseSvc<GetEmployeeVideoLink.Request, GetEmployeeVideoLink.Response>
{
    public record Request
    {
        public int EmployeeId { get; set; }
    }

    public record Response
    {
        public string VideoLink { get; set; }
    }

    public GetEmployeeVideoLink(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var employee = await uow.Repository<Data.Entities.dbo.Employee>().GetByIdAsync(req.EmployeeId);

        if (employee == null)
        {
            return new Response
            {
                VideoLink = string.Empty
            };
        }

        return new Response
        {
            VideoLink = employee.Video ?? string.Empty
        };
    }
} 