using System;

namespace CattleCat.Domain.Stock
{
    /// <summary>A classified movement in one lot/category. No persistence identifiers.</summary>
    public sealed class StockMovement
    {
        public StockMovement(DateTime date, int quantity, bool isActive, StockEffect effect)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            if (effect != StockEffect.Positive && effect != StockEffect.Negative && effect != StockEffect.Neutral)
                throw new ArgumentOutOfRangeException(nameof(effect));

            Date = date;
            Quantity = quantity;
            IsActive = isActive;
            Effect = effect;
        }

        public DateTime Date { get; }
        public int Quantity { get; }
        public bool IsActive { get; }
        public StockEffect Effect { get; }
        public int SignedQuantity => Effect == StockEffect.Positive ? Quantity :
            Effect == StockEffect.Negative ? -Quantity : 0;
    }
}
