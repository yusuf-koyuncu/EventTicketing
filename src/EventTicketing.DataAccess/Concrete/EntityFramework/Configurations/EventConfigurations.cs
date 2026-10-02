using EventTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketing.DataAccess.Concrete.EntityFramework.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("Events", t =>
        {
            t.HasCheckConstraint("CK_Events_EndAfterStart", "[EndsAtUtc] > [StartsAtUtc]");
            t.HasCheckConstraint("CK_Events_SalesWindow", "[SalesEndUtc] > [SalesStartUtc]");
            t.HasCheckConstraint("CK_Events_CancellationCutoff", "[CancellationCutoffUtc] <= [StartsAtUtc]");
        });

        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        b.Property(x => x.PosterUrl).HasMaxLength(300);
        b.Property(x => x.EventType).HasConversion<int>().IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();

        b.Property(x => x.StartsAtUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(x => x.EndsAtUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(x => x.SalesStartUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(x => x.SalesEndUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(x => x.CancellationCutoffUtc).HasColumnType("datetime2(3)").IsRequired();

        b.HasOne(x => x.Venue)
         .WithMany(v => v.Events)
         .HasForeignKey(x => x.VenueId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Organizer)
         .WithMany(o => o.Events)
         .HasForeignKey(x => x.OrganizerId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Slug)
         .IsUnique()
         .HasFilter("[IsDeleted] = 0")
         .HasDatabaseName("UX_Events_Slug_Active");

        b.HasIndex(x => new { x.Status, x.StartsAtUtc })
         .IncludeProperties(x => new { x.Name, x.VenueId })
         .HasDatabaseName("IX_Events_Status_StartsAt");

        b.HasIndex(x => new { x.VenueId, x.StartsAtUtc })
         .HasDatabaseName("IX_Events_Venue_StartsAt");

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class EventSectionConfiguration : IEntityTypeConfiguration<EventSection>
{
    public void Configure(EntityTypeBuilder<EventSection> b)
    {
        b.ToTable("EventSections", t =>
        {
            t.HasCheckConstraint(
                "CK_EventSections_GaCapacityPositive",
                "[GeneralAdmissionCapacity] IS NULL OR [GeneralAdmissionCapacity] > 0");
        });

        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Event)
         .WithMany(e => e.EventSections)
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.VenueSection)
         .WithMany(v => v.EventSections)
         .HasForeignKey(x => x.VenueSectionId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SeatCategory)
         .WithMany()
         .HasForeignKey(x => x.SeatCategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.EventId, x.VenueSectionId })
         .IsUnique()
         .HasDatabaseName("UX_EventSections_Event_VenueSection");

        b.HasIndex(x => new { x.Id, x.EventId })
         .IsUnique()
         .HasDatabaseName("AK_EventSections_Id_EventId");
    }
}

public class EventSeatConfiguration : IEntityTypeConfiguration<EventSeat>
{
    public void Configure(EntityTypeBuilder<EventSeat> b)
    {
        b.ToTable("EventSeats");

        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.BlockReason).HasMaxLength(200);

        b.HasOne(x => x.Event)
         .WithMany()
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.EventSection)
         .WithMany(s => s.EventSeats)
         .HasForeignKey(x => x.EventSectionId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Seat)
         .WithMany(s => s.EventSeats)
         .HasForeignKey(x => x.SeatId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.EventId, x.SeatId })
         .IsUnique()
         .HasDatabaseName("UX_EventSeats_Event_Seat");

        b.HasIndex(x => new { x.EventId, x.Status })
         .IncludeProperties(x => new { x.SeatId, x.EventSectionId })
         .HasDatabaseName("IX_EventSeats_Event_Status");

        b.HasIndex(x => new { x.EventSectionId, x.Status })
         .HasDatabaseName("IX_EventSeats_Section_Status");
    }
}

public class EventTicketPriceConfiguration : IEntityTypeConfiguration<EventTicketPrice>
{
    public void Configure(EntityTypeBuilder<EventTicketPrice> b)
    {
        b.ToTable("EventTicketPrices", t =>
        {
            t.HasCheckConstraint("CK_EventTicketPrices_PriceNonNegative", "[Price] >= 0");
            t.HasCheckConstraint("CK_EventTicketPrices_FeeNonNegative", "[ServiceFee] >= 0");
        });

        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");
        b.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.ServiceFee).HasPrecision(18, 2).IsRequired();

        b.HasOne(x => x.Event)
         .WithMany(e => e.TicketPrices)
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.SeatCategory)
         .WithMany()
         .HasForeignKey(x => x.SeatCategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.EventId, x.SeatCategoryId })
         .IsUnique()
         .HasFilter("[IsActive] = 1")
         .HasDatabaseName("UX_EventTicketPrices_Event_Category");
    }
}
