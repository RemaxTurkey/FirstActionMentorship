using System;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Content;

public class AssignContentToEmployee : BaseSvc<AssignContentToEmployee.Request, AssignContentToEmployee.Response>
{
    public AssignContentToEmployee(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var contentEmployeeAssoc = await uow.Repository<ContentEmployeeAssoc>()
            .FindBy(x => x.ContentId == req.ContentId && x.EmployeeId == req.EmployeeId)
            .FirstOrDefaultAsync();

        if (contentEmployeeAssoc == null)
        {
            contentEmployeeAssoc = new ContentEmployeeAssoc
            {
                ContentId = req.ParentId,
                EmployeeId = req.EmployeeId,
                CompletionDate = DateTime.Now,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            await uow.Repository<ContentEmployeeAssoc>().AddAsync(contentEmployeeAssoc);
            await uow.SaveChangesAsync();
        }

        return new Response();
    }

    public record Request(int ContentId, int ParentId, int EmployeeId);

    public record Response();
} 