using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.Services.ContentCategory.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.UnitOfWorks;

namespace Application.Services.ContentCategory;

public class GetContentCategory(IServiceProvider serviceProvider)
    : BaseSvc<GetContentCategory.Request, GetContentCategory.Response>(serviceProvider)
    {
        public record Request(int Id);

        public record Response(ContentCategoryDto Data);

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var category = await uow.Repository<Data.Entities.ContentCategory>()
            .GetByIdAsync(req.Id);

        if (category == null)
            throw new Exception("Content category not found");
            
        return new Response(Data: category.ToDto());
    }
}
