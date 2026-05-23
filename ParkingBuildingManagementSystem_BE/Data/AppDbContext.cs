using Microsoft.EntityFrameworkCore;
using ParkingBuildingManagementSystem_BE.Models;

namespace ParkingBuildingManagementSystem_BE.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
