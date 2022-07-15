using IntraDayService.Lib;
using PowerTradeService;

namespace IntraDayService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var cfg = context.Configuration.GetSection(nameof(TradeAggregatorOptions)).Get<TradeAggregatorOptions>();
                    services.AddHostedService<TradePosAggregator>();
                    services.AddSingleton(cfg);
                    services.AddSingleton<IPowerService,PowerService>();
                    services.AddSingleton<ITradeAggregatorService, TradeAggregatorService>();
                })
                .Build();

            host.Run();
        }
    }
}