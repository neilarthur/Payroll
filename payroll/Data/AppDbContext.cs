using Microsoft.EntityFrameworkCore;
using payroll.Model;

namespace payroll.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
    }
}
