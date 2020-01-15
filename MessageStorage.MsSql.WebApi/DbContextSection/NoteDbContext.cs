using MessageStorage.MsSql.WebApi.DbContextSection.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageStorage.MsSql.WebApi.DbContextSection
{
    public class NoteDbContext : DbContext
    {
        public NoteDbContext(DbContextOptions<NoteDbContext> options) : base(options)
        {
        }

        public virtual DbSet<NoteModel> NoteModel { get; set; }
    }
}