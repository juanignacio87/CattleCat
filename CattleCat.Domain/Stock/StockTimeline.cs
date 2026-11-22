using System;
using System.Collections.Generic;
using System.Linq;
namespace CattleCat.Domain.Stock
{
    public static class StockTimeline
    {
        // One lot/category. Daily convention: incomes, observations/counts, outcomes.
        // No input order or time component may decide whether an observation is valid.
        public static StockTimelineResult Evaluate(IEnumerable<StockMovement> movements)
        {
            if (movements == null) throw new ArgumentNullException(nameof(movements));
            int stock = 0;
            var days = movements.Where(m => m.IsActive).GroupBy(m => m.Date.Date).OrderBy(g => g.Key);
            foreach (var day in days)
            {
                stock = unchecked(stock + day.Where(m => m.Effect == StockEffect.Positive).Sum(m => m.Quantity));
                if (stock < 0) return new StockTimelineResult(false, day.Key, stock, StockTimelineConflict.NegativeStock);
                foreach (var observation in day.Where(m => m.Behavior == StockOperationBehavior.Count ||
                    m.Behavior == StockOperationBehavior.ExistingAnimals).OrderBy(m => m.OperationId).ThenBy(m => m.Quantity))
                {
                    var conflict = observation.Behavior == StockOperationBehavior.Count
                        ? (observation.Quantity == stock ? StockTimelineConflict.None : StockTimelineConflict.CountMismatch)
                        : (stock > 0 && observation.Quantity <= stock ? StockTimelineConflict.None : StockTimelineConflict.OperationExceedsStock);
                    if (conflict != StockTimelineConflict.None)
                        return new StockTimelineResult(false, day.Key, stock, conflict,
                            observation.OperationId, observation.Quantity, stock);
                }
                var outcomes = day.Where(m => m.Effect == StockEffect.Negative).OrderBy(m => m.OperationId).ThenBy(m => m.Quantity).ToList();
                int available = stock;
                stock = unchecked(stock + outcomes.Sum(m => -m.Quantity));
                if (stock < 0)
                {
                    // Closing balance decides validity. Locate a deterministic offending outcome
                    // only to report its quantity and the stock immediately available to it.
                    foreach (var outcome in outcomes)
                    {
                        if (outcome.Quantity > available)
                            return new StockTimelineResult(false, day.Key, stock, StockTimelineConflict.NegativeStock,
                                outcome.OperationId, outcome.Quantity, available);
                        available -= outcome.Quantity;
                    }
                    return new StockTimelineResult(false, day.Key, stock, StockTimelineConflict.NegativeStock);
                }
            }
            return new StockTimelineResult(true, null, stock);
        }
    }
}
