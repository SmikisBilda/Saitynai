using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;

public class SaitynaiContext : DbContext
{
    public SaitynaiContext(DbContextOptions<SaitynaiContext> options)
        : base(options)
    {
    }

    // Existing DbSets
    public DbSet<Saitynai.Models.AccessPoint> AccessPoint { get; set; } = default!;
    public DbSet<Saitynai.Models.Building> Building { get; set; } = default!;
    public DbSet<Saitynai.Models.Floor> Floor { get; set; } = default!;
    public DbSet<Saitynai.Models.Point> Point { get; set; } = default!;
    public DbSet<Saitynai.Models.Scan> Scan { get; set; } = default!;

    // Add new DbSets for auth
    public DbSet<Saitynai.Models.User> User { get; set; } = default!;
    public DbSet<Saitynai.Models.Role> Role { get; set; } = default!;
    public DbSet<Saitynai.Models.Permission> Permission { get; set; } = default!;
    public DbSet<Saitynai.Models.UserRole> UserRole { get; set; } = default!;
    public DbSet<Saitynai.Models.RolePermission> RolePermission { get; set; } = default!;
}
