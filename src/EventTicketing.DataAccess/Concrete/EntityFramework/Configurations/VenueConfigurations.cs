using EventTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketing.DataAccess.Concrete.EntityFramework.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> b)
    {
        b.ToTable("Venues");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Address).HasMaxLength(300).IsRequired();
        b.Property(x => x.City).HasMaxLength(100).IsRequired();
        b.Property(x => x.Country).HasMaxLength(100).IsRequired();

        b.HasIndex(x => x.Name)
         .IsUnique()
         .HasFilter("[IsDeleted] = 0")
         .HasDatabaseName("UX_Venues_Name_Active");

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SeatCategoryConfiguration : IEntityTypeConfiguration<SeatCategory>
{
    public void Configure(EntityTypeBuilder<SeatCategory> b)
    {
        b.ToTable("SeatCategories");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.Property(x => x.Name).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Venue)
         .WithMany(v => v.SeatCategories)
         .HasForeignKey(x => x.VenueId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.VenueId, x.Name })
         .IsUnique()
         .HasDatabaseName("UX_SeatCategories_Venue_Name");
    }
}

public class VenueSectionConfiguration : IEntityTypeConfiguration<VenueSection>
{
    public void Configure(EntityTypeBuilder<VenueSection> b)
    {
        b.ToTable("VenueSections", t =>
        {
            t.HasCheckConstraint(
                "CK_VenueSections_CapacityByType",
                "([SectionType] = 1 AND [GeneralAdmissionCapacity] IS NULL) OR " +
                "([SectionType] = 2 AND [GeneralAdmissionCapacity] > 0)");
        });

        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.SectionType).HasConversion<int>().IsRequired();

        b.HasOne(x => x.Venue)
         .WithMany(v => v.Sections)
         .HasForeignKey(x => x.VenueId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.SeatCategory)
         .WithMany(c => c.VenueSections)
         .HasForeignKey(x => x.SeatCategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.VenueId, x.Name })
         .IsUnique()
         .HasDatabaseName("UX_VenueSections_Venue_Name");

        b.HasIndex(x => new { x.Id, x.VenueId })
         .IsUnique()
         .HasDatabaseName("AK_VenueSections_Id_VenueId");
    }
}

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> b)
    {
        b.ToTable("Seats");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.Property(x => x.RowLabel).HasMaxLength(10);
        b.Property(x => x.SeatNumber).HasMaxLength(10).IsRequired();

        b.HasOne(x => x.VenueSection)
         .WithMany(s => s.Seats)
         .HasForeignKey(x => x.VenueSectionId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.VenueSectionId, x.RowLabel, x.SeatNumber })
         .IsUnique()
         .HasDatabaseName("UX_Seats_Section_Row_Number");
    }
}

public class OrganizerConfiguration : IEntityTypeConfiguration<Organizer>
{
    public void Configure(EntityTypeBuilder<Organizer> b)
    {
        b.ToTable("Organizers");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETDATE()");

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ContactEmail).HasMaxLength(256).IsRequired();

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
