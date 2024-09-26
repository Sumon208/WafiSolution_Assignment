using Microsoft.EntityFrameworkCore;
using WafiSolution_Assignment.Models;
namespace WafiSolution_Assignment.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }

    }
}
