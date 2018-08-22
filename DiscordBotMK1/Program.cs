using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace DiscordBotMK1
{
    class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            Data.Collection = new Dictionary<int, string>();
            using (var reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "collections.csv")))
            {
               
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Data.Collection.Add(Convert.ToInt32(values[0]), values[1]);
                }
            }

            Data.Recipes = new Dictionary<int, string>();
            using (var reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "recipes.csv")))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    string value = line.Substring(6, line.Length - 6);
                    Data.Recipes.Add(Convert.ToInt32(values[0]), value.Replace(',',' ').Trim());
                }
            }

            _client = new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000
                }
                );
            _commands = new CommandService(
                new CommandServiceConfig     // Add the command service to the service provider
            {
                DefaultRunMode = RunMode.Async,     // Force all commands to run async
                LogLevel = LogSeverity.Verbose
            } );

            // Avoid hard coding your token. Use an external source instead in your code.
            string token = System.Configuration.ConfigurationSettings.AppSettings["Token2"];

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<Random>()
                .BuildServiceProvider();

            await InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null)
                return;
            
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!( message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos) ))
                return;
            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services);
        }
    }
}
