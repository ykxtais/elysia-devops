using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ElysiaAPI.Domain.Entity;

namespace ElysiaAPI.Infrastructure.Mappings
{
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("UsuarioCsharp");

            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Id).ValueGeneratedOnAdd();

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(254);

            builder.Property(u => u.Senha)
                .IsRequired()
                .HasMaxLength(100); 

            builder.Property(u => u.Cpf)
                .IsRequired()
                .HasMaxLength(11); 
            
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UK_UsuarioCsharp_Email");

            builder.HasIndex(u => u.Cpf)
                .IsUnique()
                .HasDatabaseName("UK_UsuarioCsharp_Cpf");
        }
    }
}