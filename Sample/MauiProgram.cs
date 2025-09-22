using Brain2CPU.ApiClient;
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


        builder.Services.AddHttpClient("InsecureClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    // Skip server certificate validation
                    return true;
                }
            });

        builder.Services.AddHttpClient()
            .AddSingleton<ApiClient>();

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