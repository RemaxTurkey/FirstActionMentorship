using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;


namespace Application.Services.Employee
{
    public class SetEmployeeSocialMediaDetails : BaseSvc<SetEmployeeSocialMediaDetails.Request, SetEmployeeSocialMediaDetails.Response>
    {
        public record Request(int EmployeeId, string Facebook, string Instagram, string Twitter, string LinkedIn, string Whatsapp, string Website, string Blogger);
        public record Response();

        public SetEmployeeSocialMediaDetails(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employee = await uow.Repository<Data.Entities.dbo.Employee>().GetByIdAsync(req.EmployeeId);

            if (employee == null)
            {
                throw new Exception("Employee not found");
            }

            employee.Facebook = req.Facebook;
            employee.Whatsapp = req.Whatsapp;
            employee.Twitter = req.Twitter;
            employee.Linkedin = req.LinkedIn;
            employee.Instagram = req.Instagram;
            employee.Website = req.Website;
            employee.Blogger = req.Blogger;
 
            uow.Repository<Data.Entities.dbo.Employee>().Update(employee);
            await uow.SaveChangesAsync();

            return new Response();
        }
    }
}