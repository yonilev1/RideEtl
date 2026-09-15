using Microsoft.EntityFrameworkCore;
using O.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consumer.Models;
namespace Consumer.Data;

public class PiplineDbContext : DbContext
{
    public PiplineDbContext(DbContextOptions<PiplineDbContext> options)
        :base(options)
    { }

    public DbSet<StationInformation> StationInfo = null!;
    public DbSet<VehicleType> VehicleTypes = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StationInformation>().
            HasKey(s => s.StationId);

        modelBuilder.Entity<VehicleType>()
            .HasKey(s => s.VehicleTypeId);
    }
}
