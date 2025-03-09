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
        public class Request : ComponentTypeDto;

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

            // Yeni attributler eklenecekse
            if (req.Attributes != null && req.Attributes.Any())
            {
                // Mevcut attribute'ları getir
                var currentAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
                    new GetComponentTypeAttributes.Request { ComponentTypeId = req.Id.Value });
                
                var currentAttributeIds = currentAttributesResponse.Attributes
                    .Where(a => a.Id.HasValue)
                    .Select(a => a.Id.Value)
                    .ToList();
                
                foreach (var attributeDto in req.Attributes)
                {
                    // Yeni attribute ise ekle ve ilişkilendir
                    if (!attributeDto.Id.HasValue)
                    {
                        // Önce attribute oluştur
                        var createAttributeResult = await Svc<CreateComponentTypeAttribute>().InvokeAsync(uow, 
                            new CreateComponentTypeAttribute.Request
                            {
                                Name = attributeDto.Name
                            });
                        
                        // Sonra bu attribute'u ComponentType ile ilişkilendir
                        await Svc<AssignAttributeToComponentType>().InvokeAsync(uow, 
                            new AssignAttributeToComponentType.Request
                            {
                                ComponentTypeId = componentType.Id,
                                ComponentTypeAttributeId = createAttributeResult.Item.Id.Value
                            });
                    }
                }
            }

            // Component Type'ı ve attribute'larını tekrar getir
            var updatedComponentType = componentType.ToDto();
            
            // Attributeler ile ilgili bilgileri getir
            var attributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
                new GetComponentTypeAttributes.Request { ComponentTypeId = req.Id.Value });
            
            updatedComponentType.Attributes = attributesResponse.Attributes;

            return new Response
            {
                Item = updatedComponentType
            };
        }
    }
}