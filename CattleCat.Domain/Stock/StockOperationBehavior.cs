namespace CattleCat.Domain.Stock
{
    public enum StockOperationBehavior
    {
        Unknown, Positive, Negative, ExistingAnimals, Count,
        // Compatibility for callers that only ask for the historical balance projection.
        BalanceOnlyNeutral
    }
    public enum StockTimelineConflict { None, NegativeStock, OperationExceedsStock, CountMismatch }
}
