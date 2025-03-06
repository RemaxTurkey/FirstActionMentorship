using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.UnitOfWorks;

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

        protected async override Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var componentType = await uow.Repository<Data.Entities.ComponentType>().GetByIdAsync(req.Id);
            if (componentType == null)
            {
                throw new Exception("Component type not found");
            }

            componentType.Title = req.Title;
            
            uow.Repository<Data.Entities.ComponentType>().Update(componentType);
            await uow.SaveChangesAsync();

            return new Response
            {
                Item = componentType.ToDto()
            };
        }
    }
}