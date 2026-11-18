using Sejily.API.Services;

namespace Sejily.API.Infrastructure;


class MedicationClassifier : IMedicationClassifier
{
    public string ToRxNorm(string diesaseName)
    {
        return "RX10359383";
    }
}