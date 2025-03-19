using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.dbo
{
    public class EmployeeRecordHistory : Entity
    {
        public EmployeeRecordHistoryType HistoryTypeId { get; set; }
		public int RelatedEmployeeId { get; set; }
		public int EmployeeId { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public DateTime Date { get; set; }
		public string Ip { get; set; }
		public bool? IsMobile { get; set; }
		public int? OfficeId { get; set; }
    }
}

public enum EmployeeRecordHistoryType
    {
        SocialMediaChanged = 1,
        UsernameChanged = 2,
        EmailChanged = 3,
        PhoneChanged = 4,
        FileChanged = 5,
        CriminalRecordChanged = 6,
        StatusChanged = 7,
        SubscriptionExpireDateChanged = 8,
        SubscriptionStartDateChanged = 9,
        PhotoChanged = 10,
        BasicInfo = 11,
        //AgreementInfo = 12,
        NameSurname = 13,
        Username = 14,
        RoleChanged = 15,
        AccountingInfo = 16,
        OtherInfo = 17,
        CommunicationInfo = 18,
        NameSurnameChanged = 19,
        NameOnDocChanged = 20,
        SurnameOnDocChanged = 21,
        EmployeeCreated = 22,
        OfficeChanged = 23,
        JobChanged = 24,
        ProfessionChanged = 25,
        LicenceInfoChanged = 26,
        TeamChanged = 27,
        SpokenLanguageChanged = 28,
        SpecialityAreaChanged = 29,
        PasswordReset = 30,
        DescriptionChanged = 31,
        TaxInfoChanged = 32,
        LicenceDeleted = 33,
        LicenceAdded = 34,
        WorkingStatusChanged = 35,
        LicenceApplyFileChanged = 36,
        ProductAttendChanged = 37,
        TeamStatusChanged=39
    }