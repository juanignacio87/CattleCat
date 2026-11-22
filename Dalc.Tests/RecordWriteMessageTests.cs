using System;
using Dalc.RecordWriting;
using Entities;
using Logic;
using NUnit.Framework;
namespace Dalc.Tests
{
    public class RecordWriteMessageTests
    {
        [TestCase(17,"Antidiarreico")][TestCase(5,"Conteo")]
        public void RetroactiveConflictMessageShowsSafeDetails(int operation,string label)
        {
            var db=new MemoryWriteDatabase();
            var date=new DateTime(2026,11,6);
            db.Records.Add(new Record{Id=1,Date=date,Total=35,OperationId=8,UserId=1,LotId=1,CategoryId=1,Activo=1});
            db.Records.Add(new Record{Id=2,Date=date.AddDays(7),Total=35,OperationId=operation,UserId=1,LotId=1,CategoryId=1,Activo=1});
            var writer=new RecordWriter(db.Open,()=>date.AddDays(14));
            var result=writer.Create(new Record{Date=date.AddDays(4),Total=12,OperationId=14,UserId=1,LotId=1,CategoryId=1});
            var message=RecordWriteMessages.Describe(result);
            Assert.That(message,Does.Contain("operación posterior").And.Contain("13/11/2026").And.Contain(label).And.Contain("23").And.Contain("35"));
            Assert.That(message,Does.Not.Contain("Exception").And.Not.Contain("SQL"));
        }
    }
}
