using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace SwoopMarketplaceProject.Models;

public partial class SwoopContext : DbContext
{
    public SwoopContext()
    {
    }

    public SwoopContext(DbContextOptions<SwoopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ListingImage> ListingImages { get; set; }

    public virtual DbSet<ListingView> ListingViews { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;user=root;database=swoop;password=root;", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("listings");

            entity.HasIndex(e => e.CategoryId, "idx_listings_category");

            entity.HasIndex(e => e.Status, "idx_listings_status");

            entity.HasIndex(e => e.UserId, "idx_listings_user");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20)")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20)")
                .HasColumnName("category_id");
            entity.Property(e => e.Condition)
                .HasColumnType("enum('fn','mw','ft','ww','bs')")
                .HasColumnName("condition");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('active','sold','inactive')")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Listings)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("listings_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Listings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("listings_ibfk_1");
        });

        modelBuilder.Entity<ListingImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("listing_images");

            entity.HasIndex(e => e.ListingId, "idx_listing_images_listing");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20)")
                .HasColumnName("id");
            entity.Property(e => e.ImageUrl)
                .HasColumnType("text")
                .HasColumnName("image_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_primary");
            entity.Property(e => e.ListingId)
                .HasColumnType("bigint(20)")
                .HasColumnName("listing_id");

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingImages)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("listing_images_ibfk_1");
        });

        modelBuilder.Entity<ListingView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("listing_views");

            entity.HasIndex(e => e.ListingId, "listing_views_ibfk_1");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.ListingId)
                .HasColumnType("bigint(11)")
                .HasColumnName("listing_id");
            entity.Property(e => e.ViewsCount)
                .HasColumnType("bigint(11)")
                .HasColumnName("views_count");

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingViews)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("listing_views_ibfk_1");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PRIMARY");

            entity.ToTable("reports");

            entity.Property(e => e.ReportId)
                .HasColumnType("int(11)")
                .HasColumnName("reportId");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");

            // Match listings.id and users.id which are bigint(20)
            entity.Property(e => e.ListingId)
                .HasColumnType("bigint(20)")
                .HasColumnName("listingId");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20)")
                .HasColumnName("userId");

            entity.HasIndex(e => e.ListingId, "IX_reports_listingId");
            entity.HasIndex(e => e.UserId, "IX_reports_userId");

            entity.HasOne(d => d.Listing)
                .WithMany() // or .WithMany(p => p.Reports) if you add a Reports collection on Listing
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_reports_listings_listingId");

            entity.HasOne(d => d.User)
                .WithMany() // or .WithMany(u => u.Reports) if you add a Reports collection on User
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_reports_users_userId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.Phone, "phone").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20)")
                .HasColumnName("id");
            entity.Property(e => e.Bio)
                .HasColumnType("text")
                .HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
           
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.ProfileImageUrl)
                .HasColumnType("text")
                .HasColumnName("profile_image_url");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
