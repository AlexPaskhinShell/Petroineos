using PowerTradeService;

namespace IntraDayService.Lib
{
    public class TradeAggregatorService : ITradeAggregatorService
    {
        private readonly IPowerService _powerService;
        private readonly TradeAggregatorOptions _tradeAggregatorOptions;

        public TradeAggregatorService(IPowerService powerService,
            TradeAggregatorOptions tradeAggregatorOptions)
        {
            _powerService = powerService;
            _tradeAggregatorOptions = tradeAggregatorOptions;
        }

        public Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
        {
            return _powerService.GetTradesAsync(date);
        }

        public IEnumerable<LocalTimeVolume> AggregateTrades(IEnumerable<PowerTrade> trades, TimeSpan localTimeOffset)
        {

            return trades.SelectMany(s => s.Periods)
                .GroupBy(p => p.Period)
                .OrderBy(o => o.Key)
                .Select(s =>
                new LocalTimeVolume
                {
                    Time = TimeSpan.FromHours(ConvertPeriodToUtcHour(s.Key)) + localTimeOffset,
                    Volume = s.Sum(p => p.Volume)
                });
        }

        public int ConvertPeriodToUtcHour(int period)
        {
            return period == 1 ? 23 : period - 2 + 24;
        }
    
        public void AggregateCsvTradesReport(IEnumerable<LocalTimeVolume> localTimeVolumes,
            DateTime dateTime,
            TextWriter textWriter)
        {
            if (textWriter == null)
            {
                var fileName = $"PowerPosition_{dateTime.ToString("yyyyMMdd_HHmm")}.csv";

                if (!Directory.Exists(_tradeAggregatorOptions.CvsLocationPath))
                {
                    Directory.CreateDirectory(_tradeAggregatorOptions.CvsLocationPath);
                }

                fileName = Path.Combine(_tradeAggregatorOptions.CvsLocationPath, fileName);
                textWriter = new StreamWriter(fileName);
            }

            using (textWriter)
            {
                textWriter.WriteLine("Local Time,Volume");
                foreach (var row in localTimeVolumes)
                {
                    textWriter.Write(row.Time.ToString("hh\\:mm") + ",");
                    textWriter.WriteLine(row.Volume.ToString("######.###"));
                }
            }
        }
    }
}
