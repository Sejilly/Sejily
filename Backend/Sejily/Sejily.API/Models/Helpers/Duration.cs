using Sejily.API.Models.Enums.Medications;

namespace Sejily.API.Models.Helpers;


public class Duration
{
    public int Amount { get; set; }
    public DurationUnit Unit { get; set; }
}