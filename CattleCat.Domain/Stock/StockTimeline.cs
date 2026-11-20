using System;
using System.Collections.Generic;
using System.Linq;

namespace CattleCat.Domain.Stock
{
    public static class StockTimeline
    {
        /// <summary>
        /// Evaluates one lot/category from zero, at daily close (time of day is ignored).
        /// Callers simulate edits/deletions by excluding the original movement before calling.
        /// </summary>
        public static StockTimelineResult Evaluate(IEnumerable<StockMovement> movements)
        {
            if (movements == null)
                throw new ArgumentNullException(nameof(movements));

            int stock = 0;
            var days = movements.Where(m => m.IsActive)
                .GroupBy(m => m.Date.Date).OrderBy(day => day.Key);

            foreach (var day in days)
            {
                // Preserve legacy Int32 arithmetic: Sum checks daily overflow;
                // the running balance historically used unchecked addition.
                stock = unchecked(stock + day.Sum(m => m.SignedQuantity));
                if (stock < 0)
                    return new StockTimelineResult(false, day.Key, stock);
            }

            return new StockTimelineResult(true, null, stock);
        }
    }
}
