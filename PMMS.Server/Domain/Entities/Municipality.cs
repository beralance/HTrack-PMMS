namespace PMMS.Server.Domain.Entities;

public class Municipality
{
    public int Id {get; set;}
    public string MunicipalityName {get; set;} = string.Empty;

    public int ProvinceId {get; set;}

    public Province Province {get; set;} = null!;
}
