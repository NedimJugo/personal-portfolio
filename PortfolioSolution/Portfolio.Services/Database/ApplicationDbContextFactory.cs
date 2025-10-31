using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql.EntityFrameworkCore.PostgreSQL;


namespace Portfolio.Services.Database
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=PortfolioDb;Username=postgres;Password=NewStrongPassword123!;"); // temporary design-time connection

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
