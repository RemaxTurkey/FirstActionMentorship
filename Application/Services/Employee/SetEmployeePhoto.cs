using Application.Common;
using Application.Services.Base;
using Application.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Services.Employee;

public class SetEmployeePhoto : BaseSvc<SetEmployeePhoto.Request, SetEmployeePhoto.Response>
{
    private readonly IOptions<AppSettings> _appSettings;

    public SetEmployeePhoto(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings) : base(serviceProvider)
    {
        _appSettings = appSettings;
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var fileName = Guid.NewGuid().ToString().Replace("-", "");
        var fileExtension = req.Image.FileName.Split('.').Last();

        if (fileExtension == "jpeg") fileExtension = "jpg";

        var employeePath = req.EmployeeId.ToString().PadLeft(2, '0');
        employeePath = employeePath.Substring(employeePath.Length - 2, 2);

        var originalPath = Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath);
        if (!Directory.Exists(originalPath)) Directory.CreateDirectory(originalPath);

        employeePath = string.Concat(employeePath, "/_temp", fileName, ".", fileExtension);

        using (var stream = File.Create(Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath)))
        {
            await req.Image.CopyToAsync(stream);
        }

        using (var img = Image.Load(Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath)))
        {
            img.Mutate(x => x.Resize(330, 440));
            await ImageExtensions.SaveAsJpegAsync(img,
                Path.Combine(_appSettings.Value.PhotoUploadPath, "employee",
                    employeePath.Replace("_temp", "").Replace(string.Concat(".", fileExtension), ".jpg")),
                new JpegEncoder { Quality = 100 });
        }

        File.Delete(Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath));
        employeePath = employeePath.Replace("_temp", "").Replace(string.Concat(".", fileExtension), ".jpg");

        using (var img = Image.Load(Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath)))
        {
            await img.SaveAsync(
                Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath).Replace(".jpg", ".webp"),
                new WebpEncoder { Quality = 100 });
        }

        using (var img = Image.Load(Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath)))
        {
            img.Mutate(x => x.Resize(165, 220));
            await ImageExtensions.SaveAsync(img,
                Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath.Replace(".jpg", "_t.jpg")));
        }

        using (var img = Image.Load(Path.Combine(_appSettings.Value.PhotoUploadPath, "employee",
                   employeePath.Replace(".jpg", "_t.jpg"))))
        {
            await img.SaveAsync(
                Path.Combine(_appSettings.Value.PhotoUploadPath, "employee", employeePath).Replace(".jpg", "_t.webp"),
                new WebpEncoder { Quality = 100 });
        }

        var sql = "UPDATE [dbo].[Employee] SET [Photo] = @p0 WHERE Id = @p1";
        await uow.DbContext.Database.ExecuteSqlRawAsync(sql, employeePath, req.EmployeeId);

        await uow.SaveChangesAsync();

        await uow.Repository<Data.Entities.dbo.EmployeeRecordHistory>().AddAsync(new Data.Entities.dbo.EmployeeRecordHistory
        {
            Date = DateTime.Now,
            EmployeeId = req.EmployeeId,
            Ip = req.IpAddress,
            RelatedEmployeeId = req.EmployeeId,
            HistoryTypeId = EmployeeRecordHistoryType.PhotoChanged,
            IsMobile = true
        });
    
        await uow.SaveChangesAsync();

        await RemoveCache(req.EmployeeId);
        return new Response(req.EmployeeId);
    }

    public async Task RemoveCache(int employeeId)
    {
        await CacheManager.RemoveAsync("GetEmployeePhoto_" + employeeId);
    }

    public record Request(int EmployeeId, IFormFile Image)
    {
        public string IpAddress { get; set; }
    }

    public record Response(int EmployeeId);
}