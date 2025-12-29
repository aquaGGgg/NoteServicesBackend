using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notes.Domain.Notes;

namespace Notes.Infrastructure.Persistence.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> b)
    {
        b.ToTable("notes");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        b.Property(x => x.UserId).IsRequired();
        b.HasIndex(x => x.UserId);

        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Content).IsRequired();
        b.Property(x => x.IsArchived).IsRequired();

        b.Property(x => x.CreatedAtUnix).IsRequired();
        b.Property(x => x.UpdatedAtUnix).IsRequired();

        b.HasOne<Notes.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
