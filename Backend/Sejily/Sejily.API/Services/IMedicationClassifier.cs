namespace Sejily.API.Services;


public interface IMedicationClassifier
{
    public string ToRxNorm(string diesaseName);
}