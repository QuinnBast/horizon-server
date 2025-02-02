﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Common.Internal.Logging;
using RT.Common;
using RT.Models;
using Server.Common;
using Server.Database.Models;
using Server.Medius.PluginArgs;
using Server.Plugins;
using Server.Plugins.Interface;

namespace Server.Medius.Models
{
    public class Game
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Game>();

        public static int IdCounter = 1;

        public class GameClient
        {
            public ClientObject Client;

            public int DmeId;
            public bool InGame;
        }

        public int Id = 0;
        public int DMEWorldId = -1;
        public int ApplicationId = 0;
        public List<GameClient> Clients = new List<GameClient>();
        public string GameName;
        public string GamePassword;
        public string SpectatorPassword;
        public byte[] GameStats = new byte[Constants.GAMESTATS_MAXLEN];
        public MediusGameHostType GameHostType;
        public int MinPlayers;
        public int MaxPlayers;
        public int GameLevel;
        public int PlayerSkillLevel;
        public int RulesSet;
        public string Metadata;
        public int GenericField1;
        public int GenericField2;
        public int GenericField3;
        public int GenericField4;
        public int GenericField5;
        public int GenericField6;
        public int GenericField7;
        public int GenericField8;
        public MediusWorldStatus WorldStatus => _worldStatus;
        public MediusWorldAttributesType Attributes;
        public DMEObject DMEServer;
        public Channel ChatChannel;
        public ClientObject Host;

        public string AccountIdsAtStart => accountIdsAtStart;
        public DateTime UtcTimeCreated => utcTimeCreated;
        public DateTime? UtcTimeStarted => utcTimeStarted;
        public DateTime? UtcTimeEnded => utcTimeEnded;

        protected MediusWorldStatus _worldStatus = MediusWorldStatus.WorldPendingCreation;
        protected bool hasHostJoined = false;
        protected string accountIdsAtStart;
        protected DateTime utcTimeCreated;
        protected DateTime? utcTimeStarted;
        protected DateTime? utcTimeEnded;
        protected DateTime? utcTimeEmpty;
        protected bool destroyed = false;

        public uint Time => (uint)(Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalMilliseconds;

        public int PlayerCount => Clients.Count(x => x != null && x.Client.IsConnected && x.InGame);

        public virtual bool ReadyToDestroy => WorldStatus == MediusWorldStatus.WorldClosed && utcTimeEmpty.HasValue && (Utils.GetHighPrecisionUtcTime() - utcTimeEmpty)?.TotalSeconds > 1f;

        public Game(ClientObject client, IMediusRequest createGame, Channel chatChannel, DMEObject dmeServer)
        {
            if (createGame is MediusCreateGameRequest r)
                FromCreateGameRequest(r);
            else if (createGame is MediusCreateGameRequest1 r1)
                FromCreateGameRequest1(r1);

            Id = IdCounter++;

            utcTimeCreated = Utils.GetHighPrecisionUtcTime();
            utcTimeEmpty = null;
            DMEServer = dmeServer;
            ChatChannel = chatChannel;
            ChatChannel?.RegisterGame(this);
            Host = client;
            SetWorldStatus(MediusWorldStatus.WorldPendingCreation).Wait();

            Logger.Info($"Game {Id}:{GameName}: Created by {client}");
        }

        public GameDTO ToGameDTO()
        {
            return new GameDTO()
            {
                AppId = this.ApplicationId,
                GameCreateDt = this.utcTimeCreated,
                GameEndDt = this.utcTimeEnded,
                GameStartDt = this.utcTimeStarted,
                GameHostType = this.GameHostType.ToString(),
                GameId = this.Id,
                GameLevel = this.GameLevel,
                GameName = this.GameName,
                GameStats = this.GameStats,
                GenericField1 = this.GenericField1,
                GenericField2 = this.GenericField2,
                GenericField3 = this.GenericField3,
                GenericField4 = this.GenericField4,
                GenericField5 = this.GenericField5,
                GenericField6 = this.GenericField6,
                GenericField7 = this.GenericField7,
                GenericField8 = this.GenericField8,
                MaxPlayers = this.MaxPlayers,
                MinPlayers = this.MinPlayers,
                PlayerCount = this.PlayerCount,
                PlayerSkillLevel = this.PlayerSkillLevel,
                RuleSet = this.RulesSet,
                Metadata = this.Metadata,
                WorldStatus = this.WorldStatus.ToString(),
                PlayerListCurrent = GetActivePlayerList(),
                PlayerListStart = accountIdsAtStart,
                Destroyed = this.destroyed
            };
        }

        private void FromCreateGameRequest(MediusCreateGameRequest createGame)
        {
            ApplicationId = createGame.ApplicationID;
            GameName = createGame.GameName;
            MinPlayers = createGame.MinPlayers;
            MaxPlayers = createGame.MaxPlayers;
            GameLevel = createGame.GameLevel;
            PlayerSkillLevel = createGame.PlayerSkillLevel;
            RulesSet = createGame.RulesSet;
            GenericField1 = createGame.GenericField1;
            GenericField2 = createGame.GenericField2;
            GenericField3 = createGame.GenericField3;
            GenericField4 = createGame.GenericField4;
            GenericField5 = createGame.GenericField5;
            GenericField6 = createGame.GenericField6;
            GenericField7 = createGame.GenericField7;
            GenericField8 = createGame.GenericField8;
            GamePassword = createGame.GamePassword;
            SpectatorPassword = createGame.SpectatorPassword;
            GameHostType = createGame.GameHostType;
            Attributes = createGame.Attributes;
        }

        private void FromCreateGameRequest1(MediusCreateGameRequest1 createGame)
        {
            ApplicationId = createGame.ApplicationID;
            GameName = createGame.GameName;
            MinPlayers = createGame.MinPlayers;
            MaxPlayers = createGame.MaxPlayers;
            GameLevel = createGame.GameLevel;
            PlayerSkillLevel = createGame.PlayerSkillLevel;
            RulesSet = createGame.RulesSet;
            GenericField1 = createGame.GenericField1;
            GenericField2 = createGame.GenericField2;
            GenericField3 = createGame.GenericField3;
            GenericField4 = 0;
            GenericField5 = 0;
            GenericField6 = 0;
            GenericField7 = 0;
            GenericField8 = 0;
            GamePassword = createGame.GamePassword;
            SpectatorPassword = createGame.SpectatorPassword;
            GameHostType = createGame.GameHostType;
            Attributes = createGame.Attributes;
        }

        public string GetActivePlayerList()
        {
            return String.Join(",", this.Clients?.Select(x => x.Client.AccountId.ToString()).Where(x => x != null));
        }

        public virtual async Task Tick()
        {
            // Remove timedout clients
            for (int i = 0; i < Clients.Count; ++i)
            {
                var client = Clients[i];

                if (client == null || client.Client == null || !client.Client.IsConnected || client.Client.CurrentGame?.Id != Id)
                {
                    Clients.RemoveAt(i);
                    --i;
                }
            }

            // Auto close when everyone leaves or if host fails to connect after timeout time
            if (!utcTimeEmpty.HasValue && Clients.Count(x=>x.InGame) == 0 && (hasHostJoined || (Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalSeconds > Program.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds))
            {
                utcTimeEmpty = Utils.GetHighPrecisionUtcTime();
                await SetWorldStatus(MediusWorldStatus.WorldClosed);
            }
        }

        public virtual async Task OnMediusServerConnectNotification(MediusServerConnectNotification notification)
        {
            var player = Clients.FirstOrDefault(x => x.Client.SessionKey == notification.PlayerSessionKey);
            if (player == null)
                return;

            switch (notification.ConnectEventType)
            {
                case MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_CONNECT:
                    {
                        await OnPlayerJoined(player);
                        break;
                    }
                case MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_DISCONNECT:
                    {
                        await OnPlayerLeft(player);
                        break;
                    }
            }
        }

        protected virtual async Task OnPlayerJoined(GameClient player)
        {
            player.InGame = true;

            if (player.Client == Host)
                hasHostJoined = true;

            // Send to plugins
            await Program.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_JOINED_GAME, new OnPlayerGameArgs() { Player = player.Client, Game = this });
        }

        public virtual void AddPlayer(ClientObject client)
        {
            // Don't add again
            if (Clients.Any(x => x.Client == client))
                return;

            //
            Logger.Info($"Game {Id}:{GameName}: {client} added.");

            Clients.Add(new GameClient()
            {
                Client = client,
                DmeId = client.DmeClientId ?? -1
            });

            // Inform the client of any custom game mode
            //client.CurrentChannel?.SendSystemMessage(client, $"Gamemode is {CustomGamemode?.FullName ?? "default"}.");
        }

        protected virtual async Task OnPlayerLeft(GameClient player)
        {
            //
            Logger.Info($"Game {Id}:{GameName}: {player.Client} left.");

            //
            player.InGame = false;

            // Update player object
            await player.Client.LeaveGame(this);
            // player.Client.LeaveChannel(ChatChannel);

            // Remove from collection
            await RemovePlayer(player.Client);
        }

        public virtual async Task RemovePlayer(ClientObject client)
        {
            //
            Logger.Info($"Game {Id}:{GameName}: {client} removed.");

            // Remove host
            if (Host == client)
            {
                // Send to plugins
                await Program.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_HOST_LEFT, new OnPlayerGameArgs() { Player = client, Game = this });

                Host = null;
            }

            // Send to plugins
            await Program.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_LEFT_GAME, new OnPlayerGameArgs() { Player = client, Game = this });

            // Remove from clients list
            Clients.RemoveAll(x => x.Client == client);
        }

        public virtual async Task OnEndGameReport(MediusEndGameReport report)
        {
            await SetWorldStatus(MediusWorldStatus.WorldClosed);
        }

        public virtual void OnPlayerReport(MediusPlayerReport report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != Id)
                return;
        }

        public virtual async Task OnWorldReport(MediusWorldReport report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != Id)
                return;

            GameName = report.GameName;
            MinPlayers = report.MinPlayers;
            MaxPlayers = report.MaxPlayers;
            GameLevel = report.GameLevel;
            PlayerSkillLevel = report.PlayerSkillLevel;
            RulesSet = report.RulesSet;
            GenericField1 = report.GenericField1;
            GenericField2 = report.GenericField2;
            GenericField3 = report.GenericField3;
            GenericField4 = report.GenericField4;
            GenericField5 = report.GenericField5;
            GenericField6 = report.GenericField6;
            GenericField7 = report.GenericField7;
            GenericField8 = report.GenericField8;
            GameStats = report.GameStats;

            // Once the world has been closed then we force it closed.
            // This is because when the host hits 'Play Again' they tell the server the world has closed (EndGameReport)
            // but the existing clients tell the server the world is still active.
            // This gives the host a "Game Name Already Exists" when they try to remake with the same name.
            // This just fixes that. At the cost of the game not showing after a host leaves a game.
            if (WorldStatus != MediusWorldStatus.WorldClosed && WorldStatus != report.WorldStatus)
            {
                await SetWorldStatus(report.WorldStatus);
            }
            else
            {
                // Update db
                if (!utcTimeEnded.HasValue)
                    _ = Program.Database.UpdateGame(this.ToGameDTO());
            }
        }

        public virtual Task GameCreated()
        {
            return Task.CompletedTask;
        }

        public void OnWorldReport(MediusWorldReport0 report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != Id)
                return;

            GameName = report.GameName;
            MinPlayers = report.MinPlayers;
            MaxPlayers = report.MaxPlayers;
            GameLevel = report.GameLevel;
            PlayerSkillLevel = report.PlayerSkillLevel;
            RulesSet = report.RulesSet;
            GenericField1 = report.GenericField1;
            GenericField2 = report.GenericField2;
            GenericField3 = report.GenericField3;

            // Once the world has been closed then we force it closed.
            // This is because when the host hits 'Play Again' they tell the server the world has closed (EndGameReport)
            // but the existing clients tell the server the world is still active.
            // This gives the host a "Game Name Already Exists" when they try to remake with the same name.
            // This just fixes that. At the cost of the game not showing after a host leaves a game.
            if (WorldStatus != MediusWorldStatus.WorldClosed && WorldStatus != report.WorldStatus)
            {
                SetWorldStatus(report.WorldStatus);
            }
            else
            {
                // Update db
                if (!utcTimeEnded.HasValue)
                    _ = Program.Database.UpdateGame(this.ToGameDTO());
            }
        }


        public virtual async Task EndGame()
        {
            // destroy flag
            destroyed = true;

            //
            Logger.Info($"Game {Id}:{GameName}: EndGame() called.");

            // Send to plugins
            await Program.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_DESTROYED, new OnGameArgs() { Game = this });

            // Remove players from game world
            while (Clients.Count > 0)
            {
                var client = Clients[0].Client;
                if (client == null)
                {
                    Clients.RemoveAt(0);
                }
                else
                {
                    await client.LeaveGame(this);
                    // client.LeaveChannel(ChatChannel);
                }
            }


            // Unregister from channel
            ChatChannel?.UnregisterGame(this);

            // Send end game
            if (this.DMEWorldId > 0)
            {
                DMEServer?.Queue(new MediusServerEndGameRequest()
                {
                    WorldID = this.DMEWorldId,
                    BrutalFlag = false
                });
            }

            // Delete db entry if game hasn't started
            // Otherwise do a final update
            if (!utcTimeStarted.HasValue)
            {
                _ = Program.Database.DeleteGame(this.Id);
            }
            else
            {
                _ = Program.Database.UpdateGame(this.ToGameDTO());
            }
        }

        public virtual async Task SetWorldStatus(MediusWorldStatus status)
        {
            if (WorldStatus == status)
                return;

            _worldStatus = status;

            switch (status)
            {
                case MediusWorldStatus.WorldActive:
                    {
                        utcTimeStarted = Utils.GetHighPrecisionUtcTime();
                        accountIdsAtStart = GetActivePlayerList();

                        // Send to plugins
                        await Program.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_STARTED, new OnGameArgs() { Game = this });
                        break;
                    }
                case MediusWorldStatus.WorldClosed:
                    {
                        utcTimeEnded = Utils.GetHighPrecisionUtcTime();

                        // Send to plugins
                        await Program .Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_ENDED, new OnGameArgs() { Game = this });
                        return;
                    }
            }

            // Update db
            if (!utcTimeEnded.HasValue)
                _ = Program.Database.UpdateGame(this.ToGameDTO());
        }
    }
}
