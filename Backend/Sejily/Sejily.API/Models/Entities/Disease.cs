namespace Sejily.API.Models.Entities;


public class Disease
{
    public int DiseaseId { get; set; }

    public DiseaseIcdCode IcdCode { get; set; } = null!;
    public Uri IcdUri { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? TitleAr { get; set; }

    public bool IsGenetic { get; set; }
    public bool IsChronic { get; set; }
    public bool IsAllergy { get; set; }
}