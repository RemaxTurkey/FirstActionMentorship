using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;


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
            // Employee var olup olmadığını kontrol etmek için basit bir sorgu
            string checkSql = "SELECT COUNT(1) FROM [dbo].[Employee] WHERE [Id] = @p0";
            var count = await uow.DbContext.Database.ExecuteSqlRawAsync(checkSql, req.EmployeeId);
            
            if (count == 0)
            {
                throw new Exception("Employee not found");
            }

            // Sosyal medya detaylarını güncellemek için SQL sorgusu
            string sql = @"UPDATE [dbo].[Employee] 
                          SET [Facebook] = @p0,
                              [Whatsapp] = @p1,
                              [Twitter] = @p2,
                              [Linkedin] = @p3,
                              [Instagram] = @p4,
                              [Website] = @p5,
                              [Blogger] = @p6
                          WHERE [Id] = @p7";
            
            await uow.DbContext.Database.ExecuteSqlRawAsync(sql, 
                req.Facebook, 
                req.Whatsapp, 
                req.Twitter, 
                req.LinkedIn, 
                req.Instagram, 
                req.Website, 
                req.Blogger, 
                req.EmployeeId);
            
            return new Response();
        }
    }
}