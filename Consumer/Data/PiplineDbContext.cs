using Microsoft.EntityFrameworkCore;
using Consumer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Consumer.Data;

public class PiplineDbContext : DbContext
{
    public PiplineDbContext(DbContextOptions<PiplineDbContext> options)
        :base(options)
    { }

    public DbSet<StationInformationDto> StationInfo { get; set; }
    public DbSet<VehicleTypeDto> VehicleTypes{ get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StationInformationDto>().
            HasKey(s => s.StationId);

        modelBuilder.Entity<VehicleTypeDto>()
            .HasKey(s => s.VehicleTypeId);
    }
}
