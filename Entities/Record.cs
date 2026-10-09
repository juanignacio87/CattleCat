namespace Entities
{
    using System;

    public partial class Record
    {
        
        public int Id { get; set; }
        
        public DateTime Date { get ; set; }

        public int Total { get; set; }

        public int UserId { get; set; }

        public int LotId { get; set; }

        public int CategoryId { get; set; }

        public int OperationId { get; set; }

        public int Activo { get; set; }

        #region CONSTRUCTORES

        public Record() { }

        public Record(int id)
        {
                this.Id = id;
        }

        public Record(int id,DateTime date, int userid, int lotid, int categoryid, int operationId, int total)
        {
            this.Id = id;
            this.Date = date;
            this.UserId = userid;
            this.LotId = lotid;
            this.CategoryId = categoryid;
            this.OperationId = operationId;
            this.Total = total;
        }

        public Record(DateTime date, int userid, int lotid, int categoryid, int operationId, int total)
        {
            this.Date = date;
            this.UserId = userid;
            this.LotId = lotid;
            this.CategoryId = categoryid;
            this.OperationId = operationId;
            this.Total = total;
        }

        #endregion
    }
}
