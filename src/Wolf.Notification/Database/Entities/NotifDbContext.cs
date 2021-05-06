using Microsoft.EntityFrameworkCore;
using Wolf.Notification.Models;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
	public partial class NotifDbContext : DbContext
    {
        public NotifDbContext()
        {
        }

        public NotifDbContext(DbContextOptions<NotifDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<GeneratedMessage> GeneratedMessages { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<MessageRecipient> MessageRecipients { get; set; }
        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<Recipient> Recipients { get; set; }
        public virtual DbSet<RecipientType> RecipientTypes { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<TemplateRecipient> TemplateRecipients { get; set; }

        public virtual DbSet<MsgTimeSeria> MsgTimeSeries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Cyrillic_General_CI_AS");

            modelBuilder.Entity<MsgTimeSeria>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.DayDate).HasColumnName("day_date");
                entity.Property(e => e.MessageCount).HasColumnName("message_count");
                entity.Property(e => e.TemplateId).HasColumnName("template_id");
            });

            modelBuilder.Entity<GeneratedMessage>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.ToTable("generated_message", "notif");

                entity.Property(e => e.MessageId)
                    .ValueGeneratedNever()
                    .HasColumnName("message_id");

                entity.Property(e => e.Body).HasColumnName("body");

                entity.Property(e => e.Subject).HasColumnName("subject");

                entity.HasOne(d => d.Message)
                    .WithOne(p => p.GeneratedMessage)
                    .HasForeignKey<GeneratedMessage>(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_generated_message_message");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .HasName("message_pk")
                    .IsClustered(false);

                entity.ToTable("message", "notif");

                entity.Property(e => e.MessageId)
                    .HasColumnName("message_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateProcessed)
                    .HasColumnType("datetime")
                    .HasColumnName("date_processed");

                entity.Property(e => e.DateSent)
                    .HasColumnType("datetime")
                    .HasColumnName("date_sent");

                entity.Property(e => e.FromRecipientId).HasColumnName("from_recipient_id");

                entity.Property(e => e.ProviderCode)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("provider_code");

                entity.Property(e => e.TemplateId).HasColumnName("template_id");

                entity.Property(e => e.TokenData).HasColumnName("token_data");

                entity.HasOne(d => d.FromRecipient)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.FromRecipientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_message_recipient");

                entity.HasOne(d => d.ProviderCodeNavigation)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ProviderCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_message_provider");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_template_template_id_fk");
            });

            modelBuilder.Entity<MessageRecipient>(entity =>
            {
                entity.HasKey(e => e.MrId);

                entity.ToTable("message_recipient", "notif");

                entity.Property(e => e.MrId).HasColumnName("mr_id");

                entity.Property(e => e.MessageId).HasColumnName("message_id");

                entity.Property(e => e.RecipientId).HasColumnName("recipient_id");

                entity.Property(e => e.TypeCode)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("type_code");

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.MessageRecipients)
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_message_recipient_message");

                entity.HasOne(d => d.Recipient)
                    .WithMany(p => p.MessageRecipients)
                    .HasForeignKey(d => d.RecipientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_message_recipient_recipient");

                entity.HasOne(d => d.TypeCodeNavigation)
                    .WithMany(p => p.MessageRecipients)
                    .HasForeignKey(d => d.TypeCode)
                    .HasConstraintName("FK_message_recipient_recipient_type");
            });

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.HasKey(e => e.ProviderCode);

                entity.ToTable("provider", "notif");

                entity.Property(e => e.ProviderCode)
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("provider_code");

                entity.Property(e => e.Description).HasColumnName("description");
            });

            modelBuilder.Entity<Recipient>(entity =>
            {
                entity.ToTable("recipient", "notif");

                entity.Property(e => e.RecipientId).HasColumnName("recipient_id");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasColumnName("address");

                entity.Property(e => e.Name)
                    .HasMaxLength(1024)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<RecipientType>(entity =>
            {
                entity.HasKey(e => e.TypeCode);

                entity.ToTable("recipient_type", "notif");

                entity.Property(e => e.TypeCode)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("type_code");

                entity.Property(e => e.Description).HasColumnName("description");
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("template", "notif");

                entity.Property(e => e.TemplateId)
                    .ValueGeneratedNever()
                    .HasColumnName("template_id");

                entity.Property(e => e.DefaultFromRecipientId).HasColumnName("default_from_recipient_id");

                entity.Property(e => e.DefaultProviderCode)
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("default_provider_code");

                entity.Property(e => e.TemplateBody).HasColumnName("template_body");

                entity.Property(e => e.TemplateName)
                    .IsRequired()
                    .HasMaxLength(512)
                    .HasColumnName("template_name");

                entity.Property(e => e.TemplateSubject).HasColumnName("template_subject");

                entity.HasOne(d => d.DefaultFromRecipient)
                    .WithMany(p => p.Templates)
                    .HasForeignKey(d => d.DefaultFromRecipientId)
                    .HasConstraintName("FK_template_recipient");

                entity.HasOne(d => d.DefaultProviderCodeNavigation)
                    .WithMany(p => p.Templates)
                    .HasForeignKey(d => d.DefaultProviderCode)
                    .HasConstraintName("FK_template_provider");
            });

            modelBuilder.Entity<TemplateRecipient>(entity =>
            {
                entity.HasKey(e => e.TrId);

                entity.ToTable("template_recipient", "notif");

                entity.Property(e => e.TrId).HasColumnName("tr_id");

                entity.Property(e => e.RecipientId).HasColumnName("recipient_id");

                entity.Property(e => e.TemplateId).HasColumnName("template_id");

                entity.Property(e => e.TypeCode)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("type_code");

                entity.HasOne(d => d.Recipient)
                    .WithMany(p => p.TemplateRecipients)
                    .HasForeignKey(d => d.RecipientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_template_recipient_recipient");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.TemplateRecipients)
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_template_recipient_template");

                entity.HasOne(d => d.TypeCodeNavigation)
                    .WithMany(p => p.TemplateRecipients)
                    .HasForeignKey(d => d.TypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_template_recipient_recipient_type");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
