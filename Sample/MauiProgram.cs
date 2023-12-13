using Brain2CPU.MvvmEssence;
using Microsoft.Extensions.Logging;

namespace Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        AutoRegisterServices(builder);

        return builder.Build();
    }

    private static void AutoRegisterServices(MauiAppBuilder builder)
    {
        var discover = new DiscoverComponents(typeof(MauiProgram).Assembly,
            type =>
            {
                if (type.Namespace.Equals("Sample.Views") && type.Name.EndsWith("Page"))
                    return ClassRegistrationOption.AsTransient;
                
                if (type.Namespace.Equals("Sample.ViewModels") && type.Name.EndsWith("ViewModel"))
                    return ClassRegistrationOption.AsTransient;

                if (type.Namespace.Equals("Services"))
                    return ClassRegistrationOption.AsSingleton;

                return ClassRegistrationOption.Skip;
            });

        discover.RegisterItems(sd => builder.Services.AddSingleton(sd),
            td => builder.Services.AddTransient(td),
            (sc, si) => builder.Services.AddSingleton(si, sc),
            (tc, ti) => builder.Services.AddTransient(ti, tc));
    }
}