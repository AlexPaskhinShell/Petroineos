namespace IntraDayService.Lib
{
    public sealed class TradeAggregatorOptions
    {
        private TimeSpan _delay;
        private TimeSpan _period;

        public TradeAggregatorOptions()
        {
            _delay = TimeSpan.FromSeconds(5);
            _period = TimeSpan.FromSeconds(30);
        }

        public TimeSpan Delay
        {
            get => _delay;
            set
            {
                if (value == System.Threading.Timeout.InfiniteTimeSpan)
                {
                    throw new ArgumentException($"The {nameof(Delay)} must not be infinite.", nameof(value));
                }

                _delay = value;
            }
        }

        public TimeSpan Period
        {
            get => _period;
            set
            {
                if (value < TimeSpan.FromSeconds(1))
                {
                    throw new ArgumentException($"The {nameof(Period)} must be greater than or equal to one second.", nameof(value));
                }

                if (value == System.Threading.Timeout.InfiniteTimeSpan)
                {
                    throw new ArgumentException($"The {nameof(Period)} must not be infinite.", nameof(value));
                }

                _period = value;
            }
        }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        public string CvsLocationPath { get; set; }

        public int RepeatInCaseOfErrorTimes { get; set; } = 3;
    }
}