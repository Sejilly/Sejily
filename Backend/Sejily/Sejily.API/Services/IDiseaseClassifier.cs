namespace Sejily.API.Services;


public interface IDiseaseClassifier
{
    public string ToIcdCode(string diesaseName);
}