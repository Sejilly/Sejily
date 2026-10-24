namespace Sejily.API.Models.Entities;


public class Disease
{
    public int DiseaseId { get; set; }
    public string IcdCode { get; set; } = null!;
    public string IcdVersion { get; set; } = null!;
    public Uri IcdUri { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool IsGenetic { get; set; }
    public bool IsChronic { get; set; }
    public string? TitleAr { get; set; }
}