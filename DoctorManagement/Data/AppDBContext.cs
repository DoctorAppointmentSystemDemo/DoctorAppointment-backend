using DoctorManagement.Models.User;
using Microsoft.EntityFrameworkCore;

namespace DoctorManagement.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions options): base(options){ }
    public DbSet<User> Users { get; set; }
}
