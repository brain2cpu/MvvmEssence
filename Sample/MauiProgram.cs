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
                if (type.Namespace is not ("Sample.Views" or "Sample.ViewModels"))
                    return ClassRegistrationOption.Skip;

                if (!type.Name.EndsWith("Page") && !type.Name.EndsWith("ViewModel"))
                    return ClassRegistrationOption.Skip;

                // default mode
                return ClassRegistrationOption.AsTransient;
            });

        discover.RegisterItems(s => builder.Services.AddSingleton(s),
            t => builder.Services.AddTransient(t));
    }
}