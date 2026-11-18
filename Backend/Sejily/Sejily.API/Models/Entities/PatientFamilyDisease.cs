using Sejily.API.Models.Enums;


namespace Sejily.API.Models.Entities;


public class PatientFamilyDisease
{
    public int PatientId { get; set; }
    public int DiseaseId { get; set; }
    public string DiseaseEntryName {get; set;} = null!;

    public RelationshipType FamilyRelationship { get; set; }

    public Patient Patient { get; set; } = null!;
    public Disease Disease { get; set; } = null!;
}