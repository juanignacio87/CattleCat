using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Dalc.RecordWriting;
using Entities;

namespace Dalc.Tests
{
    // Transactional test double, NOT an EF/SQL Server substitute for integration claims.
    internal sealed class MemoryWriteDatabase
    {
        public List<Record> Records = new List<Record>();
        public readonly Dictionary<int, User> Users = new Dictionary<int, User> { [1] = new User { Id = 1, Activo = 1 } };
        public readonly Dictionary<int, Lot> Lots = new Dictionary<int, Lot> { [1] = new Lot { Id = 1, FarmId = 1, Active = 1 }, [2] = new Lot { Id = 2, FarmId = 1, Active = 1 } };
        public readonly Dictionary<int, Farm> Farms = new Dictionary<int, Farm> { [1] = new Farm { Id = 1, Activo = 1 } };
        public readonly HashSet<int> Categories = new HashSet<int> { 1, 2 };
        public readonly HashSet<int> Operations = new HashSet<int>(Enumerable.Range(1, 18).Concat(new[] { 999 }));
        public readonly List<string> Events = new List<string>();
        public string FailAt;
        public IsolationLevel? Isolation;
        public int Commits;
        public int Saves;
        public int Rollbacks;
        private readonly object gate = new object();
        public IRecordWriteSession Open() => new Session(this);
        public static Record Copy(Record r) => new Record { Id=r.Id, Date=r.Date, Total=r.Total, UserId=r.UserId, LotId=r.LotId, CategoryId=r.CategoryId, OperationId=r.OperationId, Activo=r.Activo };

        private sealed class Session : IRecordWriteSession, IRecordWriteTransaction
        {
            private readonly MemoryWriteDatabase database;
            private List<Record> original;
            private List<Record> pending;
            private bool active;
            private bool committed;
            public Session(MemoryWriteDatabase database) { this.database=database; }
            public IRecordWriteTransaction BeginTransaction(IsolationLevel isolation)
            {
                Monitor.Enter(database.gate);
                active=true;
                database.Isolation=isolation;
                original=database.Records.Select(Copy).ToList();
                pending=original.Select(Copy).ToList();
                database.Events.Add("Begin");
                return this;
            }
            private void Check(string action)
            {
                if (!active) throw new InvalidOperationException("Operation outside transaction: " + action);
                database.Events.Add(action);
                if (database.FailAt==action) throw new InvalidOperationException("Injected technical failure");
            }
            public Record FindRecord(int id) { Check("FindRecord"); return pending.SingleOrDefault(r=>r.Id==id); }
            public User FindUser(int id) { Check("FindUser"); return database.Users.ContainsKey(id) ? database.Users[id] : null; }
            public Lot FindLot(int id) { Check("FindLot"); return database.Lots.ContainsKey(id) ? database.Lots[id] : null; }
            public Farm FindFarm(int id) { Check("FindFarm"); return database.Farms.ContainsKey(id) ? database.Farms[id] : null; }
            public bool CategoryExists(int id) { Check("CategoryExists"); return database.Categories.Contains(id); }
            public bool OperationExists(int id) { Check("OperationExists"); return database.Operations.Contains(id); }
            public IList<Record> LoadActiveRecords(int lotId,int categoryId) { Check("LoadActiveRecords"); return pending.Where(r=>r.Activo==1 && r.LotId==lotId && r.CategoryId==categoryId).Select(Copy).ToList(); }
            public void Add(Record record) { Check("Add"); var copy=Copy(record);copy.Id=pending.Select(r=>r.Id).DefaultIfEmpty(0).Max()+1;pending.Add(copy); }
            public void Update(Record record) { Check("Update"); pending[pending.FindIndex(r=>r.Id==record.Id)]=Copy(record); }
            public void Deactivate(int id) { Check("Deactivate"); pending.Single(r=>r.Id==id).Activo=0; }
            public void SaveChanges()
            {
                Check("SaveChanges");database.Saves++;
                database.Records=pending.Select(Copy).ToList();
                Check("AfterSaveChanges");
            }
            public void Commit() { Check("Commit"); committed=true; database.Commits++; }
            public void Dispose()
            {
                if (!active) return;
                if (!committed) { database.Records=original.Select(Copy).ToList();database.Rollbacks++; }
                database.Events.Add(committed ? "DisposeCommitted" : "Rollback");
                active=false;
                Monitor.Exit(database.gate);
            }
        }
    }
}
