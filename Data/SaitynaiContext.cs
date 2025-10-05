using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;

    public class SaitynaiContext : DbContext
    {
        public SaitynaiContext (DbContextOptions<SaitynaiContext> options)
            : base(options)
        {
        }

        public DbSet<Saitynai.Models.AccessPoint> AccessPoint { get; set; } = default!;

public DbSet<Saitynai.Models.Building> Building { get; set; } = default!;

public DbSet<Saitynai.Models.Floor> Floor { get; set; } = default!;

public DbSet<Saitynai.Models.Point> Point { get; set; } = default!;

public DbSet<Saitynai.Models.Scan> Scan { get; set; } = default!;
    }
