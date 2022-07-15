using PowerTradeService;

namespace IntraDayService.Lib
{
    public interface ITradeAggregatorService
    {
        Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date);
        IEnumerable<LocalTimeVolume> AggregateTrades(IEnumerable<PowerTrade> trades, TimeSpan localtimeOffset);
        void AggregateCsvTradesReport(IEnumerable<LocalTimeVolume> localTimeVolumes, DateTime dateTime, TextWriter textWriter = null);
    }
}
