using Brain2CPU.MvvmEssence;

namespace Sample.Services;

[RegisterAsSingleton]
public class CalculationService
{
    public double Sqrt(string v)
    {
        var nr = double.Parse(v);

        // the template takes care of the exception if handler is defined
        if (nr < 0)
            throw new ArgumentException("Cannot extract square root from a negative number.");

        return Math.Sqrt(nr);
    }
}