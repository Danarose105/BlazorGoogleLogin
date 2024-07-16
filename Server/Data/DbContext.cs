using Microsoft.EntityFrameworkCore;

namespace BlazorGoogleLogin.Server.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

      
    }
}
