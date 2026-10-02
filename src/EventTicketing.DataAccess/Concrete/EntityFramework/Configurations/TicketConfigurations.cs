using EventTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketing.DataAccess.Concrete.EntityFramework.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.PhoneNumber).HasMaxLength(32);
        b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        b.Property(x => x.Role).HasConversion<int>().IsRequired();

        b.HasIndex(x => x.Email)
         .IsUnique()
         .HasFilter("[IsDeleted] = 0")
         .HasDatabaseName("UX_Users_Email_Active");

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.ToTable("Tickets", t =>
        {
            t.HasCheckConstraint("CK_Tickets_PriceNonNegative", "[PricePaid] >= 0 AND [ServiceFeePaid] >= 0");

            t.HasCheckConstraint(
                "CK_Tickets_CancellationConsistency",
                "([Status] = 2 AND [CancelledAtUtc] IS NOT NULL) OR " +
                "([Status] <> 2 AND [CancelledAtUtc] IS NULL)");
        });

        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.Property(x => x.TicketNumber).IsRequired();
        b.Property(x => x.SeatLabelSnapshot).HasMaxLength(150).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();

        b.Property(x => x.PricePaid).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.ServiceFeePaid).HasPrecision(18, 2).IsRequired();

        b.Property(x => x.PurchasedAtUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(x => x.CancelledAtUtc).HasColumnType("datetime2(3)");

        b.Ignore(x => x.OccupiesInventory);
        b.Ignore(x => x.TotalPaid);

        b.HasOne(x => x.Event)
         .WithMany(e => e.Tickets)
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.User)
         .WithMany(u => u.Tickets)
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.EventSection)
         .WithMany(s => s.Tickets)
         .HasForeignKey(x => x.EventSectionId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.EventSeat)
         .WithMany(s => s.Tickets)
         .HasForeignKey(x => x.EventSeatId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SeatCategory)
         .WithMany()
         .HasForeignKey(x => x.SeatCategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.StatusHistory)
         .WithOne(h => h.Ticket)
         .HasForeignKey(h => h.TicketId)
         .OnDelete(DeleteBehavior.Restrict);

        b.Navigation(x => x.StatusHistory)
         .UsePropertyAccessMode(PropertyAccessMode.Field)
         .HasField("_statusHistory");

        b.HasIndex(x => x.TicketNumber)
         .IsUnique()
         .HasDatabaseName("UX_Tickets_TicketNumber");

        b.HasIndex(x => x.EventSeatId)
         .IsUnique()
         .HasFilter("[EventSeatId] IS NOT NULL AND [Status] = 1")
         .HasDatabaseName("UX_Tickets_ActiveSeat");

        b.HasIndex(x => new { x.UserId, x.PurchasedAtUtc })
         .IsDescending(false, true)
         .IncludeProperties(x => new { x.EventId, x.Status, x.PricePaid, x.TicketNumber })
         .HasDatabaseName("IX_Tickets_User_PurchasedAt");

        b.HasIndex(x => new { x.EventId, x.Status })
         .HasDatabaseName("IX_Tickets_Event_Status");

        b.HasIndex(x => new { x.EventSectionId, x.Status })
         .HasDatabaseName("IX_Tickets_EventSection_Status");
    }
}

public class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> b)
    {
        b.ToTable("TicketStatusHistory");
        b.HasKey(x => x.Id);

        b.Property(x => x.FromStatus).HasConversion<int?>();
        b.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.ChangedAtUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(x => x.Reason).HasMaxLength(500);

        b.HasOne(x => x.ChangedByUser)
         .WithMany()
         .HasForeignKey(x => x.ChangedByUserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.TicketId, x.ChangedAtUtc })
         .HasDatabaseName("IX_TicketStatusHistory_Ticket_ChangedAt");

        b.HasIndex(x => new { x.ToStatus, x.ChangedAtUtc })
         .HasDatabaseName("IX_TicketStatusHistory_ToStatus_ChangedAt");
    }
}
