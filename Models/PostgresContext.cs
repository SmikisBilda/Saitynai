using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Saitynai.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessPoint> AccessPoints { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<Point> Points { get; set; }

    public virtual DbSet<Scan> Scans { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("access_point_pkey");

            entity.ToTable("access_point", "saitynai");

            entity.HasIndex(e => e.ScanId, "idx_ap_scan_id");

            entity.HasIndex(e => new { e.ScanId, e.Bssid }, "ux_ap_scan_bssid").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('access_point_id_seq'::regclass)")
                .HasColumnName("id");
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

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('building_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("floor_pkey");

            entity.ToTable("floor", "saitynai");

            entity.HasIndex(e => e.BuildingId, "idx_floor_building_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('floor_id_seq'::regclass)")
                .HasColumnName("id");
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

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('point_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ApCount).HasColumnName("ap_count");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.Latitude)
                .HasPrecision(9, 6)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(9, 6)
                .HasColumnName("longitude");

            entity.HasOne(d => d.Floor).WithMany(p => p.Points)
                .HasForeignKey(d => d.FloorId)
                .HasConstraintName("point_floor_id_fkey");
        });

modelBuilder.Entity<Scan>(entity =>
{
    entity.HasKey(e => e.Id).HasName("scan_pkey");
    entity.ToTable("scan", "saitynai");

    entity.HasIndex(e => e.PointId, "idx_scan_point_id");

    entity.Property(e => e.Id)
        .HasDefaultValueSql("nextval('scan_id_seq'::regclass)")
        .HasColumnName("id");

    entity.Property(e => e.ApCount).HasColumnName("ap_count");
    entity.Property(e => e.Filters).HasColumnName("filters");
    entity.Property(e => e.PointId).HasColumnName("point_id");
    entity.Property(e => e.ScannedAt)
        .HasColumnName("scanned_at")
        .ValueGeneratedOnAdd()
        .HasDefaultValueSql("now()"); 
    

    entity.HasOne(d => d.Point).WithMany(p => p.Scans)
        .HasForeignKey(d => d.PointId)
        .HasConstraintName("scan_point_id_fkey");
});

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
