﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace SpellingMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<YourListPage>();
            builder.Services.AddTransient<SpellingPage>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ResultsPage>();
            builder.Services.AddSingleton<SpellingsDatabase>();
            builder.Services.AddSingleton<Spellcheker>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}