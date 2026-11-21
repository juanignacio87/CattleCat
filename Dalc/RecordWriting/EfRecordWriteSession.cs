using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Entities;

namespace Dalc.RecordWriting
{
    internal sealed class EfRecordWriteSession : IRecordWriteSession
    {
        // A fresh context per command: no tracked UI objects or failed writes survive reuse.
        private readonly Model db = new Model();

        public IRecordWriteTransaction BeginTransaction(IsolationLevel isolation)
            => new Transaction(db.Database.BeginTransaction(isolation));
        public Record FindRecord(int id) => db.Records.SingleOrDefault(r => r.Id == id);
        public User FindUser(int id) => db.Users.AsNoTracking().SingleOrDefault(u => u.Id == id);
        public Lot FindLot(int id) => db.Lots.AsNoTracking().SingleOrDefault(l => l.Id == id);
        public Farm FindFarm(int id) => db.Farms.AsNoTracking().SingleOrDefault(f => f.Id == id);
        public bool CategoryExists(int id) => db.Categories.Any(c => c.Id == id);
        public bool OperationExists(int id) => db.Operations.Any(o => o.Id == id);
        public IList<Record> LoadActiveRecords(int lotId, int categoryId)
            => db.Records.AsNoTracking().Where(r => r.Activo == 1 && r.LotId == lotId && r.CategoryId == categoryId).ToList();
        public void Add(Record record) => db.Records.Add(record);
        public void Update(Record record)
        {
            var current = FindRecord(record.Id);
            current.Date = record.Date;
            current.Total = record.Total;
            current.UserId = record.UserId;
            current.LotId = record.LotId;
            current.CategoryId = record.CategoryId;
            current.OperationId = record.OperationId;
        }
        public void Deactivate(int id) => FindRecord(id).Activo = 0;
        public void SaveChanges() => db.SaveChanges();
        public void Dispose() => db.Dispose();

        private sealed class Transaction : IRecordWriteTransaction
        {
            private readonly DbContextTransaction transaction;
            public Transaction(DbContextTransaction transaction) { this.transaction = transaction; }
            public void Commit() => transaction.Commit();
            // EF rolls back an uncommitted transaction on disposal, including early returns.
            public void Dispose() => transaction.Dispose();
        }
    }
}
