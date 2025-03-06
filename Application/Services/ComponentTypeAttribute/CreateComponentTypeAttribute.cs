using Application.Exceptions;
using Application.Services.Base;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.Services.ComponentTypeAttribute.Extensions;
using Application.UnitOfWorks;
using System;
using System.Threading.Tasks;

namespace Application.Services.ComponentTypeAttribute;

public class CreateComponentTypeAttribute(IServiceProvider serviceProvider)
    : BaseSvc<CreateComponentTypeAttribute.Request, CreateComponentTypeAttribute.Response>(serviceProvider)
{
    public class Request : ComponentTypeAttributeDto;
    
    public class Response
    {
        public ComponentTypeAttributeDto Item { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        ArgumentException.ThrowIfNullOrEmpty(request.Name);
        
        var newAttribute = new Data.Entities.ComponentTypeAttribute
        {
            Name = request.Name,
            Value = request.Value
        };
        
        await uow.Repository<Data.Entities.ComponentTypeAttribute>()
            .AddAsync(newAttribute);
        await uow.SaveChangesAsync();
        
        return new Response { Item = newAttribute.ToDto() };
    }
} 