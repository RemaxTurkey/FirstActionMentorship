﻿namespace Data.Entities.dbo;

public class Employee : Entity
{
    public int Id { get; set; }
    public byte? ChildrenCount { get; set; }
    public byte? MaritalStatus { get; set; }
    public int? MasterEmployeeId { get; set; }
    public bool? EmployeeHasOwnCompany { get; set; }
    public string EmployeeNo { get; set; }
    public string InternationalRaNo { get; set; }
    public string FramesNo { get; set; }
    public string GlobalId { get; set; }
    public bool TransferGlobal { get; set; }
    public string NameOnDoc { get; set; }
    public string SurnameOnDoc { get; set; }
    public string NameSurname { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string MobileCodePersonal { get; set; }
    public string MobileNoPersonal { get; set; }
    public string MobileCodeWork { get; set; }
    public string MobileNoWork { get; set; }
    public string Email { get; set; }
    public string EmailPersonal { get; set; }
    public string Website { get; set; }
    public bool WebsiteApproved { get; set; }
    public string Photo { get; set; }
    public DateTime RegisterDate { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionExpireDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? RemoveDate { get; set; }
    public bool SmsSend { get; set; }
    public bool IsEYT { get; set; }
    public bool? IsHaveKids { get; set; }
    public DateTime? BirthDate { get; set; }
    
    public string BirthPlace { get; set; }
    public byte? Education { get; set; }
    public int? RemoveReasonId { get; set; }
    public string RemoveReasonDesc { get; set; }
    public int? ResidenceCityId { get; set; }
    public int? ResidenceTownId { get; set; }
    public int? ResidenceNeighborhoodId { get; set; }
    public int? ResidenceStreetId { get; set; }
    public int? ResidenceBuildingId { get; set; }
    public string ResidenceApartmentNumber { get; set; }
    public string ResidenceAddress { get; set; }
    public string ResidenceZipCode { get; set; }
    public int? TeamId { get; set; }
    public decimal? CommentRate { get; set; }
    public string IdentificationNo { get; set; }
    public bool? CriminalRecordApproved { get; set; }
    public bool? GdtAuthority { get; set; }
    public string AccountingNameSurname { get; set; }
    public string AccountingPhoneCode { get; set; }
    public string AccountingPhoneNo { get; set; }
    public string AccountingMobilePhoneCode { get; set; }
    public string AccountingMobilePhoneNo { get; set; }
    public string AccountingEmail { get; set; }
    public int? RoleId { get; set; }
    public string Blogger { get; set; }
    public string Video { get; set; }
    public string Facebook { get; set; }
    public string Twitter { get; set; }
    public string Linkedin { get; set; }
    public bool? ContractConfirmationBroker { get; set; }
    public bool? ContractConfirmationRa { get; set; }
    public string ContractFile { get; set; }
    public string RecruitSource { get; set; }
    public string GooglePlus { get; set; }
    public string Instagram { get; set; }
    public bool? DeletedPoolShare { get; set; }
    public string ProfessionalCertificateNumber { get; set; }
    public string DiplomaFile { get; set; }
    public bool? BlackListed { get; set; }
    public string AuthorizationLicenceNo { get; set; }
    public string AuthorizationLicenceName { get; set; }
    public DateTime? AuthorizationLicenceStartDate { get; set; }
    public string AuthorizationLicenceFile { get; set; }
    public string AuthorizationLicenceRejectNote { get; set; }
    public string SgkDeclaration { get; set; }
    public int? NationalityId { get; set; }
    public string SpouseName { get; set; }
    public DateTime? SpouseBirthDate { get; set; }
    public string Whatsapp { get; set; }
    public Guid? ResetPasswordCode { get; set; }
    public string AuthorizationLicenceApplyFile { get; set; }
    public DateTime? AuthorizationLicenceApplyDate { get; set; }
    public bool? LicenceApproved { get; set; }
    public string PendingReasonDesc { get; set; }
    public string MasterEmployeeCode { get; set; }
    public string OtherEmployeeCodes { get; set; }
    public int? RtDepartmentId { get; set; }
    public int? RtJobId { get; set; }
    public bool? RtHideOnList { get; set; }
    public bool? ShowCommunicationOnWebsite { get; set; }
    public int CriminalStatus { get; set; }
    public DateTime? CriminalStatusApprovedDate { get; set; }
    public string CriminalFilePath { get; set; }
    public DateTime? CriminalFileCreateDate { get; set; }
    public int? EuropeRegisterDay { get; set; }
    public int? PrevOccupationId { get; set; }
    public int? EducationalId { get; set; }
    public string PushToken { get; set; }
    public int? MyRecruitSourceMain { get; set; }
    public string LeftAgentReasonTypeDesc { get; set; }
    public bool? UnapprovedEntry { get; set; }
    public bool? BlockAutoPropertyApprove { get; set; }
    public DateTime? CriminalFileExpireDate { get; set; }
    public bool? ShowFirstLoginMessage { get; set; }
    public int? ReferenceEmployeeId { get; set; }
    
    public override bool IsActive => true;
}

public class EmployeeWorkingDays
{
    public int EmployeeId { get; set; }
    public int WorkingDays { get; set; }
}

public class EmployeeSpecialtyArea : Entity
{
    public int EmployeeId { get; set; }
    public int NeighborhoodId { get; set; }
}

public class Neighborhood : Entity
{
    public string NeighborhoodName { get; set; }
}


public class EmployeeProfession : Entity
{
    public int EmployeeId { get; set; }
    public int ProfessionId { get; set; }
}

public class Profession : Entity
{
    public string ProfessionName { get; set; }
}