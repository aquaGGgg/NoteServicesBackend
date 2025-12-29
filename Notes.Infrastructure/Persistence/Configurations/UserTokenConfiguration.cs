using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notes.Infrastructure.Persistence.Models;

namespace Notes.Infrastructure.Persistence.Configurations;

public sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> b)
    {
        b.ToTable("user_tokens");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        b.Property(x => x.UserId).IsRequired();
        b.HasIndex(x => x.UserId);

        b.Property(x => x.TokenHash).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.TokenHash).IsUnique();

        b.Property(x => x.CreatedAtUnix).IsRequired();
        b.Property(x => x.ExpiresAtUnix).IsRequired();
        b.Property(x => x.RevokedAtUnix);

        b.HasOne<Notes.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
