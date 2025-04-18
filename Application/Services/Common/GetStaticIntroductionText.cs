using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;

namespace Application.Services.Common
{
    public class GetStaticIntroductionText : BaseSvc<GetStaticIntroductionText.Request, GetStaticIntroductionText.Response>
    {
        public GetStaticIntroductionText(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var staticIntroductionTextPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "StaticIntroductionText.txt");
            var staticIntroductionText = await File.ReadAllTextAsync(staticIntroductionTextPath);
            return new Response(staticIntroductionText);
        }

        public record Request();
        public record Response(string IntroductionText);
    }
}