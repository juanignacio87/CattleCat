using System;

namespace CattleCat.Domain.Stock
{
    /// <summary>A classified operation in one lot/category. Optional operation ID is conflict metadata.</summary>
    public sealed class StockMovement
    {
        // Legacy balance-only projection, retained for read-only compatibility.
        // All production writes must use the behavior-aware overload through RecordWriter.
        public StockMovement(DateTime date, int quantity, bool isActive, StockEffect effect)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            if (effect != StockEffect.Positive && effect != StockEffect.Negative && effect != StockEffect.Neutral)
                throw new ArgumentOutOfRangeException(nameof(effect));

            Behavior = effect == StockEffect.Positive ? StockOperationBehavior.Positive :
                effect == StockEffect.Negative ? StockOperationBehavior.Negative : StockOperationBehavior.BalanceOnlyNeutral;
            Date = date;
            Quantity = quantity;
            IsActive = isActive;
            Effect = effect;
        }

        public StockMovement(DateTime date, int quantity, bool isActive, StockOperationBehavior behavior, int operationId)
            : this(date, quantity, isActive, behavior == StockOperationBehavior.Positive ? StockEffect.Positive :
                behavior == StockOperationBehavior.Negative ? StockEffect.Negative : StockEffect.Neutral)
        {
            if (behavior != StockOperationBehavior.Positive && behavior != StockOperationBehavior.Negative &&
                behavior != StockOperationBehavior.ExistingAnimals && behavior != StockOperationBehavior.Count)
                throw new ArgumentOutOfRangeException(nameof(behavior));
            Behavior = behavior;
            OperationId = operationId;
        }

        public StockOperationBehavior Behavior { get; }
        public int? OperationId { get; }
        public DateTime Date { get; }
        public int Quantity { get; }
        public bool IsActive { get; }
        public StockEffect Effect { get; }
        public int SignedQuantity => Effect == StockEffect.Positive ? Quantity :
            Effect == StockEffect.Negative ? -Quantity : 0;
    }
}
