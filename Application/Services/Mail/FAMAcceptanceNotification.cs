using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Mail
{
    public class FAMAcceptanceNotification : BaseSvc<FAMAcceptanceNotification.Request, FAMAcceptanceNotification.Response>
    {
        // X saatinde her gün çalışacak. 
        // Her çalıştığında EmployeeAcceptance tablosunda IsBrokerNotified false olan kayıtları bulacak.
        // Burada her kayıt için ofis brokerları bulunacak.
        // Her borker için birden fazla employee bilgilendirmesi yapılacaksa isimler ',' ile birleştirilecek.
        private readonly IEmailService _emailService;
        public FAMAcceptanceNotification(IServiceProvider serviceProvider, IEmailService emailService) : base(serviceProvider)
        {
            _emailService = emailService;
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            // IsBrokerNotified false veya null olan EmployeeAcceptance kayıtlarını al
            var employees = await uow.Repository<EmployeeAcceptance>()
                .FindBy(x => x.IsBrokerNotified == false || x.IsBrokerNotified == null)
                .ToListAsync();

            if (!employees.Any())
            {
                return new Response();
            }

            // Brokerları ve employee'leri grupla
            var brokerToEmployeesMap = new Dictionary<string, List<EmployeeInfo>>();
            var allBrokers = new Dictionary<string, BrokerInfo>();
            
            foreach (var employee in employees)
            {
                var employeeInfo = await uow.DbContext.Database.SqlQuery<EmployeeInfo>($@"
                    SELECT Id as EmployeeId, NameSurname, Email
                    FROM [dbo].Employee
                    WHERE Id = {employee.EmployeeId}
                ").FirstOrDefaultAsync();
                
                if (employeeInfo == null)
                    continue;

                var brokers = await uow.DbContext.Database.SqlQuery<BrokerInfo>($@"
                    SELECT e.Email, e.NameSurname, e.Id as EmployeeId
                    FROM [dbo].Employee e
                    WHERE e.EmployeeStatusId = 2
                    AND e.Id IN (SELECT DISTINCT EmployeeId
                                FROM [dbo].OfficeEmployee oe
                                WHERE oe.Active = 1
                                AND oe.OfficeId IN (SELECT DISTINCT OfficeId
                                                    FROM [dbo].OfficeEmployee oe
                                                    WHERE oe.Active = 1
                                                        AND EmployeeId = {employee.EmployeeId})
                                AND oe.JobId = 2)
                ").ToListAsync();

                // Her broker için employee listesine ekle
                foreach (var broker in brokers)
                {
                    if (string.IsNullOrEmpty(broker.Email))
                        continue;

                    if (!allBrokers.ContainsKey(broker.Email))
                    {
                        allBrokers[broker.Email] = broker;
                    }

                    if (!brokerToEmployeesMap.ContainsKey(broker.Email))
                    {
                        brokerToEmployeesMap[broker.Email] = new List<EmployeeInfo>();
                    }
                    
                    if (!brokerToEmployeesMap[broker.Email].Any(e => e.EmployeeId == employeeInfo.EmployeeId))
                    {
                        brokerToEmployeesMap[broker.Email].Add(employeeInfo);
                    }
                }

                employee.IsBrokerNotified = true;
                uow.Repository<EmployeeAcceptance>().Update(employee);
            }

            foreach (var brokerEntry in brokerToEmployeesMap)
            {
                var brokerEmail = brokerEntry.Key;
                var employeesForBroker = brokerEntry.Value;

                if (!employeesForBroker.Any())
                    continue;

                var broker = allBrokers.GetValueOrDefault(brokerEmail);
                if (broker == null)
                    continue;

                var employeeNames = string.Join(", ", employeesForBroker.Select(e => $"<b>{e.NameSurname}</b>"));
                var subject = "İlk İşlem Mentörlüğü Hakkında";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style type='text/css'>
        body, p, div {{
            font-family: Arial, Helvetica, sans-serif;
            font-size: 14px;
            color: #333333;
        }}
        p {{
            margin: 0 0 16px 0;
            line-height: 1.5;
        }}
        ul {{
            margin: 0 0 16px 0;
            padding-left: 20px;
        }}
        li {{
            margin-bottom: 8px;
        }}
        b {{
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div style='max-width: 600px; margin: 0 auto;'>
        <p>Değerli {broker.NameSurname},</p>

        <p>Yeni Gayrimenkul Danışmanlarınızdan {employeeNames}, <b>ilk işlem mentörlüğü programına</b> katılmıştır. Kendilerini bu önemli adımından dolayı tebrik ediyoruz ve sizin de bu sürecin bir parçası olarak desteklerinizi sunmanızı bekliyoruz. Danışmanlarınızın ilk işlemini başarıyla gerçekleştirebilmesi için, onlara sağlayacağınız desteğin büyük bir önemi olduğuna inanıyoruz.</p>

        <p>Bu süreçte, <b>her hafta kendileriyle 30 dakikalık bir görüşme planlamanızı</b> önemle rica edeceğiz. Görüşmelerde aşağıdaki konularda gayrimenkul danışmanınıza rehberlik etmeniz büyük fayda sağlayacaktır:</p>

        <ul>
            <li><b>Hazırlık süreci:</b> Portföy öncesi gerekli hazırlıkların yapılması</li>
            <li><b>Portföy edinme:</b> Doğru ve etkili portföy alma stratejileri</li>
            <li><b>Pazarlama:</b> Portföylerin etkili bir şekilde pazarlanması</li>
            <li><b>Tapu süreci:</b> Resmi işlemlerin doğru ve hızlı bir şekilde yürütülmesi</li>
            <li><b>Kapanış:</b> İşlem sürecinin tamamlanması ve işlemin sonuçlandırılması</li>
            <li><b>Kapanış Sonrası Hizmetler:</b> Alıcı ve satıcı ile ilişkilerin sürdürülmesi</li>
        </ul>

        <p>Sizin sağlayacağınız bu rehberlik, danışmanlarınızın ilk işlemini gerçekleştirmesi için ona <b>özgüven kazandıracak</b> ve süreci <b>hızlandırarak</b> danışmanınızın uzun vadeli başarısına katkıda bulunacaktır.</p>

        <p>Desteğiniz ve iş birliğiniz için teşekkür eder, başarılı bir süreç geçirmenizi dileriz.</p>

        <p>Sevgilerimle,</p>

        <p>
        Evren Topal<br>
        İlk İşlem Mentörü<br>
        RE/MAX TÜRKİYE
        </p>
    </div>
</body>
</html>";

                var emailDto = new EmailDto
                {
                    To = new[] { brokerEmail },
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                await _emailService.SendEmail(emailDto);
            }
            
            await uow.SaveChangesAsync();
            return new Response();
        }

        private class BrokerInfo
        {
            public string Email { get; set; }
            public string NameSurname { get; set; }
            public int EmployeeId { get; set; }
        }

        private class EmployeeInfo
        {
            public int EmployeeId { get; set; }
            public string Email { get; set; }
            public string NameSurname { get; set; }
        }

        public record Request();
        public record Response();
    }
}