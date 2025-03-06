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
    public class CreateComponentType : BaseSvc<CreateComponentType.Request, CreateComponentType.Response>
    {
        public CreateComponentType(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected async override Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            ArgumentException.ThrowIfNullOrEmpty(req.Title);
            
            var componentType = new Data.Entities.ComponentType
            {
                Title = req.Title
            };

            await uow.Repository<Data.Entities.ComponentType>().AddAsync(componentType);
            await uow.SaveChangesAsync();

            return new Response
            {
                Item = componentType.ToDto()
            };
        }

        public class Request : ComponentTypeDto;

        public class Response
        {
            public ComponentTypeDto Item { get; set; }
        }
        
    }
}