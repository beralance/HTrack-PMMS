namespace PMMS.Server.Domain.Entities;

public class Draft
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
    public DateOnly DateOfCompletion {get; set;}
    public DateOnly? ExtensionOfTime {get; set;}
    public DateOnly? PerformanceBond {get; set;}
    public DateOnly? SemestralReport {get; set;}
    
    public string? Remarks {get; set;} = string.Empty;
    public bool? IsPublished {get; set;}
    public DateTimeOffset? PublishedAt {get; set;}

    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset? UpdatedAt {get; set;}

    public string AddedById {get; set;} = string.Empty;
    public string? UpdatedById {get; set;}
    public int MunicipalityId {get; set;}
    public int ProjectTypeId {get; set;}
    public string? PublishedById {get; set;}

    public ApplicationUser AddedBy {get; set;} = null!;
    public ApplicationUser? UpdatedBy {get; set;}
    public Municipality Municipality {get; set;} = null!;
    public ProjectType ProjectType {get; set;} = null!;
    public ApplicationUser? PublishedBy {get; set;}
}
