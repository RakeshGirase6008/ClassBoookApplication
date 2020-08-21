using ClassBookApplication.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClassBookApplication.DataContext
{
    public class ClassBookLogsContext : DbContext
    {
        public ClassBookLogsContext(DbContextOptions<ClassBookLogsContext> options)
            : base(options)
        {
        }

        #region Common
        public DbSet<Logs> Logs { get; set; }

        #endregion
    }
}
