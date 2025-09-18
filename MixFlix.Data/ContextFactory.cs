using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MixFlix.Data
{
    public class ContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            return CreateDbContextPostgreSQL();
        }

        public static Context CreateDbContextPostgreSQL()
        {
            var host = "192.168.70.1";
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            Context.ConfigureOptions(optionsBuilder, @$"");

            return new Context(optionsBuilder.Options);
        }
    }
}
