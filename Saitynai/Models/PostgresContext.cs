using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Npgsql; 
using Saitynai.Models;


namespace Saitynai.Models;


public partial class PostgresContext : DbContext
{
    public PostgresContext() { }


    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options) { }


    // Your main application DbSets
    public virtual DbSet<AccessPoint> AccessPoint { get; set; }
    public virtual DbSet<Building> Building { get; set; }
    public virtual DbSet<Floor> Floor { get; set; }
    public virtual DbSet<Point> Point { get; set; }
    public virtual DbSet<Scan> Scan { get; set; }


    // Your Authentication/Authorization DbSets
    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<Role> Role { get; set; }
    public virtual DbSet<Permission> Permission { get; set; }
    public virtual DbSet<UserRole> UserRole { get; set; }
    public virtual DbSet<RolePermission> RolePermission { get; set; }
    public virtual DbSet<ResourceType> ResourceType { get; set; }
    
    // === ADD THIS DbSet for REFRESH TOKENS ===
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // === Auth Entity Configurations ===


        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "saitynai");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            // This defines the other side of the relationship from RefreshToken
            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId);
        });
        
        // === ADD THIS NEW ENTITY CONFIGURATION ===
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens", "saitynai");
            entity.HasKey(e => e.Id);


            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Token).HasColumnName("token").IsRequired();
            entity.Property(e => e.ExpiresOn).HasColumnName("expires_on").IsRequired();
            entity.Property(e => e.CreatedOn).HasColumnName("created_on").IsRequired();
            entity.Property(e => e.RevokedOn).HasColumnName("revoked_on");
            entity.Property(e => e.ReplacedByToken).HasColumnName("replaced_by_token");


            // This configures the foreign key relationship to the User table
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // When a user is deleted, their tokens are deleted
        });
        // ==========================================


        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "saitynai");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });


        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions", "saitynai");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });


        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles", "saitynai");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");


            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);


            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });


        modelBuilder.Entity<ResourceType>(entity =>
        {
            entity.ToTable("resource_types", "saitynai");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });


        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions", "saitynai");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId, rp.ResourceTypeId, rp.ResourceId });


            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.ResourceTypeId).HasColumnName("resource_type_id");
            entity.Property(e => e.ResourceId).HasColumnName("resource_id");


            entity.Property(e => e.Allow).HasColumnName("allow");
            entity.Property(e => e.Cascade).HasColumnName("cascade");


            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);


            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);


             entity.HasOne(rp => rp.ResourceType)
                .WithMany() 
                .HasForeignKey(rp => rp.ResourceTypeId);
        });


        // === Existing Entity Configurations (Unchanged) ===


        modelBuilder.Entity<AccessPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("access_point_pkey");
            entity.ToTable("access_point", "saitynai");
            entity.HasIndex(e => e.ScanId, "idx_ap_scan_id");
            entity.HasIndex(e => new { e.ScanId, e.Bssid }, "ux_ap_scan_bssid").IsUnique();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bssid).HasColumnName("bssid");
            entity.Property(e => e.Capabilities).HasColumnName("capabilities");
            entity.Property(e => e.Centerfreq0).HasColumnName("centerfreq0");
            entity.Property(e => e.Centerfreq1).HasColumnName("centerfreq1");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.ScanId).HasColumnName("scan_id");
            entity.Property(e => e.Ssid).HasColumnName("ssid");
            entity.HasOne(d => d.Scan).WithMany(p => p.AccessPoints)
                .HasForeignKey(d => d.ScanId)
                .HasConstraintName("access_point_scan_id_fkey");
        });


        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("building_pkey");
            entity.ToTable("building", "saitynai");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Name).HasColumnName("name");
        });


        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("floor_pkey");
            entity.ToTable("floor", "saitynai");
            entity.HasIndex(e => e.BuildingId, "idx_floor_building_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuildingId).HasColumnName("building_id");
            entity.Property(e => e.FloorNumber).HasColumnName("floor_number");
            entity.Property(e => e.FloorPlanPath).HasColumnName("floor_plan_path");
            entity.HasOne(d => d.Building).WithMany(p => p.Floors)
                .HasForeignKey(d => d.BuildingId)
                .HasConstraintName("floor_building_id_fkey");
        });


        modelBuilder.Entity<Point>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("point_pkey");
            entity.ToTable("point", "saitynai");
            entity.HasIndex(e => e.FloorId, "idx_point_floor_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApCount).HasColumnName("ap_count");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.Latitude).HasPrecision(9, 6).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasPrecision(9, 6).HasColumnName("longitude");
            entity.HasOne(d => d.Floor).WithMany(p => p.Points)
                .HasForeignKey(d => d.FloorId)
                .HasConstraintName("point_floor_id_fkey");
        });


        modelBuilder.Entity<Scan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("scan_pkey");
            entity.ToTable("scan", "saitynai");
            entity.HasIndex(e => e.PointId, "idx_scan_point_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApCount).HasColumnName("ap_count");
            entity.Property(e => e.Filters).HasColumnName("filters");
            entity.Property(e => e.PointId).HasColumnName("point_id");
            entity.Property(e => e.ScannedAt).HasColumnName("scanned_at").HasDefaultValueSql("now()");
            entity.HasOne(d => d.Point).WithMany(p => p.Scans)
                .HasForeignKey(d => d.PointId)
                .HasConstraintName("scan_point_id_fkey");
        });


        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
