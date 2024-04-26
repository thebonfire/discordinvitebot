using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Pat.Services
{
    public class CommandHandler : DiscordClientService
    {
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private IServiceProvider _provider;

        public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider provider, ILogger<CommandHandler> logger) : base(client, logger)
        {
            _provider = provider;
            _client = client;
            _commands = commands;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _client.InteractionCreated += OnInteractionCreated;
            _commands.SlashCommandExecuted += OnSlashCommandExecuted;

            _client.UserJoined += OnUserJoined;

            _client.Ready += OnReadyAsync;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
            await _client.WaitForReadyAsync(cancellationToken);
        }

        private async Task<List<Invite>> GetInvites(ulong server_id)
        {
            using var scope = _provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            return await dataAccessLayer.GetInvites(server_id);
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            var old_invites = await GetInvites(user.Guild.Id);
            if (old_invites.Count == 0) return;

            var new_invites = await user.Guild.GetInvitesAsync();
            foreach (var invite_old in old_invites)
            {
                var invite_new = new_invites.Where(x => x.Code == invite_old.InviteCode).FirstOrDefault();
                if (invite_new == null) continue;

                if (invite_new.Uses > invite_old.Uses)
                {
                    var role = user.Guild.Roles.Where(x => x.Id == invite_old.RoleId).FirstOrDefault();
                    if (role == null) continue;

                    await user.AddRoleAsync(role);
                }

                await invite_new.DeleteAsync();
            }
        }

        private async Task OnReadyAsync()
        {
            await _client.SetActivityAsync(new Game("created by pavlos21", ActivityType.Playing));
            Console.WriteLine("Activity has been set!");

            await _commands.RegisterCommandsGloballyAsync();
            Console.WriteLine("Guild commands have been registered!");
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _commands.ExecuteCommandAsync(ctx, _provider);
        }

        private async Task OnSlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            string title = string.Empty;
            string description = string.Empty;

            switch (result.Error)
            {
                case InteractionCommandError.UnknownCommand:
                    title = "Not found";
                    description = "The command that was provided could not be found";
                    break;
                case InteractionCommandError.ConvertFailed:
                    title = "Convertion failed";
                    description = "The slash command failed to be converted to a TypeReader";
                    break;
                case InteractionCommandError.BadArgs:
                    title = "Command used incorrectly";
                    description = "Please provide the correct amount of parameters";
                    break;
                case InteractionCommandError.Exception:
                    title = "Exception thrown";
                    description = "The interaction threw an exception";
                    break;
                case InteractionCommandError.Unsuccessful:
                    title = "Command ran unsuccessfully";
                    description = "The command was ran unsuccessfully";
                    break;
                case InteractionCommandError.UnmetPrecondition:
                    title = "Access denied";
                    description = "You or the bot do not meet the required preconditions";
                    break;
                case InteractionCommandError.ParseFailed:
                    title = "Invalid argument";
                    description = "The argument that you provided could not be parsed correctly";
                    break;
                default:
                    title = "An error occurred";
                    description = "An error occurred while trying to run this command";
                    break;
            }

            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .Build();

            try
            {
                await context.Interaction.RespondAsync(embed: embed);
            }
            catch
            {
                await context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "";
                    x.Embed = embed;
                });
            }
        }
    }
}
