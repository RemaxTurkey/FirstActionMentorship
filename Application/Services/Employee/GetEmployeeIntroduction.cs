using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Attributes;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;

namespace Application.Services.Employee
{
    public class GetEmployeeIntroduction : BaseSvc<GetEmployeeIntroduction.Request, GetEmployeeIntroduction.Response>
    {
        public record Request
        {
            public int EmployeeId { get; set; }
        }

        public record Response
        {
            public string Introduction { get; set; }
        }

        public GetEmployeeIntroduction(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employeeDescription = await uow.Repository<EmployeeDescription>()
                .GetAsync(e => e.EmployeeId == req.EmployeeId
                               && e.Active == true && e.LanguageId == 1);

            if (employeeDescription == null)
            {
                return new Response
                {
                    Introduction = string.Empty
                };
            }

            var introduction = employeeDescription.Introduction;

            var removeHtmLtagsRegex = new Regex("<(?:\"[^\"]*\"['\"]*|'[^']*'['\"]*|[^'\">])+>");
            introduction = !String.IsNullOrEmpty(introduction)
                ? removeHtmLtagsRegex.Replace(introduction, "").Replace("&nbsp;", " ").Replace("\r\n", " ")
                : "";

            return new Response
            {
                Introduction = introduction
            };
        }
    }
}