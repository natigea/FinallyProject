using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedDate).IsRequired();
        builder.Property(entity => entity.UpdatedDate).IsRequired();
        builder.Property(entity => entity.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
