using Interview.Domain.Categories;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomConfiguration : EntityTypeConfigurationBase<Room>
{
    protected override void ConfigureCore(EntityTypeBuilder<Room> builder)
    {
        builder.Property(room => room.Name).IsRequired().HasMaxLength(70);
        builder.Property(room => room.ScheduleStartTime).IsRequired();
        builder.Property(room => room.Status)
            .HasConversion(e => e.Value, e => SERoomStatus.FromValue(e))
            .IsRequired()
            .HasDefaultValue(SERoomStatus.New);
        builder
            .HasOne<Domain.Rooms.RoomConfigurations.RoomConfiguration>(room => room.Configuration)
            .WithOne(e => e.Room)
            .HasForeignKey<Domain.Rooms.RoomConfigurations.RoomConfiguration>(e => e.Id);
        builder
            .HasOne<RoomTimer>(room => room.Timer)
            .WithOne(roomTimer => roomTimer.Room)
            .HasForeignKey<RoomTimer>(roomTimer => roomTimer.Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany<Tag>(e => e.Tags).WithMany();
        builder.Property(room => room.AccessType)
            .HasConversion(e => e.Value, e => SERoomAccessType.FromValue(e))
            .IsRequired()
            .HasDefaultValue(SERoomAccessType.Public);
        builder.Property(e => e.QuestionTreeId);
        builder.HasOne(e => e.QuestionTree)
            .WithMany()
            .HasForeignKey(e => e.QuestionTreeId);

        var roomTypes = SERoomType.List.OrderBy(e => e.Value).Select(e => e.Value + ": " + e.Name);
        builder.Property(e => e.Type)
            .HasConversion(e => e.Value, e => SERoomType.FromValue(e))
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(1)
            .HasDefaultValue(SERoomType.Standard)
            .HasComment("Available values: " + string.Join(", ", roomTypes));
    }
}
