using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Infrastructure
{
    public class BotContext : DbContext
    {
        public DbSet<Invite> Invites { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("server=;user=;database=;port=;Connect Timeout=;", new MySqlServerVersion(new Version(8, 0, 21)));
    }

    public class Invite
    {
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public string InviteCode { get; set; }
        public ulong RoleId { get; set; }
        public int Uses { get; set; }
    }
}