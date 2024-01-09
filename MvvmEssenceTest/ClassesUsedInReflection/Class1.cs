namespace TestProject.ClassesUsedInReflection.Ns1;

interface IInterface1
{
}

internal class Class1 : IInterface1
{
}

[RegisterAsSingleton]
[IgnoreInterfaceForRegistration]
internal class Class1Ni : IInterface1
{
}