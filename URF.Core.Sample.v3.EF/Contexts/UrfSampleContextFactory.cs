using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace URF.Core.Sample.v3.EF.Contexts
{
    public class UrfSampleContextFactory : IDesignTimeDbContextFactory<UrfSampleContext>
    {
        public UrfSampleContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UrfSampleContext>();
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;initial catalog=UrfSample;Integrated Security=True; MultipleActiveResultSets=True");
            return new UrfSampleContext(optionsBuilder.Options);
        }
    }
}
