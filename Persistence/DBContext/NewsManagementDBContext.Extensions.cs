using Microsoft.EntityFrameworkCore;

namespace Persistence.DBContext
{
	public partial class NewsManagementDBContext
	{
		public override int SaveChanges()
		{
			UpdateModifiedDates();
			return base.SaveChanges();
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			UpdateModifiedDates();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			UpdateModifiedDates();
			return base.SaveChangesAsync(cancellationToken);
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			UpdateModifiedDates();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void UpdateModifiedDates()
		{
			var utcNow = DateTime.UtcNow;
			// Update ModifiedDate for any modified NewsArticle
			var entries = ChangeTracker.Entries<Domain.Entities.NewsArticle>()
			.Where(e => e.State == EntityState.Modified);

			foreach (var entry in entries)
			{
				entry.Entity.ModifiedDate = utcNow;
			}
		}
	}
}
