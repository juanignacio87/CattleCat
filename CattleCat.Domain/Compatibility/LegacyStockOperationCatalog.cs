using CattleCat.Domain.Stock;

namespace CattleCat.Compatibility
{
    /// <summary>
    /// Boundary adapter for existing database IDs. The stock engine never reads these IDs.
    /// Unknown operations, counts and treatments retain their legacy neutral effect.
    /// </summary>
    public static class LegacyStockOperationCatalog
    {
        public const int Entry = 8;
        public const int Birth = 12;
        public const int Death = 11;
        public const int Exit = 14;

        public static StockEffect Classify(int operationId)
        {
            switch (operationId)
            {
                case Entry:
                case Birth:
                    return StockEffect.Positive;
                case Death:
                case Exit:
                    return StockEffect.Negative;
                default:
                    return StockEffect.Neutral;
            }
        }

        public static int SignedQuantity(int operationId, int quantity)
        {
            var effect = Classify(operationId);
            return effect == StockEffect.Positive ? quantity :
                effect == StockEffect.Negative ? -quantity : 0;
        }

        // Return a fresh array so consumers cannot change the catalog.
        public static int[] GetMovementIds() => new[] { Entry, Birth, Death, Exit };
    }
}
