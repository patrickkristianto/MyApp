using Applications.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Applications.Data
{
    public class DBContext : IdentityDbContext<Users>
    {
        public DBContext(DbContextOptions options) : base(options)
        { 
        }
    }
}
