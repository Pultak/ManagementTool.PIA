using ManagementTool.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.DataAccess; 

public class ManToolDbContext : DbContext {
    public virtual DbSet<User> User { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //todo load from config
        optionsBuilder.UseNpgsql(@"Host=localhost;Database=entitycore;Username=postgres;Password=mypassword");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<User>(entity => {
            entity.ToTable("item", "account");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id")
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Pwd)
                .IsRequired()
                .HasColumnName("pwd");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasColumnName("full_name");
            entity.Property(e => e.PrimaryWorkplace)
                .IsRequired()
                .HasColumnName("primary_workplace");
            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasColumnName("email_address");
        });
        modelBuilder.HasSequence("item_id_seq", "account");

        /* todo do something like this
         
        var entryPoint = (from ep in dbContext.tbl_EntryPoint
            join e in dbContext.tbl_Entry on ep.EID equals e.EID
            join t in dbContext.tbl_Title on e.TID equals t.TID
            where e.OwnerID == user.UID
            select new
            {
                UID = e.OwnerID,
                TID = e.TID,
                Title = t.Title,
                EID = e.EID
            }).Take(10);
        */
    }
}