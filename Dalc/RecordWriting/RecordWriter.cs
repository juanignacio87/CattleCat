using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CattleCat.Compatibility;
using CattleCat.Domain.Records;
using CattleCat.Domain.Stock;
using Entities;

namespace Dalc.RecordWriting
{
    public sealed class RecordWriter
    {
        private readonly Func<IRecordWriteSession> openSession;
        private readonly Func<DateTime> today;
        public RecordWriter(Func<IRecordWriteSession> openSession, Func<DateTime> today = null)
        {
            this.openSession = openSession ?? throw new ArgumentNullException(nameof(openSession));
            this.today = today ?? (() => DateTime.Today);
        }

        public RecordWriteResult Create(Record record) => CreateMany(new[] { record });

        public RecordWriteResult CreateMany(IEnumerable<Record> records) => Execute(session =>
        {
            if (records == null) return Fail(RecordWriteStatus.InvalidRequest);
            var candidates = new List<Record>();
            foreach (var record in records)
            {
                if (record == null || record.Id != 0) return Fail(RecordWriteStatus.InvalidRequest);
                var candidate = Copy(record);
                candidate.Activo = 1;
                var validation = Validate(session, candidate);
                if (!validation.IsSuccess) return validation;
                candidates.Add(candidate);
            }
            var timeline = ValidateTimelines(session, candidates, null);
            if (!timeline.IsSuccess) return timeline;
            foreach (var candidate in candidates) session.Add(candidate);
            return RecordWriteResult.Success();
        });

        public RecordWriteResult Update(Record record) => Execute(session =>
        {
            if (record == null) return Fail(RecordWriteStatus.InvalidRequest);
            var candidate = Copy(record);
            var original = session.FindRecord(candidate.Id);
            if (original == null || original.Activo != 1) return Fail(RecordWriteStatus.RecordUnavailable);
            candidate.Activo = 1;
            var validation = Validate(session, candidate);
            if (!validation.IsSuccess) return validation;
            var timeline = ValidateTimelines(session, new[] { candidate }, original);
            if (!timeline.IsSuccess) return timeline;
            session.Update(candidate);
            return RecordWriteResult.Success();
        });

        public RecordWriteResult Deactivate(int id) => Execute(session =>
        {
            var original = session.FindRecord(id);
            if (original == null || original.Activo != 1) return Fail(RecordWriteStatus.RecordUnavailable);
            var validation = Validate(session, original);
            if (!validation.IsSuccess) return validation;
            var timeline = ValidateTimelines(session, new Record[0], original);
            if (!timeline.IsSuccess) return timeline;
            session.Deactivate(id);
            return RecordWriteResult.Success();
        });

        private RecordWriteResult Execute(Func<IRecordWriteSession, RecordWriteResult> operation)
        {
            try
            {
                using (var session = openSession())
                using (var transaction = session.BeginTransaction(IsolationLevel.Serializable))
                {
                    // Every validation read and every mutation uses this session/context/transaction.
                    var result = operation(session);
                    if (!result.IsSuccess) return result;
                    session.SaveChanges();
                    transaction.Commit();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Record write failed: {0}", ex);
                return Fail(RecordWriteStatus.TechnicalError);
            }
        }

        private RecordWriteResult Validate(IRecordWriteSession session, Record record)
        {
            if (record.Total <= 0) return Fail(RecordWriteStatus.InvalidQuantity);
            if (record.Date.Date == DateTime.MinValue) return Fail(RecordWriteStatus.InvalidRequest);
            if (record.Date.Date > today().Date) return Fail(RecordWriteStatus.FutureDate);
            var user = session.FindUser(record.UserId);
            if (user == null) return Fail(RecordWriteStatus.UserNotFound);
            if (user.Activo != 1) return Fail(RecordWriteStatus.UserInactive);
            var lot = session.FindLot(record.LotId);
            if (lot == null) return Fail(RecordWriteStatus.LotNotFound);
            if (lot.Active != 1) return Fail(RecordWriteStatus.LotInactive);
            var farm = session.FindFarm(lot.FarmId);
            if (farm == null) return Fail(RecordWriteStatus.FarmNotFound);
            if (farm.Activo != 1) return Fail(RecordWriteStatus.FarmInactive);
            if (!session.CategoryExists(record.CategoryId)) return Fail(RecordWriteStatus.CategoryNotFound);
            if (!session.OperationExists(record.OperationId)) return Fail(RecordWriteStatus.OperationNotFound);
            if (LegacyStockOperationCatalog.Behavior(record.OperationId) == StockOperationBehavior.Unknown)
                return Fail(RecordWriteStatus.UnsupportedOperation);
            return RecordWriteResult.Success();
        }

        private static RecordWriteResult ValidateTimelines(IRecordWriteSession session, IEnumerable<Record> replacements, Record original)
        {
            var candidates = replacements.ToList();
            var groups = candidates.Select(r => new { r.LotId, r.CategoryId }).ToList();
            if (original != null) groups.Add(new { original.LotId, original.CategoryId });
            RecordWriteResult firstConflict = null;
            // Stable ordering reduces opposite lock ordering when moving between groups.
            foreach (var group in groups.Distinct().OrderBy(g => g.LotId).ThenBy(g => g.CategoryId))
            {
                var history = session.LoadActiveRecords(group.LotId, group.CategoryId)
                    .Where(r => r.Activo == 1 && (original == null || r.Id != original.Id));
                var proposed = history.Concat(candidates.Where(r => r.LotId == group.LotId && r.CategoryId == group.CategoryId)).ToList();
                if (proposed.Any(r => r.Total <= 0)) return Fail(RecordWriteStatus.InvalidQuantity);
                if (proposed.Any(r => r.Date.Date == DateTime.MinValue)) return Fail(RecordWriteStatus.InvalidRequest);
                if (proposed.Any(r => LegacyStockOperationCatalog.Behavior(r.OperationId) == StockOperationBehavior.Unknown))
                    return Fail(RecordWriteStatus.UnsupportedOperation);
                var movements = proposed.Select(r => new StockMovement(r.Date, r.Total, r.Activo == 1,
                    LegacyStockOperationCatalog.Behavior(r.OperationId), r.OperationId));
                StockTimelineResult result;
                try { result = StockTimeline.Evaluate(movements); }
                catch (OverflowException) { return Fail(RecordWriteStatus.InvalidQuantity); }
                if (!result.IsValid && (firstConflict == null || result.ConflictDate < firstConflict.ConflictDate))
                {
                    var changedDates = candidates.Select(r => r.Date).ToList();
                    if (original != null) changedDates.Add(original.Date);
                    firstConflict = RecordWriteResult.TimelineConflict(result, group.LotId, group.CategoryId, changedDates.Min());
                }
            }
            return firstConflict ?? RecordWriteResult.Success();
        }

        private static RecordWriteResult Fail(RecordWriteStatus status) => RecordWriteResult.Failure(status);
        private static Record Copy(Record r) => new Record
        {
            Id = r.Id, Date = r.Date, Total = r.Total, UserId = r.UserId, LotId = r.LotId,
            CategoryId = r.CategoryId, OperationId = r.OperationId, Activo = r.Activo
        };
    }
}
