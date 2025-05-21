using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ElysiaAPI.Domain.Entity;

namespace ElysiaAPI.Infrastructure.Mappings
{
    public class MotoMapping : IEntityTypeConfiguration<Moto>
    {
        public void Configure(EntityTypeBuilder<Moto> builder)
        {
            builder.ToTable("Moto");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                   .ValueGeneratedOnAdd(); 

            builder.Property(m => m.Placa)
                   .HasMaxLength(7); 

            builder.Property(m => m.Marca)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(m => m.Modelo)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(m => m.Ano)
                   .IsRequired();
        }
    }
}
