using Brain2CPU.MvvmEssence;

namespace Sample.ViewModels.Validators;

public  class NonZeroDoubleAsStringValidator : IValidator
{
    public bool IsValid(object o) => double.TryParse((string)o, out var d) && Math.Abs(d) > 1E-6;
}
