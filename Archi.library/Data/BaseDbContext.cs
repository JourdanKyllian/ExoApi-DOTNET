using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Models
{
    public abstract class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions options) : base(options)
        {
        }
        public override int SaveChanges()
        {
            ChangeAddedState();
            ChangeModifiedState();
            ChangeDeletedState();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ChangeAddedState();
            ChangeModifiedState();
            ChangeDeletedState();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ChangeDeletedState()
        {
            var deletedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in deletedEntries)
            {
                if (entry.Entity is BaseModel model)
                {
                    entry.State = EntityState.Modified;
                    model.IsDeleted = true;
                    model.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        private void ChangeModifiedState()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in modifiedEntries)
            {
                if (entry.Entity is BaseModel model)
                {
                    model.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        private void ChangeAddedState()
        {
            var addedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            foreach (var entry in addedEntries)
            {
                if (entry.Entity is BaseModel model)
                {
                    model.CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}