using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities.dbo;
using Microsoft.EntityFrameworkCore;
using RemaxSiteService.Notification.Abstractions;
using RemaxSiteService.Notification.DTOs;

namespace Application.Services.Notification
{
    public class FAMAcceptanceGDSMSNotification : BaseSvc<FAMAcceptanceGDSMSNotification.Request, FAMAcceptanceGDSMSNotification.Response>
    {
        public FAMAcceptanceGDSMSNotification(IServiceProvider serviceProvider, INotificationFactory notificationFactory) : base(serviceProvider)
        {
            _notificationFactory = notificationFactory;
        }

        private readonly INotificationFactory _notificationFactory;
        
        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var employeeIds = await uow.DbContext.Database.SqlQuery<int>($@"SELECT
                            EmployeeFirstApprovalDates.EmployeeId
                        FROM EmployeeFirstApprovalDates
                        WHERE
                            DATEADD(DAY, 10, Ctimestamp) = CAST(GETDATE() AS DATE)
                        AND JobId IN (2, 31, 32)
                            AND EmployeeId NOT IN(
                        SELECT 
                            EmployeeId
                        FROM fam.EmployeeAcceptance)").ToListAsync();

            if (!employeeIds.Any())
            {
                return new Response();
            }

            
            var employees = await uow.Repository<Data.Entities.dbo.Employee>()
            .FindByNoTracking(x => employeeIds.Contains(x.Id))
            .ToListAsync();

            var smsTemplate = $@"Değerli #Ad Soyad#,
Mobil uygulamamıza giriş yaparak sizin için hazırladığımız ücretsiz İlk İşlem Mentörlüğü programı ile gayrimenkul yolculuğunuza adım atabilirsiniz!
Mobil Uygulamayı indirmek için: https://www.remax.com.tr/myremaxapp.html
Sizi aramızda görmek için sabırsızlanıyoruz!
RE/MAX Türkiye";

            foreach (var item in employeeIds)
            {
                var employee = employees.FirstOrDefault(x => x.Id == item);
                if (employee == null)
                {
                    continue;
                }

                var smsText = smsTemplate.Replace("#Ad Soyad#", employee.NameSurname);

                await _notificationFactory.SendNotification(new SmsNotificationRequest(){
                    Message = smsText,
                    PhoneNumber = employee.MobileNoWork
                });
            }

            return new Response();
        }

        public record Request();
        public record Response();

        public class EmployeeInfo
        {
            public int EmployeeId { get; set; }
            public string NameSurname { get; set; }
        }
    }
}