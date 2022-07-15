
namespace PowerTradeService
{
    public class PowerService : IPowerService
    {
        private static readonly TimeZoneInfo GmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        private readonly Random _random = new Random();
        private readonly string _mode;

        public PowerService() => _mode = Environment.GetEnvironmentVariable("SERVICE_MODE") ?? "Normal";

        public IEnumerable<PowerTrade> GetTrades(DateTime date)
        {
            CheckThrowError();
            Thread.Sleep(GetDelay());
            return GetTradesImpl(date);
        }

        public async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
        {
            CheckThrowError();
            await Task.Delay(GetDelay());
            return GetTradesImpl(date);
        }

        private void CheckThrowError()
        {
            if (_mode == "Error" || _random.Next(10) == 9)
                throw new PowerServiceException("Error retrieving power volumes");
        }

        private TimeSpan GetDelay() => TimeSpan.FromSeconds(_random.NextDouble() * 5.0);

        private IEnumerable<PowerTrade> GetTradesImpl(DateTime date)
        {
            DateTime dateTime1 = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(-1.0);
            DateTime dateTime2 = dateTime1.AddDays(1.0);
            DateTime utc1 = TimeZoneInfo.ConvertTimeToUtc(dateTime1, GmtTimeZoneInfo);
            TimeZoneInfo gmtTimeZoneInfo = GmtTimeZoneInfo;
            DateTime utc2 = TimeZoneInfo.ConvertTimeToUtc(dateTime2, gmtTimeZoneInfo);
            int numberOfPeriods = (int)utc2.Subtract(utc1).TotalHours;
            PowerTrade[] array = Enumerable.Range(0, _mode == "Test" ? 2 : _random.Next(1, 20)).Select(_ => PowerTrade.Create(date, numberOfPeriods)).ToArray();
            int index = 0;
            for (DateTime dateTime3 = utc1; dateTime3 < utc2; dateTime3 = dateTime3.AddHours(1.0))
            {
                foreach (PowerTrade powerTrade in array)
                {
                    double num = _mode == "Test" ? index + 1 : _random.NextDouble() * 1000.0;
                    powerTrade.Periods[index].Volume = num;
                }
                ++index;
            }
            return array;
        }
    }

}
