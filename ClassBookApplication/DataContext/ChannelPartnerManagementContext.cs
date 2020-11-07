using ClassBookApplication.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClassBookApplication.DataContext
{
    public class ChannelPartnerManagementContext : DbContext
    {
        public ChannelPartnerManagementContext(DbContextOptions<ChannelPartnerManagementContext> options)
            : base(options)
        {
        }

        public DbSet<Settings> Settings { get; set; }
    }
    
}
