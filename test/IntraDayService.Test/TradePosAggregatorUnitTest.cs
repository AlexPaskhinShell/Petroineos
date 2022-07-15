using IntraDayService.Lib;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IntraDayService
{
    /// <summary>
    /// For test task purposes has been provided subset of the unit tests
    /// </summary>
    public class TradePosAggregatorUnitTest
    {
        private readonly TradeAggregatorOptions _tradeAggregatorOptions;
        private readonly Mock<ITradeAggregatorService> _tradeAggretorService;

        public TradePosAggregatorUnitTest()
        {
            _tradeAggregatorOptions = new TradeAggregatorOptions();
            _tradeAggretorService = new Mock<ITradeAggregatorService>();
        }

        [Fact]
        public async Task StartAsyncShouldBeCalledCupture2xTimes()
        {
            // Arrange
            var sut = new TradePosAggregator(
                NullLogger<TradePosAggregator>.Instance,
                _tradeAggretorService.Object,
                _tradeAggregatorOptions);
           // Act

            await sut.StartAsync();

            await Task.Delay(_tradeAggregatorOptions.Delay 
                + _tradeAggregatorOptions.Period 
                + _tradeAggregatorOptions.Period/2);
            await sut.StopAsync();

            // Assert

            _tradeAggretorService.Verify(v => v.GetTradesAsync(It.IsAny<DateTime>()), Times.Exactly(2));

        }

    }
}