namespace PowerTradeService
{
    public interface IPowerService
    {
        IEnumerable<PowerTrade> GetTrades(DateTime date);
        Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date);
    }

}
