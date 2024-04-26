using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System;

namespace Pat.Modules
{
    public class Commands : InteractionModuleBase<SocketInteractionContext>
    {
        private IServiceScope _scope;
        public DataAccessLayer _dataAccessLayer;

        public Commands(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
            _dataAccessLayer = _scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [SlashCommand("invite", "Create a new invite with 1 use")]
        public async Task Invite(SocketTextChannel channel, IRole role)
        {
            await RespondAsync("Working on it...", ephemeral: true);

            if (role.Id == Context.Guild.EveryoneRole.Id)
            {
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Content = "Invalid role.");
                return;
            }

            var invite = await channel.CreateInviteAsync(maxAge: null, maxUses: 2);

            await _dataAccessLayer.CreateInvite(Context.Guild.Id, role.Id, invite.Code);

            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Content = $"Success!\n{invite.Url}");
        }
    }
}