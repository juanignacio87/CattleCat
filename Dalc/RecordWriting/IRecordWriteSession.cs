using System;
using System.Collections.Generic;
using System.Data;
using Entities;

namespace Dalc.RecordWriting
{
    // Test seam for the transaction boundary. Production always uses EfRecordWriteSession.
    public interface IRecordWriteTransaction : IDisposable
    {
        void Commit();
    }

    public interface IRecordWriteSession : IDisposable
    {
        IRecordWriteTransaction BeginTransaction(IsolationLevel isolation);
        Record FindRecord(int id);
        User FindUser(int id);
        Lot FindLot(int id);
        Farm FindFarm(int id);
        bool CategoryExists(int id);
        bool OperationExists(int id);
        IList<Record> LoadActiveRecords(int lotId, int categoryId);
        void Add(Record record);
        void Update(Record record);
        void Deactivate(int id);
        void SaveChanges();
    }
}
