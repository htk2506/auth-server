using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuthServer.Database
{
    public sealed class CustomSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
        )
        {
            if (eventData.Context is null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            // Handle timestampable creations
            IEnumerable<EntityEntry<ICreateModifyTimestampable>> createdTimestampableEntries =
                eventData
                    .Context
                    .ChangeTracker
                    .Entries<ICreateModifyTimestampable>()
                    .Where(e => e.State == EntityState.Added);

            foreach (EntityEntry<ICreateModifyTimestampable> createdTimestampableEntry in createdTimestampableEntries)
            {
                createdTimestampableEntry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                createdTimestampableEntry.Entity.ModifiedAt = DateTimeOffset.UtcNow;
            }

            // Handle timestampable modifications
            IEnumerable<EntityEntry<ICreateModifyTimestampable>> modifiedTimestampableEntries =
                eventData
                    .Context
                    .ChangeTracker
                    .Entries<ICreateModifyTimestampable>()
                    .Where(e => e.State == EntityState.Modified);

            foreach (EntityEntry<ICreateModifyTimestampable> modifiedTimestampableEntry in modifiedTimestampableEntries)
            {
                modifiedTimestampableEntry.Entity.ModifiedAt = DateTimeOffset.UtcNow;
            }

            // Handle soft deletes
            IEnumerable<EntityEntry<ISoftDeletable>> softDeletedEntries =
                eventData
                    .Context
                    .ChangeTracker
                    .Entries<ISoftDeletable>()
                    .Where(e => e.State == EntityState.Deleted);

            foreach (EntityEntry<ISoftDeletable> softDeletedEntry in softDeletedEntries)
            {
                softDeletedEntry.State = EntityState.Modified;
                softDeletedEntry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                softDeletedEntry.Entity.IsDeleted = true;
            }

            // Call base version
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
