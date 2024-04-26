1) The bot's token must be put in the Program.cs file
2) The db connection string has to be filled in the BotContext.cs file
3) DB migrations need to be added and updated
4) Create a new invite by using the /invite command, you do not create a command manually
5) When the user is invited and the role is granted, the invite is automatically deleted so if you see that it is active with max 2 uses don't mind it.
6) Make sure the bot has all 3 intents enabled in the developer portal
7) Make sure the bot is private from the developer portal
8) Entrypoint is InviteBot\InviteBot\bin\Debug-Release\net8.0\InviteBot.dll