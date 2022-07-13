using Microsoft.EntityFrameworkCore;

namespace Intro.DAL.Context
{
    public class IntroContext : DbContext
    {
        public DbSet<Entities.User> Users { get; set; }

        public IntroContext(DbContextOptions options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Этот метод вызывается когда создается модель - 
            //  БД из кода. Здесь можно задать начальные настройки
            modelBuilder.ApplyConfiguration(new UsersConfiguration());

        }
    }
}
