using Polly;
using PowerTradeService;

namespace IntraDayService.Lib
{
    public class TradePosAggregator : IHostedService
    {
        private readonly ILogger<TradePosAggregator> _logger;
        private readonly ITradeAggregatorService _tradeAggretorService;
        private readonly TradeAggregatorOptions _tradeAggretorOptions;
        private readonly CancellationTokenSource _stopping;
        private Timer _timer;
        private CancellationTokenSource _runTokenSource;

        public TradePosAggregator(ILogger<TradePosAggregator> logger,
            ITradeAggregatorService tradeAggretorService,
            TradeAggregatorOptions tradeAggretorOptions)
        {
            _logger = logger;
            _tradeAggretorService = tradeAggretorService;
            _tradeAggretorOptions = tradeAggretorOptions;
            _stopping = new CancellationTokenSource();

        }

        public bool IsStopping => _stopping.IsCancellationRequested;

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"[{nameof(TradePosAggregator)}] - Start");
            _timer = CreateTimer(Timer_Tick, null, dueTime: _tradeAggretorOptions.Delay, period: _tradeAggretorOptions.Period);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"[{nameof(TradePosAggregator)}] - Stop");
            try
            {
                _stopping.Cancel();
            }
            catch
            {
                // Ignore exceptions thrown as a result of a cancellation.
            }

            _timer?.Dispose();
            _timer = null;

            return Task.CompletedTask;
        }

        // Yes, async void.
        // We need to be async.
        // We need to be void.
        // We handle the exceptions in RunAsync
        private async void Timer_Tick(object state)
        {
            // Forcibly yield - we want to unblock the timer thread.
            await Task.Yield();
            await RunAsync().ConfigureAwait(false);
        }

        public async Task RunAsync()
        {

            var policy = Policy.Handle<PowerServiceException>()
                .WaitAndRetryAsync(_tradeAggretorOptions.RepeatInCaseOfErrorTimes,
                _ => TimeSpan.FromSeconds(5),
                (e,t) => { _logger.LogInformation($"[{nameof(TradePosAggregator)}] - Repeat trades capture due to exception {e.GetType().Name}"); });

            CancellationTokenSource cancellation = null;
            try
            {
                var timeout = _tradeAggretorOptions.Timeout;

                cancellation = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
                _runTokenSource = cancellation;
                cancellation.CancelAfter(timeout);

                var captureTimeLocal = DateTime.Now;
                var captureUtcOffset = captureTimeLocal - captureTimeLocal.ToUniversalTime();

                _logger.LogInformation($"[{nameof(TradePosAggregator)}] -  Start trades capture cycle");

                await policy.ExecuteAsync(async () =>
                   {
                       await ProcessTrades(captureTimeLocal, captureUtcOffset);
                   });

                _logger.LogInformation($"[{nameof(TradePosAggregator)}] -  Complete trades capture cycle");
            }
            catch (OperationCanceledException exc) when (IsStopping)
            {
                // This is a cancellation - if the app is shutting down we want to ignore it. Otherwise, it's
                // a timeout and we want to log it.
                _logger.LogError(exc, $"[{nameof(TradePosAggregator)}] - Timeout {_tradeAggretorOptions.Timeout} expired ");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(TradePosAggregator)}] - Exception thrown -  {ex.Message}");
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        private async Task ProcessTrades(DateTime captureTimeLocal, TimeSpan captureUtcOffset)
        {
            var powertrades = await _tradeAggretorService.GetTradesAsync(captureTimeLocal);

            if (powertrades.Any())
            {
                var listofAggValues = _tradeAggretorService.AggregateTrades(powertrades, captureUtcOffset);
                _tradeAggretorService.AggregateCsvTradesReport(listofAggValues, captureTimeLocal);
            }
            else
            {
                _logger.LogWarning($"[{nameof(TradePosAggregator)}] -  No trades information");
            }
        }

        public static Timer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
            bool restoreFlow = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restoreFlow = true;
                }

                return new Timer(callback, state, dueTime, period);
            }
            finally
            {
                // Restore the current ExecutionContext
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
    }
}