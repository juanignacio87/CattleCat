using System;

namespace CattleCat.Domain.Records
{
    public enum RecordWriteStatus
    {
        Success, InvalidRequest, InvalidQuantity, FutureDate,
        UserNotFound, UserInactive, LotNotFound, LotInactive,
        FarmNotFound, FarmInactive, CategoryNotFound, OperationNotFound,
        RecordUnavailable, StockConflict, TechnicalError, OperationExceedsStock, CountMismatch, UnsupportedOperation
    }

    public sealed class RecordWriteResult
    {
        private RecordWriteResult(RecordWriteStatus status, DateTime? conflictDate, int? projectedStock)
        {
            Status = status;
            ConflictDate = conflictDate;
            ProjectedStock = projectedStock;
        }
        public RecordWriteStatus Status { get; }
        public bool IsSuccess => Status == RecordWriteStatus.Success;
        public DateTime? ConflictDate { get; }
        public int? ProjectedStock { get; }
        public int? OperationId { get; private set; }
        public int? RequestedQuantity { get; private set; }
        public int? AvailableStock { get; private set; }
        public int? LotId { get; private set; }
        public int? CategoryId { get; private set; }
        public bool IsRetroactiveConflict { get; private set; }
        public static RecordWriteResult TimelineConflict(Stock.StockTimelineResult conflict, int lotId, int categoryId, DateTime changedDate)
        {
            var status = conflict.Conflict == Stock.StockTimelineConflict.CountMismatch ? RecordWriteStatus.CountMismatch :
                conflict.Conflict == Stock.StockTimelineConflict.OperationExceedsStock ? RecordWriteStatus.OperationExceedsStock : RecordWriteStatus.StockConflict;
            return new RecordWriteResult(status, conflict.ConflictDate, conflict.ProjectedStock)
            {
                OperationId = conflict.OperationId, RequestedQuantity = conflict.RequestedQuantity,
                AvailableStock = conflict.AvailableStock, LotId = lotId, CategoryId = categoryId,
                IsRetroactiveConflict = conflict.ConflictDate.Value > changedDate.Date
            };
        }
        public static RecordWriteResult Success() => new RecordWriteResult(RecordWriteStatus.Success, null, null);
        public static RecordWriteResult Failure(RecordWriteStatus status) => new RecordWriteResult(status, null, null);
        public static RecordWriteResult Conflict(DateTime date, int stock)
            => new RecordWriteResult(RecordWriteStatus.StockConflict, date, stock);
    }
}
