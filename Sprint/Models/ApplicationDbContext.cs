using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Sprint.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<ApplicationDbContext>());
        }

        public virtual DbSet<Sprint> Sprints { get; set; }

        public virtual DbSet<SprintIssue> SprintsIssues { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sprint>().HasMany(x => x.Issues);

            base.OnModelCreating(modelBuilder);
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}