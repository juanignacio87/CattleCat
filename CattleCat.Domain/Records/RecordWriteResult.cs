using System;

namespace CattleCat.Domain.Records
{
    public enum RecordWriteStatus
    {
        Success, InvalidRequest, InvalidQuantity, FutureDate,
        UserNotFound, UserInactive, LotNotFound, LotInactive,
        FarmNotFound, FarmInactive, CategoryNotFound, OperationNotFound,
        RecordUnavailable, StockConflict, TechnicalError
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
        public static RecordWriteResult Success() => new RecordWriteResult(RecordWriteStatus.Success, null, null);
        public static RecordWriteResult Failure(RecordWriteStatus status) => new RecordWriteResult(status, null, null);
        public static RecordWriteResult Conflict(DateTime date, int stock)
            => new RecordWriteResult(RecordWriteStatus.StockConflict, date, stock);
    }
}
