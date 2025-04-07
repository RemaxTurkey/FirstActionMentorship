using System;
using System.Threading.Tasks;
using Application.Attributes;
using Application.Services.Base;
using Application.Services.Content;
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

    // [Cache("CheckEmployeeContentAssociation_{ContentId}_{EmployeeId}_{PageType}", 600)]
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        bool exists = false;

        if (req.PropertyId.HasValue)
        {
            exists = await uow.Repository<ContentEmployeeRecord>()
                .FindByNoTracking(x =>
                    x.ContentId == req.ContentId 
                    && x.EmployeeId == req.EmployeeId
                    && x.PropertyId == req.PropertyId)
                .AnyAsync();
        }
        else if (req.PageType == PageType.Content)
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
                    x.EmployeeId == req.EmployeeId && x.IsCompleted)
                .AnyAsync();

            // hazırlıksa staitk sayfaları kontrol et
            if (req.ContentId != Constants.Constants.ContentHazirlikId)
            {
                return new Response(exists);
            }

            // burası employee işlemi olduğunda çalışlır sadece
            var profilePhoto = await Svc<GetEmployeePhoto>().InvokeNoTrackingAsync(new GetEmployeePhoto.Request()
            {
                EmployeeId = req.EmployeeId
            });

            if (string.IsNullOrEmpty(profilePhoto.Photo))
            {
                return new Response(false);
            }

            var introduction = await Svc<GetEmployeeIntroduction>().InvokeNoTrackingAsync(
                new GetEmployeeIntroduction.Request()
                {
                    EmployeeId = req.EmployeeId
                });

            if (string.IsNullOrEmpty(introduction.Introduction))
            {
                return new Response(false);
            }

            var video = await Svc<GetEmployeeVideoLink>().InvokeNoTrackingAsync(new GetEmployeeVideoLink.Request()
            {
                EmployeeId = req.EmployeeId
            });

            if (string.IsNullOrEmpty(video.VideoLink))
            {
                return new Response(false);
            }

            var socialMedia = await Svc<GetEmployeeSocialMediaDetails>().InvokeNoTrackingAsync(
                new GetEmployeeSocialMediaDetails.Request()
                {
                    EmployeeId = req.EmployeeId
                });

            if (string.IsNullOrEmpty(socialMedia.Facebook) &&
                string.IsNullOrEmpty(socialMedia.Instagram) &&
                string.IsNullOrEmpty(socialMedia.Twitter) &&
                string.IsNullOrEmpty(socialMedia.LinkedIn) &&
                string.IsNullOrEmpty(socialMedia.Blogger) &&
                string.IsNullOrEmpty(socialMedia.Whatsapp) &&
                string.IsNullOrEmpty(socialMedia.Website))
            {
                return new Response(false);
            }

            if (exists)
            {
                return new Response(true);
            }

            // kod buraya geldiyse statik sayfaların hepsi tamamlanmış demektir.
            var siblingCheck = await Svc<GetSiblingContents>()
                .InvokeNoTrackingAsync(
                    new GetSiblingContents.Request(ParentContentId: Constants.Constants.ContentHazirlikId));
            if (siblingCheck.Contents.Count > 0)
            {
                var siblingContentCount = siblingCheck.Contents.Count;
                var siblingContentCompletedCount = 0;
                foreach (var sibling in siblingCheck.Contents)
                {
                    var siblingContentCompleted = await uow.Repository<ContentEmployeeRecord>()
                        .FindByNoTracking(x =>
                            x.ContentId == sibling.Id &&
                            x.EmployeeId == req.EmployeeId)
                        .AnyAsync();

                    if (siblingContentCompleted)
                    {
                        siblingContentCompletedCount++;
                    }
                }

                if (siblingContentCompletedCount == siblingContentCount)
                {
                    exists = true;

                    // kod buraya geldiyse eğitimleri bitirdikten sonra web ve ya farklı kanallardan eğitimleri bir şekilde tamamlamış demektir.
                    // dış platformlardan tamamlandığı için bu apiye kayıt gelmiyor. burada elle kayıt atmak gerek.
                    var contentEmployeeAssoc = await uow.Repository<ContentEmployeeAssoc>()
                        .FindByNoTracking(x =>
                            x.ContentId == req.ContentId &&
                            x.EmployeeId == req.EmployeeId)
                        .FirstAsync();
                    contentEmployeeAssoc.IsCompleted = true;
                    uow.Repository<ContentEmployeeAssoc>().Update(contentEmployeeAssoc);
                    await uow.SaveChangesAsync();
                }
            }

            return new Response(exists);
        }
        else if (req.PageType == PageType.Static)
        {
            switch (req.ContentId)
            {
                case 7: // Profil Fotoğrafını Güncelle
                {
                    var profilePhoto = await Svc<GetEmployeePhoto>().InvokeNoTrackingAsync(
                        new GetEmployeePhoto.Request()
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
                    var introduction = await Svc<GetEmployeeIntroduction>().InvokeNoTrackingAsync(
                        new GetEmployeeIntroduction.Request()
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
                    var video = await Svc<GetEmployeeVideoLink>().InvokeNoTrackingAsync(
                        new GetEmployeeVideoLink.Request()
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
                    var socialMedia = await Svc<GetEmployeeSocialMediaDetails>().InvokeNoTrackingAsync(
                        new GetEmployeeSocialMediaDetails.Request()
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
                case 22: // Portföy Edinme -> Komşu Mektubunu Dağıt
                {
                    exists = await uow.Repository<ContentEmployeeRecord>()
                        .FindByNoTracking(x =>
                            x.ContentId == req.ContentId &&
                            x.EmployeeId == req.EmployeeId)
                        .AnyAsync();
                    break;
                }
                default:
                    break;
            }
        }

        return new Response(exists);
    }

    public record Request(int ContentId, int EmployeeId, PageType PageType, int? PropertyId = null);

    public record Response(bool Exists);
}