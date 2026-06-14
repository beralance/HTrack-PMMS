using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Domain.Entities;

public class Project
{
    public Guid Id {get; set;}
    public DateOnly DateIssued {get; set;}
    public string ProjectName {get; set;} = string.Empty;
    public string Developer {get; set;} = string.Empty;
    public string Owner {get; set;} = string.Empty;
    public string CrNo {get; set;} = string.Empty;
    public string LsNo {get; set;} = string.Empty;
    public string Barangay {get; set;} = string.Empty;
    public string Salable {get; set;} = string.Empty;
    public bool IsFm {get; set;}
    public bool? HasCoc {get; set;}
    public bool? HasDod {get; set;}
    public DateOnly? CocDate {get; set;}
    public DateOnly? DodDate {get; set;}
    public ProjectStatuses? Status {get; set;}
    public ProjectSeverities? Severity {get; set;}
    public string? Remarks {get; set;} = string.Empty;
    
    public bool IsExtended {get; set;}
    public bool IsDeleted {get; set;}
    public bool IsModified {get; set;}
    public bool IsPublished {get; set;}
    public ProjectSetupStatuses? SetupStatus {get; set;}

    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset? UpdatedAt {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}

    public string? ModifiedById {get; set;}
    public string AddedById {get; set;} = string.Empty;
    public string? DeletedById {get; set;}
    public int MunicipalityId {get; set;}
    public int ProjectTypeId {get; set;}

    public ApplicationUser? ModifiedBy {get; set;}
    public ApplicationUser AddedBy {get; set;} = null!;
    public ApplicationUser? DeletedBy {get; set;}
    public Municipality Municipality {get; set;} = null!;
    public ProjectType ProjectType {get; set;} = null!;
}
