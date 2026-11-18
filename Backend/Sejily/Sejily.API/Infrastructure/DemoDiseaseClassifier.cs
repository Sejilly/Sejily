using Sejily.API.Services;

namespace Sejily.API.Infrastructure;


class DemoDiseaseClassifier : IDiseaseClassifier
{
    public string ToIcdCode(string diesaseName)
    {
        return "S80.02XA";
    }
}