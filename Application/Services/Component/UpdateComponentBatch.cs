using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class UpdateComponentBatch(IServiceProvider serviceProvider)
    : BaseSvc<UpdateComponentBatch.Request, UpdateComponentBatch.Response>(serviceProvider)
{
    public class Request
    {
        public List<UpdateComponentItem> Components { get; set; }
    }
    
    public class UpdateComponentItem
    {
        public int Id { get; set; }
        public int ComponentTypeId { get; set; }
        public List<ComponentItemDto> Items { get; set; }
        public List<ComponentTypeAttributeValueDto> ComponentTypeAttributeValues { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    public class Response
    {
        public List<ComponentDto> Items { get; set; }
        public bool AllSuccessful { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        if (request.Components == null || !request.Components.Any())
        {
            throw new BusinessException("At least one component must be provided");
        }
        
        var results = new List<ComponentDto>();
        var allSuccessful = true;
        
        foreach (var componentToUpdate in request.Components)
        {
            try
            {
                var updateResult = await Svc<UpdateComponent>().InvokeAsync(uow, new UpdateComponent.Request
                {
                    Id = componentToUpdate.Id,
                    ComponentTypeId = componentToUpdate.ComponentTypeId,
                    Items = componentToUpdate.Items,
                    ComponentTypeAttributeValues = componentToUpdate.ComponentTypeAttributeValues,
                    IsActive = componentToUpdate.IsActive
                });
                
                results.Add(updateResult.Item);
            }
            catch (Exception ex)
            {
                allSuccessful = false;
                // İsterseniz burada hata loglaması yapabilirsiniz
                // Batch işlemde bir hata olduğunda diğer işlemlere devam etmek için try-catch içinde işliyoruz
            }
        }
        
        return new Response
        {
            Items = results,
            AllSuccessful = allSuccessful
        };
    }
} 