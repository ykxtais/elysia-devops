using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ElysiaAPI.Domain.Entity;

namespace ElysiaAPI.Infrastructure.Mappings
{
    public class VagaMapping : IEntityTypeConfiguration<Vaga>
    {
        public void Configure(EntityTypeBuilder<Vaga> builder)
        {
            builder.ToTable("Vaga");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                   .ValueGeneratedOnAdd(); 

            builder.Property(v => v.Status)
                   .HasMaxLength(20);  

            builder.Property(v => v.Numero)
                   .IsRequired();

            builder.Property(v => v.Patio)
                   .IsRequired()
                   .HasMaxLength(50);
        }
    }
}
