using Microsoft.EntityFrameworkCore;
using WikiKnowledge.Models;


namespace WikiKnowledge.Data  
{
    public class CronologiaContext: DbContext
    {
        public CronologiaContext(DbContextOptions<CronologiaContext> options) : base(options)
        {
        }

        DbSet<Cronologia> CronologiaEntries{get; set;}
    }
}
