using Application.Services.Base;
using Application.Services.Content;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Employee;

public class SetEmployeeVideoLink : BaseSvc<SetEmployeeVideoLink.Request, SetEmployeeVideoLink.Response>
{
    public record Request(int EmployeeId, string VideoLink);

    public record Response();

    public SetEmployeeVideoLink(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        string sql = "UPDATE [dbo].[Employee] SET [Video] = @p0 WHERE [Id] = @p1";
        await uow.DbContext.Database.ExecuteSqlRawAsync(sql, req.VideoLink, req.EmployeeId);

        await Svc<CheckContentCompletion>().InvokeAsync(uow,
            new CheckContentCompletion.Request(Constants.Constants.ContentHazirlikId, req.EmployeeId));

        await RemoveCache(req.EmployeeId);
        return new Response();
    }

    public async Task RemoveCache(int employeeId)
    {
        await CacheManager.RemoveAsync("GetEmployeeVideoLink_" + employeeId);
    }
}