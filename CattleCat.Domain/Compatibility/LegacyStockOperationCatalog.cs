using CattleCat.Domain.Stock;

namespace CattleCat.Compatibility
{
    /// <summary>
    /// Boundary adapter for existing operation IDs. The engine uses behavior, not display names.
    /// Balance classification remains compatible; write behavior is explicit and independent of labels.
    /// </summary>
    public static class LegacyStockOperationCatalog
    {
        public const int Entry = 8;
        public const int Birth = 12;
        public const int Death = 11;
        public const int Exit = 14;

        public const int FootAndMouth = 1;
        public const int Antiparasitic = 2;
        public const int Brucellosis = 3;
        public const int Castration = 4;
        public const int Count = 5;
        public const int Weaning = 6;
        public const int Ultrasound = 7;
        public const int Insemination = 9;
        public const int Ir9 = 10;
        public const int Reproductive = 13;
        public const int Keratitis = 15;
        public const int Scabies = 16;
        public const int Antidiarrheal = 17;
        public const int Carbuncle = 18;

        public static StockOperationBehavior Behavior(int operationId)
        {
            switch (operationId)
            {
                case Entry: case Birth: return StockOperationBehavior.Positive;
                case Death: case Exit: return StockOperationBehavior.Negative;
                case Count: return StockOperationBehavior.Count;
                case FootAndMouth: case Antiparasitic: case Brucellosis: case Castration:
                case Weaning: case Ultrasound: case Insemination: case Ir9:
                case Reproductive: case Keratitis: case Scabies: case Antidiarrheal: case Carbuncle:
                    return StockOperationBehavior.ExistingAnimals;
                default: return StockOperationBehavior.Unknown;
            }
        }

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
