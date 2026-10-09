using System.Data.Entity;

namespace Entities
{
    public partial class Model : DbContext
    {
        public Model()
            : base("name=ModelConfig")
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Farm> Farms { get; set; }
        public virtual DbSet<Lot> Lots { get; set; }
        public virtual DbSet<Operation> Operations { get; set; }
        public virtual DbSet<Record> Records { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Category>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.Category)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Farm>()
                .HasMany(e => e.Lots)
                .WithRequired(e => e.Farm)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Lot>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.Lot)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Operation>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.Operation)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.CellPhone)
                .IsFixedLength();

            //modelBuilder.Entity<User>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.User)
            //    .WillCascadeOnDelete(false);
        }
    }
}
