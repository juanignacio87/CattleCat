using System;

namespace CattleCat.Domain.Stock
{
    public sealed class StockTimelineResult
    {
        internal StockTimelineResult(bool isValid, DateTime? conflictDate, int projectedStock)
        {
            IsValid = isValid;
            ConflictDate = conflictDate;
            ProjectedStock = projectedStock;
        }

        public bool IsValid { get; }
        public DateTime? ConflictDate { get; }
        /// <summary>Balance at the first conflict, or final balance when valid.</summary>
        public int ProjectedStock { get; }
    }
}
