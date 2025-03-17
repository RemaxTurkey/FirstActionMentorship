using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.Services.ComponentTypeAttribute;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.Services.ComponentTypeAttributeAssoc;
using Application.Services.ComponentTypeAttributeAssoc.DTOs;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ComponentType
{
    public class UpdateComponentType : BaseSvc<UpdateComponentType.Request, UpdateComponentType.Response>
    {
        public class Request : ComponentTypeDto
        {
            public bool IsActive { get; set; } = true;
        }

        public class Response
        {
            public ComponentTypeDto Item { get; set; }
        }

        public UpdateComponentType(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            ArgumentException.ThrowIfNullOrEmpty(req.Title);
            ArgumentNullException.ThrowIfNull(req.Id);
            
            var componentType = await uow.Repository<Data.Entities.ComponentType>().GetByIdAsync(req.Id.Value);
            if (componentType == null)
            {
                throw new BusinessException("Component type not found");
            }

            componentType.Title = req.Title;
            
            uow.Repository<Data.Entities.ComponentType>().Update(componentType);
            await uow.SaveChangesAsync();

            if (req.Attributes != null && req.Attributes.Any())
            {
                foreach (var attributeDto in req.Attributes)
                {
                    if (!attributeDto.Id.HasValue)
                    {
                        var createAttributeResult = await Svc<CreateComponentTypeAttribute>().InvokeAsync(uow, 
                            new CreateComponentTypeAttribute.Request
                            {
                                Name = attributeDto.Name,
                                IsActive = req.IsActive,
                                DataType = attributeDto.DataType
                            });
                        
                        await Svc<AssignAttributeToComponentType>().InvokeAsync(uow, 
                            new AssignAttributeToComponentType.Request
                            {
                                ComponentTypeId = componentType.Id,
                                ComponentTypeAttributeId = createAttributeResult.Item.Id!.Value,
                                IsActive = req.IsActive
                            });
                    }
                    else
                    {
                        var existingAttribute = await uow.Repository<Data.Entities.ComponentTypeAttribute>()
                            .GetByIdAsync(attributeDto.Id.Value);
                        
                        if (existingAttribute != null)
                        {
                            existingAttribute.Name = attributeDto.Name;
                            existingAttribute.DataType = attributeDto.DataType;
                            
                            uow.Repository<Data.Entities.ComponentTypeAttribute>().Update(existingAttribute);
                        }
                    }
                }
                
                await uow.SaveChangesAsync();
            }

            var updatedComponentType = componentType.ToDto();
            
            var attributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
                new GetComponentTypeAttributes.Request(req.Id.Value));
            
            updatedComponentType.Attributes = attributesResponse.Attributes;

            return new Response
            {
                Item = updatedComponentType
            };
        }
    }
}