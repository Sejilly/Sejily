using Sejily.API.Models.Enums.Medications;

namespace Sejily.API.Models.Helpers;


public class Dose
{
    public decimal Amount { get; set; }
    public DoseUnit Unit { get; set; }
}