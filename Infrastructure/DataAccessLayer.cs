using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class DataAccessLayer
    {
        private BotContext _dbContext;

        public DataAccessLayer(BotContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateInvite(ulong server_id, ulong role_id, string invite_code)
        {
            _dbContext.Add(new Invite { ServerId = server_id, RoleId = role_id, InviteCode = invite_code });
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteInvite(int id)
        {
            var invite = await _dbContext.Invites.Where(x => x.Id == id).FirstOrDefaultAsync();
            _dbContext.Remove(invite);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Invite>> GetInvites(ulong server_id)
        {
            return await _dbContext.Invites.Where(x => x.ServerId == server_id).ToListAsync();
        }
    }
}