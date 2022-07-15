namespace PowerTradeService
{

    public class PowerTrade
    {
        public DateTime Date { get; internal set; }

        public PowerPeriod[] Periods { get; internal set; }

        public static PowerTrade Create(DateTime date, int numberOfPeriods) => new PowerTrade()
        {
            Date = date,
            Periods = Enumerable.Range(1, numberOfPeriods).Select(period => new PowerPeriod()
            {
                Period = period
            }).ToArray()
        };
    }

}
