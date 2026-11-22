using System;
namespace CattleCat.Domain.Stock
{
    public sealed class StockTimelineResult
    {
        internal StockTimelineResult(bool isValid, DateTime? conflictDate, int projectedStock,
            StockTimelineConflict conflict = StockTimelineConflict.None, int? operationId = null,
            int? requestedQuantity = null, int? availableStock = null)
        {
            IsValid = isValid; ConflictDate = conflictDate; ProjectedStock = projectedStock;
            Conflict = conflict; OperationId = operationId; RequestedQuantity = requestedQuantity; AvailableStock = availableStock;
        }
        public bool IsValid { get; }
        public DateTime? ConflictDate { get; }
        public int ProjectedStock { get; }
        public StockTimelineConflict Conflict { get; }
        public int? OperationId { get; }
        public int? RequestedQuantity { get; }
        public int? AvailableStock { get; }
    }
}
