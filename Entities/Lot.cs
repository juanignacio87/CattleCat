namespace Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Lot
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Lot()
        {
            Records = new HashSet<Record>();
        }

        public int Id { get; set; }

        [Required]
        public string LotName { get; set; }

        public int FarmId { get; set; }

        public virtual Farm Farm { get; set; }

        public int Active { get; set; }

        //El constructor lo hice yo
        #region CONSTRUCTOR

        public Lot(int farmid)
        {
            this.FarmId = farmid;
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Record> Records { get; set; }
    }
}
