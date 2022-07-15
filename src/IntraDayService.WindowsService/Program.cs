using IntraDayService.Lib;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PowerTradeService;

namespace IntraDayService.WindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHostBuilder hostbuilder = WebHost.CreateDefaultBuilder(args);
            hostbuilder.ConfigureServices((context, services) =>
            {
                var cfg = context.Configuration.GetSection(nameof(TradeAggregatorOptions)).Get<TradeAggregatorOptions>();
                services.AddHostedService<TradePosAggregator>();
                services.AddSingleton(cfg);
                services.AddSingleton<IPowerService, PowerService>();
                services.AddSingleton<ITradeAggregatorService, TradeAggregatorService>();
            });

            var host = hostbuilder.Build();
            var webHostService = new WebHostService(host);
        }
    }
}