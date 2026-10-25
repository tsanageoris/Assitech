namespace Assitech.App;
public static class MauiProgram{public static MauiApp CreateMauiApp(){var b=MauiApp.CreateBuilder();b.UseMauiApp<App>();
 b.Services.AddSingleton<Api>();b.Services.AddTransient<LoginPage>().AddTransient<TicketsPage>().AddTransient<NewTicketPage>().AddTransient<ChatPage>();return b.Build();}}
