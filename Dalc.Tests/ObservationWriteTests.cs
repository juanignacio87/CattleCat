using System;
using System.Linq;
using CattleCat.Domain.Records;
using Dalc.RecordWriting;
using Entities;
using NUnit.Framework;
namespace Dalc.Tests
{
    [TestFixture]
    public class ObservationWriteTests
    {
        private static readonly DateTime Start = new DateTime(2026,11,6);
        private MemoryWriteDatabase db;
        private RecordWriter writer;
        private static Record R(int op,int quantity,int day=0,int id=0,int lot=1,int category=1,int active=1)
            => new Record {Id=id,OperationId=op,Total=quantity,Date=Start.AddDays(day),LotId=lot,CategoryId=category,UserId=1,Activo=active};
        [SetUp] public void Setup(){db=new MemoryWriteDatabase();writer=new RecordWriter(db.Open,()=>Start.AddDays(14));}
        private void Income(int quantity=35) => db.Records.Add(R(8,quantity,id:1));
        private void Treatment() => db.Records.Add(R(17,35,7,2));
        private void Rejected(RecordWriteResult result,RecordWriteStatus status)
        {
            Assert.That(result.Status,Is.EqualTo(status));
            Assert.That(db.Saves,Is.Zero);Assert.That(db.Commits,Is.Zero);Assert.That(db.Rollbacks,Is.EqualTo(1));
        }
        [TestCase(10,true)][TestCase(35,true)][TestCase(36,false)]
        public void IndividualTreatmentIsValidated(int quantity,bool valid)
        {
            Income();var result=writer.Create(R(17,quantity,1));
            Assert.That(result.IsSuccess,Is.EqualTo(valid));
            Assert.That(db.Records.Count,Is.EqualTo(valid?2:1));
        }
        [Test] public void TreatmentWithoutStockIsRejected()
        {Rejected(writer.Create(R(17,1)),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records,Is.Empty);}
        [TestCase(34,false)][TestCase(35,true)][TestCase(36,false)]
        public void IndividualCountMustMatch(int quantity,bool valid)
        {Income();var result=writer.Create(R(5,quantity,1));Assert.That(result.IsSuccess,Is.EqualTo(valid));Assert.That(db.Records.Count,Is.EqualTo(valid?2:1));}
        [Test] public void RetroactiveExitPreservesLaterTreatmentAndReportsContext()
        {
            Income();Treatment();var result=writer.Create(R(14,12,4));
            Rejected(result,RecordWriteStatus.OperationExceedsStock);
            Assert.Multiple(()=>{Assert.That(result.AvailableStock,Is.EqualTo(23));Assert.That(result.RequestedQuantity,Is.EqualTo(35));
                Assert.That(result.OperationId,Is.EqualTo(17));Assert.That(result.ConflictDate,Is.EqualTo(Start.AddDays(7)));
                Assert.That(result.LotId,Is.EqualTo(1));Assert.That(result.CategoryId,Is.EqualTo(1));Assert.That(result.IsRetroactiveConflict,Is.True);
                Assert.That(db.Records.Count,Is.EqualTo(2));});
        }
        [Test] public void RemovingIncomeCannotInvalidateTreatment()
        {Income();Treatment();Rejected(writer.Deactivate(1),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records[0].Activo,Is.EqualTo(1));}
        [Test] public void ReducingIncomeCannotInvalidateCount()
        {Income();db.Records.Add(R(5,35,7,2));Rejected(writer.Update(R(8,34,id:1)),RecordWriteStatus.CountMismatch);Assert.That(db.Records[0].Total,Is.EqualTo(35));}
        [Test] public void RemovingExitCanInvalidateLaterCount()
        {Income();db.Records.Add(R(14,12,4,2));db.Records.Add(R(5,23,7,3));Rejected(writer.Deactivate(2),RecordWriteStatus.CountMismatch);Assert.That(db.Records[1].Activo,Is.EqualTo(1));}
        [TestCase(2,1)][TestCase(1,2)]
        public void OtherGroupsCannotSupplyObservationStock(int lot,int category)
        {Income();Rejected(writer.Create(R(17,1,1,lot:lot,category:category)),RecordWriteStatus.OperationExceedsStock);}
        [TestCase(2,1)][TestCase(1,2)]
        public void MovingIncomeValidatesOriginObservations(int lot,int category)
        {Income();Treatment();Rejected(writer.Update(R(8,35,id:1,lot:lot,category:category)),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records[0].LotId,Is.EqualTo(1));}
        [Test] public void MovingTreatmentValidatesDestination()
        {Income();Treatment();Rejected(writer.Update(R(17,35,7,2,lot:2)),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records[1].LotId,Is.EqualTo(1));}
        [Test] public void ValidEditExcludesOriginalAndDoesNotDuplicate()
        {Income();db.Records.Add(R(14,12,4,2));db.Records.Add(R(17,23,7,3));Assert.That(writer.Update(R(17,22,6,3)).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(3));Assert.That(db.Records.Single(r=>r.Id==3).Total,Is.EqualTo(22));}
        [Test] public void InvalidEditPreservesOriginalQuantityAndDate()
        {Income();db.Records.Add(R(14,12,4,2));db.Records.Add(R(17,23,7,3));Rejected(writer.Update(R(17,35,6,3)),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records[2].Total,Is.EqualTo(23));Assert.That(db.Records[2].Date,Is.EqualTo(Start.AddDays(7)));}
        [Test] public void LaterIncomeCannotJustifyEarlierTreatment()
        {db.Records.Add(R(8,35,8,1));Rejected(writer.Create(R(17,35,7)),RecordWriteStatus.OperationExceedsStock);}
        [Test] public void InactiveTreatmentDoesNotBlockRetroactiveExit()
        {Income();db.Records.Add(R(17,35,7,2,active:0));Assert.That(writer.Create(R(14,12,4)).IsSuccess,Is.True);}
        [Test] public void InactiveIncomeCannotSupplyStock()
        {db.Records.Add(R(8,35,id:1,active:0));Rejected(writer.Create(R(17,1,7)),RecordWriteStatus.OperationExceedsStock);}
        [Test] public void NeutralDeactivationIsLogicalAndDoesNotRemoveOtherRows()
        {Income();Treatment();Assert.That(writer.Deactivate(2).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(2));Assert.That(db.Records[1].Activo,Is.Zero);}
        [Test] public void BatchUsesDailyOrderAndIsAtomic()
        {Assert.That(writer.CreateMany(new[]{R(14,12),R(17,35),R(5,35),R(8,35)}).IsSuccess,Is.True);Assert.That(db.Records.Count,Is.EqualTo(4));Assert.That(db.Commits,Is.EqualTo(1));}
        [Test] public void InvalidBatchStagesNothing()
        {Rejected(writer.CreateMany(new[]{R(8,35),R(17,36)}),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records,Is.Empty);Assert.That(db.Events,Does.Not.Contain("Add"));}
        [TestCase(6,true)][TestCase(8,false)]
        public void BatchIncomeRepairsOnlySubsequentTreatment(int day,bool valid)
        {Income();Treatment();var result=writer.CreateMany(new[]{R(14,12,4),R(8,12,day)});Assert.That(result.IsSuccess,Is.EqualTo(valid));Assert.That(db.Records.Count,Is.EqualTo(valid?4:2));}
        [TestCase("Add")][TestCase("SaveChanges")][TestCase("AfterSaveChanges")][TestCase("Commit")]
        public void TechnicalFailureDoesNotPartiallySaveObservations(string failure)
        {db.FailAt=failure;Assert.That(writer.CreateMany(new[]{R(8,35),R(17,35),R(5,35)}).Status,Is.EqualTo(RecordWriteStatus.TechnicalError));Assert.That(db.Records,Is.Empty);Assert.That(db.Commits,Is.Zero);Assert.That(db.Rollbacks,Is.EqualTo(1));}
        [Test] public void UnknownIdIsExplicitlyRejected()
        {Income();Rejected(writer.Create(R(999,1)),RecordWriteStatus.UnsupportedOperation);}
        [Test] public void InvalidHistoricalQuantityIsNotReportedAsTechnicalFailure()
        {db.Records.Add(R(8,0,id:1));Rejected(writer.Create(R(17,1)),RecordWriteStatus.InvalidQuantity);}
        [Test] public void ChangingOperationToTreatmentCannotBypassRule()
        {Income();db.Records.Add(R(8,12,7,2));Rejected(writer.Update(R(17,36,7,2)),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records[1].OperationId,Is.EqualTo(8));}
        [Test] public void ChangingDateCannotInvalidateEarlierTreatment()
        {Income();Treatment();Rejected(writer.Update(R(8,35,8,1)),RecordWriteStatus.OperationExceedsStock);Assert.That(db.Records[0].Date,Is.EqualTo(Start));}
        [Test] public void MovingOutcomeToOtherGroupValidatesBothCounts()
        {Income();db.Records.Add(R(14,12,4,2));db.Records.Add(R(5,23,7,3));db.Records.Add(R(8,35,id:4,lot:2));Rejected(writer.Update(R(14,12,4,2,lot:2)),RecordWriteStatus.CountMismatch);Assert.That(db.Records[1].LotId,Is.EqualTo(1));}
        [Test] public void OverflowingBatchIsInvalidDataAndRollsBack()
        {Rejected(writer.CreateMany(new[]{R(8,int.MaxValue),R(8,1)}),RecordWriteStatus.InvalidQuantity);Assert.That(db.Records,Is.Empty);}
        [Test] public void DifferentEstablishmentCannotSupplyObservationStock()
        {
            Income();db.Farms[2]=new Farm{Id=2,Activo=1};db.Lots[2].FarmId=2;
            Rejected(writer.Create(R(17,1,1,lot:2)),RecordWriteStatus.OperationExceedsStock);
        }
        [Test] public void UnrecognizedHistoricalOperationDoesNotSilentlyEscapeValidation()
        {Income();db.Records.Add(R(999,1,1,2));Rejected(writer.Create(R(8,1,2)),RecordWriteStatus.UnsupportedOperation);}
    }
}
