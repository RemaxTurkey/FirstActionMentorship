using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.Services.Component.Extensions;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class GetComponent(IServiceProvider serviceProvider)
    : BaseSvc<GetComponent.Request, GetComponent.Response>(serviceProvider)
{
    public class Request
    {
        public int Id { get; set; }
    }
    
    public class Response
    {
        public ComponentDto Item { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        var component = await uow.Repository<Data.Entities.Component>()
            .FindBy(c => c.Id == request.Id && c.IsActive)
            .Include(c => c.ComponentType)
            .Include(c => c.ComponentAttributeValue.Where(cav => cav.IsActive))
                .ThenInclude(cav => cav.ComponentTypeAttribute)
            .FirstOrDefaultAsync();
        
        if (component == null)
            throw new BusinessException($"Component with ID {request.Id} not found or not active.");
        
        return new Response { Item = component.ToDto() };
    }
} 