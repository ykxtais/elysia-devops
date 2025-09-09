using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ElysiaAPI.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto>    Motos    { get; set; } = default!;
        public DbSet<Vaga>    Vagas    { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MotoMapping());
            modelBuilder.ApplyConfiguration(new VagaMapping());
            modelBuilder.ApplyConfiguration(new UsuarioMapping());

            base.OnModelCreating(modelBuilder);
        }
    }
}