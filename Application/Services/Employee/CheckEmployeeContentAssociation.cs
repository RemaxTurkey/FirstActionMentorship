using System;
using System.Threading.Tasks;
using Application.Attributes;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Employee;

public class
    CheckEmployeeContentAssociation : BaseSvc<CheckEmployeeContentAssociation.Request,
    CheckEmployeeContentAssociation.Response>
{
    public CheckEmployeeContentAssociation(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Cache("CheckEmployeeContentAssociation_{ContentId}_{EmployeeId}_{PageType}", 600)]
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        bool exists = false;

        if (req.PageType == PageType.Content)
        {
            exists = await uow.Repository<ContentEmployeeRecord>()
                .FindByNoTracking(x =>
                    x.ContentId == req.ContentId &&
                    x.EmployeeId == req.EmployeeId)
                .AnyAsync();
        }
        else if (req.PageType == PageType.Menu)
        {
            exists = await uow.Repository<ContentEmployeeAssoc>()
                .FindByNoTracking(x =>
                    x.ContentId == req.ContentId &&
                    x.EmployeeId == req.EmployeeId)
                .AnyAsync();
        }
        else if (req.PageType == PageType.Static)
        {
            switch (req.ContentId)
            {
                case 7: // Profil Fotoğrafını Güncelle
                    {
                        var profilePhoto = await Svc<GetEmployeePhoto>().InvokeNoTrackingAsync(new GetEmployeePhoto.Request()
                        {
                            EmployeeId = req.EmployeeId
                        });

                        if (!string.IsNullOrEmpty(profilePhoto.Photo))
                        {
                            exists = true;
                        }
                        break;
                    }
                case 8: // Tanıtım Bilgisi Ekle
                    {
                        var introduction = await Svc<GetEmployeeIntroduction>().InvokeNoTrackingAsync(new GetEmployeeIntroduction.Request()
                        {
                            EmployeeId = req.EmployeeId
                        });

                        if (!string.IsNullOrEmpty(introduction.Introduction))
                        {
                            exists = true;
                        }
                        break;
                    }
                case 9: // Video CV Ekle
                    {
                        var video = await Svc<GetEmployeeVideoLink>().InvokeNoTrackingAsync(new GetEmployeeVideoLink.Request()
                        {
                            EmployeeId = req.EmployeeId
                        });

                        if (!string.IsNullOrEmpty(video.VideoLink))
                        {
                            exists = true;
                        }
                        
                        break;
                    }
                case 10: // Sosyal Medya Hesaplarını Ekle
                    {
                        var socialMedia = await Svc<GetEmployeeSocialMediaDetails>().InvokeNoTrackingAsync(new GetEmployeeSocialMediaDetails.Request()
                        {
                            EmployeeId = req.EmployeeId
                        });

                        if (!string.IsNullOrEmpty(socialMedia.Facebook) ||
                            !string.IsNullOrEmpty(socialMedia.Instagram) ||
                            !string.IsNullOrEmpty(socialMedia.Twitter) ||
                            !string.IsNullOrEmpty(socialMedia.LinkedIn) ||
                            !string.IsNullOrEmpty(socialMedia.Blogger) ||
                            !string.IsNullOrEmpty(socialMedia.Whatsapp) ||
                            !string.IsNullOrEmpty(socialMedia.Website))
                        {
                            exists = true;
                        }
                        
                        break;
                    }
                case 4: // Komşulara Mektup
                    {
                        exists = false;
                        break;
                    }
                default:
                    break;
            }
        }

        return new Response(exists);
    }

    public record Request(int ContentId, int EmployeeId, PageType PageType);

    public record Response(bool Exists);
}