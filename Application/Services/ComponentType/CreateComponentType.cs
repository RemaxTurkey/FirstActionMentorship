using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.Services.ComponentTypeAttribute;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.Services.ComponentTypeAttributeAssoc;
using Application.Services.ComponentTypeAttributeAssoc.DTOs;
using Application.UnitOfWorks;

namespace Application.Services.ComponentType
{
    public class CreateComponentType : BaseSvc<CreateComponentType.Request, CreateComponentType.Response>
    {
        public CreateComponentType(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            ArgumentException.ThrowIfNullOrEmpty(req.Title);
            
            var componentType = new Data.Entities.ComponentType
            {
                Title = req.Title,
                Description = req.Description,
                IsActive = req.IsActive
            };

            await uow.Repository<Data.Entities.ComponentType>().AddAsync(componentType);
            await uow.SaveChangesAsync();

            // Attrıbuteler varsa ekleyelim
            if (req.Attributes != null && req.Attributes.Any())
            {
                foreach (var attributeDto in req.Attributes)
                {
                    var componentTypeAttributeId = attributeDto.Id ?? (await Svc<CreateComponentTypeAttribute>().InvokeAsync(uow,
                        new CreateComponentTypeAttribute.Request
                        {
                            Name = attributeDto.Name,
                            IsActive = req.IsActive,
                            DataType = attributeDto.DataType
                        })).Item.Id!.Value;

                    // Sonra bu attribute'u ComponentType ile ilişkilendir
                    await Svc<AssignAttributeToComponentType>().InvokeAsync(uow, new AssignAttributeToComponentType.Request
                    {
                        ComponentTypeId = componentType.Id,
                        ComponentTypeAttributeId = componentTypeAttributeId,
                        IsActive = req.IsActive
                    });
                }
            }

            return new Response
            {
                Item = componentType.ToDto()
            };
        }

        public class Request : ComponentTypeDto
        {
            public bool IsActive { get; set; } = true;
        }

        public class Response
        {
            public ComponentTypeDto Item { get; set; }
        }
        
    }
}