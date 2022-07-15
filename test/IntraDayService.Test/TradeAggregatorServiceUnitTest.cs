using FluentAssertions;
using IntraDayService.Lib;
using Moq;
using PowerTradeService;

namespace IntraDayService
{
    /// <summary>
    /// For test task purposes has been provided subset of the unit tests
    /// </summary>
    public class TradeAggregatorServiceUnitTest
    {
        private readonly Mock<IPowerService> _powerServiceMock;
        private readonly TradeAggregatorOptions _tradeAggregatorOptions;

        public TradeAggregatorServiceUnitTest()
        {
            _powerServiceMock = new Mock<IPowerService>();
            _tradeAggregatorOptions = new TradeAggregatorOptions();
        }

        [Fact]
        public async Task TestGetTradesAsyncShouldBeCalled()
        {
            // Arrange
            _powerServiceMock.Setup(v => v.GetTradesAsync(It.IsAny<DateTime>())).Returns(Task.FromResult<IEnumerable<PowerTrade>>(null));

            var sut = new TradeAggregatorService(_powerServiceMock.Object, _tradeAggregatorOptions);

            // Act
            await sut.GetTradesAsync(DateTime.Now);

            // Assert
            _powerServiceMock.Verify(v => v.GetTradesAsync(It.IsAny<DateTime>()), Times.Once);
        }


        [Fact]
        public void TestAggregateTradesShouldBeAggregated()
        {
            // Arrange
            var sut = new TradeAggregatorService(_powerServiceMock.Object, _tradeAggregatorOptions);

            IEnumerable<PowerTrade> trades = GenerateTrades(100);

            // Act
            var results = sut.AggregateTrades(trades, TimeSpan.Zero);

            // Assert
            results.Should().NotBeNullOrEmpty();
            results.Should().HaveCount(24);
            results.All(v => v.Volume == 200);
        }


        [Theory]
        [InlineData(1, 23)]
        [InlineData(2, 24)]
        [InlineData(24, 24 + 22)]
        public void TestConvertPeriodToUtcHourShouldBeAggregated(int data, int expected)
        {
            // Arrange
            var sut = new TradeAggregatorService(_powerServiceMock.Object, _tradeAggregatorOptions);

            // Act
            var res = sut.ConvertPeriodToUtcHour(data);

            // Assert
            res.Should().Be(expected);
        }

        [Fact]
        public void TestAggregateCsvTradesReportShouldCreateFile()
        {
            // Arrange
            StringWriter stringWriter = new StringWriter();

            var sut = new TradeAggregatorService(_powerServiceMock.Object, _tradeAggregatorOptions);

            List<LocalTimeVolume> localTimeVolumes = new List<LocalTimeVolume>
            {
                new LocalTimeVolume
                {
                Time = TimeSpan.Zero,
                Volume = 0 }
            };

            // Act
            sut.AggregateCsvTradesReport(localTimeVolumes, DateTime.Now, stringWriter);
            var res = stringWriter.ToString();

            // Assert
            res.Should().StartWith("Local Time,Volume");
        }

        private IEnumerable<PowerTrade> GenerateTrades(int seed)
        {
            PowerTrade[] array = Enumerable.Range(0, 2).Select(_ => PowerTrade.Create(DateTime.UtcNow, 24)).ToArray();

            foreach (PowerTrade powerTrade in array)
            {
                foreach (var per in powerTrade.Periods)
                {
                    per.Volume = seed;
                }
            }
            return array;
        }

    }
}