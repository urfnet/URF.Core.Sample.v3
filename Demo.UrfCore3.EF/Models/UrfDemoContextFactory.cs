using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Demo.UrfCore3.EF.Models
{
    public class UrfDemoContextFactory : IDesignTimeDbContextFactory<UrfDemoContext>
    {
        public UrfDemoContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UrfDemoContext>();
            optionsBuilder.UseSqlServer(Constants.ConnectionStrings.LocalDbConnection);
            return new UrfDemoContext(optionsBuilder.Options);
        }
    }
}
