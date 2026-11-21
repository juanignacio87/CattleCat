using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CattleCat.Compatibility;
using CattleCat.Domain.Records;
using Dalc.RecordWriting;
using Entities;
using NUnit.Framework;

namespace Dalc.Tests
{
    [TestFixture]
    public class RecordWriterTests
    {
        private static readonly DateTime Today=new DateTime(2026,11,20);
        private MemoryWriteDatabase db;
        private RecordWriter writer;
        private static Record Row(int quantity=20,int operation=LegacyStockOperationCatalog.Entry,int day=-3,int id=0,int lot=1,int category=1,int active=1)
            => new Record {Id=id,Date=Today.AddDays(day),Total=quantity,OperationId=operation,UserId=1,LotId=lot,CategoryId=category,Activo=active};
        [SetUp] public void Setup() {db=new MemoryWriteDatabase();writer=new RecordWriter(db.Open,()=>Today);}
        private void Seed() {db.Records.Add(Row(id:1));db.Records.Add(Row(5,LegacyStockOperationCatalog.Exit,-1,2));}
        private void AssertRejected(RecordWriteResult result,RecordWriteStatus status)
        {
            Assert.That(result.Status,Is.EqualTo(status));
            Assert.That(db.Commits,Is.Zero);
            Assert.That(db.Saves,Is.Zero);
        }

        [Test] public void ValidCreateUsesSerializableAndOneCommit()
        {
            var input=Row(); Assert.That(writer.Create(input).IsSuccess,Is.True);
            Assert.Multiple(()=> {Assert.That(db.Records.Count,Is.EqualTo(1));Assert.That(db.Records[0].Activo,Is.EqualTo(1));Assert.That(input.Id,Is.Zero);Assert.That(db.Isolation,Is.EqualTo(IsolationLevel.Serializable));Assert.That(db.Commits,Is.EqualTo(1));Assert.That(db.Saves,Is.EqualTo(1));Assert.That(db.Events.First(),Is.EqualTo("Begin"));Assert.That(db.Events.Last(),Is.EqualTo("DisposeCommitted"));});
        }
        [Test] public void CreateWithoutStockReportsConflict()
        {var result=writer.Create(Row(5,LegacyStockOperationCatalog.Exit));AssertRejected(result,RecordWriteStatus.StockConflict);Assert.That(result.ConflictDate,Is.EqualTo(Today.AddDays(-3)));Assert.That(result.ProjectedStock,Is.EqualTo(-5));Assert.That(db.Records,Is.Empty);}
        [TestCase(0)][TestCase(-1)][TestCase(int.MinValue)]
        public void CreateRejectsNonPositiveQuantity(int quantity) => AssertRejected(writer.Create(Row(quantity)),RecordWriteStatus.InvalidQuantity);
        [Test] public void CreateRejectsFutureDate() => AssertRejected(writer.Create(Row(day:1)),RecordWriteStatus.FutureDate);
        [Test] public void TodayWithTimeIsNotFuture() => Assert.That(writer.Create(new Record {Date=Today.AddHours(23),Total=1,UserId=1,LotId=1,CategoryId=1,OperationId=LegacyStockOperationCatalog.Entry}).IsSuccess,Is.True);
        [TestCase("user-missing",RecordWriteStatus.UserNotFound)]
        [TestCase("user-inactive",RecordWriteStatus.UserInactive)]
        [TestCase("lot-missing",RecordWriteStatus.LotNotFound)]
        [TestCase("lot-inactive",RecordWriteStatus.LotInactive)]
        [TestCase("farm-missing",RecordWriteStatus.FarmNotFound)]
        [TestCase("farm-inactive",RecordWriteStatus.FarmInactive)]
        [TestCase("category-missing",RecordWriteStatus.CategoryNotFound)]
        [TestCase("operation-missing",RecordWriteStatus.OperationNotFound)]
        public void CreateRejectsUnavailableReference(string missing,RecordWriteStatus expected)
        {
            switch(missing) {case "user-missing":db.Users.Clear();break;case "user-inactive":db.Users[1].Activo=0;break;case "lot-missing":db.Lots.Clear();break;case "lot-inactive":db.Lots[1].Active=0;break;case "farm-missing":db.Farms.Clear();break;case "farm-inactive":db.Farms[1].Activo=0;break;case "category-missing":db.Categories.Clear();break;case "operation-missing":db.Operations.Clear();break;}
            AssertRejected(writer.Create(Row()),expected);
        }
        [Test] public void BatchValidatesCombinedEffectsAndDailyOrder()
        {Assert.That(writer.CreateMany(new[] {Row(5,LegacyStockOperationCatalog.Exit),Row(20)}).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(2));Assert.That(db.Commits,Is.EqualTo(1));}
        [Test] public void BatchRejectsCombinedOverdraw()
        {db.Records.Add(Row(id:1));AssertRejected(writer.CreateMany(new[] {Row(12,LegacyStockOperationCatalog.Exit),Row(12,LegacyStockOperationCatalog.Exit)}),RecordWriteStatus.StockConflict);Assert.That(db.Records.Count,Is.EqualTo(1));}
        [Test] public void BatchDoesNotSaveFirstValidRowWhenSecondInvalid()
        {AssertRejected(writer.CreateMany(new[] {Row(),Row(0)}),RecordWriteStatus.InvalidQuantity);Assert.That(db.Records,Is.Empty);Assert.That(db.Events,Does.Not.Contain("Add"));}
        [Test] public void BatchValidatesAllGroupsBeforeStaging()
        {AssertRejected(writer.CreateMany(new[] {Row(),Row(1,LegacyStockOperationCatalog.Exit,lot:2)}),RecordWriteStatus.StockConflict);Assert.That(db.Records,Is.Empty);Assert.That(db.Events,Does.Not.Contain("Add"));}
        [Test] public void CreateDoesNotReactivateAnInactiveHistoricalId()
        {db.Records.Add(Row(id:1,active:0));AssertRejected(writer.Create(Row(id:1)),RecordWriteStatus.InvalidRequest);Assert.That(db.Records[0].Activo,Is.Zero);}
        [Test] public void InactiveHistoryDoesNotSupplyStock()
        {db.Records.Add(Row(id:1,active:0));AssertRejected(writer.Create(Row(1,LegacyStockOperationCatalog.Exit)),RecordWriteStatus.StockConflict);}
        [TestCase(5)][TestCase(1)][TestCase(999)]
        public void NeutralOperationsDoNotAffectStock(int operation) => Assert.That(writer.Create(Row(1,operation)).IsSuccess,Is.True);
        [TestCase(8)][TestCase(12)] public void HistoricalIncomeIdsSupplyStock(int income)
        {Assert.That(writer.CreateMany(new[] {Row(5,income),Row(5,LegacyStockOperationCatalog.Exit,day:-1)}).IsSuccess,Is.True);}
        [TestCase(11)][TestCase(14)] public void HistoricalOutcomeIdsRequireStock(int outcome)
        {AssertRejected(writer.Create(Row(1,outcome)),RecordWriteStatus.StockConflict);}
        [Test] public void ValidUpdateReplacesWithoutDuplicate()
        {Seed();Assert.That(writer.Update(Row(25,id:1)).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(2));Assert.That(db.Records.Single(r=>r.Id==1).Total,Is.EqualTo(25));}
        [Test] public void UpdateExcludesOriginalExactlyOnce()
        {Seed();Assert.That(writer.Update(Row(5,id:1)).IsSuccess,Is.True);Assert.That(db.Records.Single(r=>r.Id==1).Total,Is.EqualTo(5));}
        [Test] public void UpdateRejectsLaterNegativeBalanceAndPreservesOriginal()
        {Seed();var result=writer.Update(Row(4,id:1));AssertRejected(result,RecordWriteStatus.StockConflict);Assert.That(result.ConflictDate,Is.EqualTo(Today.AddDays(-1)));Assert.That(result.ProjectedStock,Is.EqualTo(-1));Assert.That(db.Records[0].Total,Is.EqualTo(20));}
        [Test] public void UpdateMovingIncomeAfterOutcomeIsRejected()
        {Seed();AssertRejected(writer.Update(Row(20,day:0,id:1)),RecordWriteStatus.StockConflict);Assert.That(db.Records[0].Date,Is.EqualTo(Today.AddDays(-3)));}
        [TestCase(2,1)][TestCase(1,2)] public void UpdateChecksOriginalGroupWhenMoving(int lot,int category)
        {Seed();AssertRejected(writer.Update(Row(id:1,lot:lot,category:category)),RecordWriteStatus.StockConflict);Assert.That(db.Records[0].LotId,Is.EqualTo(1));Assert.That(db.Records[0].CategoryId,Is.EqualTo(1));}
        [Test] public void UpdateChecksDestinationGroup()
        {db.Records.Add(Row(id:1));db.Records.Add(Row(5,LegacyStockOperationCatalog.Exit,-1,2,lot:2));AssertRejected(writer.Update(Row(4,id:1,lot:2)),RecordWriteStatus.StockConflict);}
        [TestCase(false)][TestCase(true)] public void MissingOrInactiveUpdateRejected(bool inactive)
        {if(inactive) db.Records.Add(Row(id:1,active:0));AssertRejected(writer.Update(Row(id:1)),RecordWriteStatus.RecordUnavailable);}
        [Test] public void UpdateCannotBypassQuantityValidation()
        {Seed();AssertRejected(writer.Update(Row(0,id:1)),RecordWriteStatus.InvalidQuantity);}
        [Test] public void UpdateCannotBypassFutureDateValidation()
        {Seed();AssertRejected(writer.Update(Row(day:1,id:1)),RecordWriteStatus.FutureDate);}
        [Test] public void UpdateCannotBypassInactiveParentValidation()
        {Seed();db.Farms[1].Activo=0;AssertRejected(writer.Update(Row(id:1)),RecordWriteStatus.FarmInactive);}
        [Test] public void IncomeCanBeMovedWhenOriginAndDestinationRemainValid()
        {db.Records.Add(Row(id:1));Assert.That(writer.Update(Row(id:1,lot:2)).IsSuccess,Is.True);Assert.That(db.Records[0].LotId,Is.EqualTo(2));}
        [Test] public void IncomeCannotBecomeNeutralIfNeededLater()
        {Seed();AssertRejected(writer.Update(Row(20,5,id:1)),RecordWriteStatus.StockConflict);}
        [Test] public void ValidNeutralDeactivationIsLogical()
        {db.Records.Add(Row(20,5,id:1));Assert.That(writer.Deactivate(1).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(1));Assert.That(db.Records[0].Activo,Is.Zero);}
        [Test] public void UnneededIncomeCanBeDeactivated()
        {db.Records.Add(Row(id:1));Assert.That(writer.Deactivate(1).IsSuccess,Is.True);Assert.That(db.Records[0].Activo,Is.Zero);}
        [TestCase(8)][TestCase(12)] public void NecessaryIncomeCannotBeDeactivated(int income)
        {db.Records.Add(Row(operation:income,id:1));db.Records.Add(Row(5,LegacyStockOperationCatalog.Exit,-1,2));var result=writer.Deactivate(1);AssertRejected(result,RecordWriteStatus.StockConflict);Assert.That(result.ProjectedStock,Is.EqualTo(-5));Assert.That(db.Records[0].Activo,Is.EqualTo(1));}
        [Test] public void OutcomeCanBeDeactivated()
        {Seed();Assert.That(writer.Deactivate(2).IsSuccess,Is.True);Assert.That(db.Records.Single(r=>r.Id==2).Activo,Is.Zero);}
        [TestCase(false)][TestCase(true)] public void MissingOrInactiveDeactivationRejected(bool inactive)
        {if(inactive)db.Records.Add(Row(id:1,active:0));AssertRejected(writer.Deactivate(1),RecordWriteStatus.RecordUnavailable);}
        [TestCase("FindUser")][TestCase("LoadActiveRecords")][TestCase("Add")][TestCase("SaveChanges")][TestCase("AfterSaveChanges")][TestCase("Commit")]
        public void TechnicalFailureRollsBackWholeBatch(string failure)
        {db.FailAt=failure;Assert.That(writer.CreateMany(new[] {Row(),Row(1,5)}).Status,Is.EqualTo(RecordWriteStatus.TechnicalError));Assert.That(db.Records,Is.Empty);Assert.That(db.Commits,Is.Zero);Assert.That(db.Rollbacks,Is.EqualTo(1));}
        [TestCase("Update")][TestCase("AfterSaveChanges")][TestCase("Commit")]
        public void TechnicalUpdateFailurePreservesOriginal(string failure)
        {Seed();db.FailAt=failure;Assert.That(writer.Update(Row(25,id:1)).Status,Is.EqualTo(RecordWriteStatus.TechnicalError));Assert.That(db.Records[0].Total,Is.EqualTo(20));Assert.That(db.Records.Count,Is.EqualTo(2));}
        [TestCase("Deactivate")][TestCase("AfterSaveChanges")][TestCase("Commit")]
        public void TechnicalDeactivationFailurePreservesState(string failure)
        {Seed();db.FailAt=failure;Assert.That(writer.Deactivate(2).Status,Is.EqualTo(RecordWriteStatus.TechnicalError));Assert.That(db.Records[1].Activo,Is.EqualTo(1));}
        [Test] public void ARejectedCommandDoesNotPoisonNextWrite()
        {Assert.That(writer.Create(Row(1,LegacyStockOperationCatalog.Exit)).IsSuccess,Is.False);Assert.That(writer.Create(Row()).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(1));}
        [Test] public void BatchReportsEarliestConflictAcrossGroups()
        {
            var result=writer.CreateMany(new[] {Row(1,LegacyStockOperationCatalog.Exit,day:-1,lot:1),Row(1,LegacyStockOperationCatalog.Exit,day:-3,lot:2)});
            AssertRejected(result,RecordWriteStatus.StockConflict);
            Assert.That(result.ConflictDate,Is.EqualTo(Today.AddDays(-3)));
        }
        [Test] public void DeactivationStillRequiresActiveReferences()
        {
            Seed();db.Users[1].Activo=0;
            AssertRejected(writer.Deactivate(2),RecordWriteStatus.UserInactive);
            Assert.That(db.Records[1].Activo,Is.EqualTo(1));
        }
        [Test] public void ATechnicalFailureDoesNotPoisonNextWrite()
        {
            db.FailAt="AfterSaveChanges";
            Assert.That(writer.Create(Row()).Status,Is.EqualTo(RecordWriteStatus.TechnicalError));
            db.FailAt=null;
            Assert.That(writer.Create(Row()).IsSuccess,Is.True);
            Assert.That(db.Records.Count,Is.EqualTo(1));
        }
        [Test] public void ContextCreationFailureIsTechnical()
        {var failing=new RecordWriter(()=>{throw new InvalidOperationException("Unavailable provider");});Assert.That(failing.Create(Row()).Status,Is.EqualTo(RecordWriteStatus.TechnicalError));}
        [Test] public void NullRequestsAreExplicitlyRejected()
        {AssertRejected(writer.Create(null),RecordWriteStatus.InvalidRequest);AssertRejected(writer.Update(null),RecordWriteStatus.InvalidRequest);AssertRejected(writer.CreateMany(null),RecordWriteStatus.InvalidRequest);}
        [Test] public void EmptyBatchRetainsNoOpCompatibility()
        {Assert.That(writer.CreateMany(new Record[0]).IsSuccess,Is.True);Assert.That(db.Records,Is.Empty);}
        [Test] public void SimultaneousCommandsWithSerializedTestTransactionsCannotBothOverdraw()
        {
            db.Records.Add(Row(20,id:1));
            using(var start=new ManualResetEventSlim(false))
            {
                var a=Task.Run(()=>{start.Wait();return new RecordWriter(db.Open,()=>Today).Create(Row(15,LegacyStockOperationCatalog.Exit,day:-1));});
                var b=Task.Run(()=>{start.Wait();return new RecordWriter(db.Open,()=>Today).Create(Row(15,LegacyStockOperationCatalog.Exit,day:-1));});
                start.Set();Assert.That(Task.WaitAll(new Task[] {a,b},TimeSpan.FromSeconds(10)),Is.True);
                Assert.That(new[] {a.Result,b.Result}.Count(r=>r.IsSuccess),Is.EqualTo(1));
                Assert.That(new[] {a.Result,b.Result}.Count(r=>r.Status==RecordWriteStatus.StockConflict),Is.EqualTo(1));
                Assert.That(db.Records.Count,Is.EqualTo(2));
            }
        }
    }
}
