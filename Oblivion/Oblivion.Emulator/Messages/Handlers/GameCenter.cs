using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Games the center load game.
        /// </summary>
        internal async Task GameCenterLoadGame()
        {
            var GameId = Request.GetInteger();

            ServerMessage gamesLeft = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterGamesLeftMessageComposer"));
            await gamesLeft.AppendIntegerAsync(GameId);
            await gamesLeft.AppendIntegerAsync(-1);
            await gamesLeft.AppendIntegerAsync(0);
            await Session.SendMessageAsync(gamesLeft);

            ServerMessage enterInGame = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterEnterInGameMessageComposer"));
            await enterInGame.AppendIntegerAsync(GameId);
            await enterInGame.AppendIntegerAsync(0);
            await Session.SendMessageAsync(enterInGame);

            ServerMessage achievements = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterGameAchievementsMessageComposer"));
            await achievements.AppendIntegerAsync(GameId);
            await achievements.AppendIntegerAsync(1);//count
            await achievements.AppendIntegerAsync(295);//id
            await achievements.AppendIntegerAsync(1);
            await achievements.AppendStringAsync("ACH_StoryChallengeChampion1");
            await achievements.AppendIntegerAsync(0);
            await achievements.AppendIntegerAsync(1);
            await achievements.AppendIntegerAsync(0);
            await achievements.AppendIntegerAsync(0);
            await achievements.AppendIntegerAsync(0);
            achievements.AppendBool(false);
            await achievements.AppendStringAsync("games");
            await achievements.AppendStringAsync("basejump");
            await achievements.AppendIntegerAsync(1);
            await achievements.AppendIntegerAsync(0);
            await achievements.AppendStringAsync("");
            await Session.SendMessageAsync(achievements);
/*
            ServerMessage weeklyLeaderboard = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLeaderboardMessageComposer"));
            weeklyLeaderboard.AppendInteger(2014);
            weeklyLeaderboard.AppendInteger(49);
            weeklyLeaderboard.AppendInteger(0);
            weeklyLeaderboard.AppendInteger(0);
            weeklyLeaderboard.AppendInteger(6526);
            weeklyLeaderboard.AppendInteger(1);
            weeklyLeaderboard.AppendInteger(Session.GetHabbo().Id);
            weeklyLeaderboard.AppendInteger(0);
            weeklyLeaderboard.AppendInteger(1);
            weeklyLeaderboard.AppendString(Session.GetHabbo().UserName);
            weeklyLeaderboard.AppendString(Session.GetHabbo().Look);
            weeklyLeaderboard.AppendString(Session.GetHabbo().Gender);
            weeklyLeaderboard.AppendInteger(1);
            weeklyLeaderboard.AppendInteger(18);
            await Session.SendMessageAsync(weeklyLeaderboard);

            ServerMessage weeklyLeaderboard2 = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLeaderboard2MessageComposer"));
            weeklyLeaderboard2.AppendInteger(2014);
            weeklyLeaderboard2.AppendInteger(49);
            weeklyLeaderboard2.AppendInteger(0);
            weeklyLeaderboard2.AppendInteger(0);
            weeklyLeaderboard2.AppendInteger(6526);
            weeklyLeaderboard2.AppendInteger(1);
            weeklyLeaderboard2.AppendInteger(Session.GetHabbo().Id);
            weeklyLeaderboard2.AppendInteger(0);
            weeklyLeaderboard2.AppendInteger(1);
            weeklyLeaderboard2.AppendString(Session.GetHabbo().UserName);
            weeklyLeaderboard2.AppendString(Session.GetHabbo().Look);
            weeklyLeaderboard2.AppendString(Session.GetHabbo().Gender);
            weeklyLeaderboard2.AppendInteger(0);
            weeklyLeaderboard2.AppendInteger(18);
            await Session.SendMessageAsync(weeklyLeaderboard2);

            ServerMessage weeklyLeaderboard3 = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLeaderboard2MessageComposer"));
            weeklyLeaderboard3.AppendInteger(2014);
            weeklyLeaderboard3.AppendInteger(48);
            weeklyLeaderboard3.AppendInteger(0);
            weeklyLeaderboard3.AppendInteger(1);
            weeklyLeaderboard3.AppendInteger(6526);
            weeklyLeaderboard3.AppendInteger(1);
            weeklyLeaderboard3.AppendInteger(Session.GetHabbo().Id);
            weeklyLeaderboard3.AppendInteger(0);
            weeklyLeaderboard3.AppendInteger(1);
            weeklyLeaderboard3.AppendString(Session.GetHabbo().UserName);
            weeklyLeaderboard3.AppendString(Session.GetHabbo().Look);
            weeklyLeaderboard3.AppendString(Session.GetHabbo().Gender);
            weeklyLeaderboard3.AppendInteger(0);
            weeklyLeaderboard3.AppendInteger(18);
            await Session.SendMessageAsync(weeklyLeaderboard3);



            ServerMessage previousWinner = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterPreviousWinnerMessageComposer"));
            previousWinner.AppendInteger(18);
            previousWinner.AppendInteger(0);

            previousWinner.AppendString("name");
            previousWinner.AppendString("figure");
            previousWinner.AppendString("gender");
            previousWinner.AppendInteger(0);
            previousWinner.AppendInteger(0);

            await Session.SendMessageAsync(previousWinner);

            /*ServerMessage Products = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterProductsMessageComposer"));
            Products.AppendInteger(18);//gameId
            Products.AppendInteger(0);//count
            Products.AppendInteger(6526);
            Products.AppendBool(false);
            await Session.SendMessageAsync(Products);#1#

            ServerMessage allAchievements = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterAllAchievementsMessageComposer"));
            allAchievements.AppendInteger(0);//count

            //For Stories
            /*PacketName5.AppendInteger(18);
            PacketName5.AppendInteger(1);
            PacketName5.AppendInteger(191);
            PacketName5.AppendString("StoryChallengeChampion");
            PacketName5.AppendInteger(20);#1#

            allAchievements.AppendInteger(0);//gameId
            allAchievements.AppendInteger(0);//count
            allAchievements.AppendInteger(0);//achId
            allAchievements.AppendString("SnowWarTotalScore");//achName
            allAchievements.AppendInteger(0);//levels

            await Session.SendMessageAsync(allAchievements);*/


        }

        internal void InitializeGameCenter()
        {
            return ;
        }
        internal async Task GetGameListing()
        {

            var game = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterGamesListMessageComposer"));
            await game.AppendIntegerAsync(1);
            await game.AppendIntegerAsync(1);
            await game.AppendStringAsync("basejump");
            await game.AppendStringAsync("93d4f3");
            await game.AppendStringAsync("");
            await game.AppendStringAsync(ExtraSettings.GameCenterBaseJumpUrl);
            await game.AppendStringAsync("");
            await Session.SendMessageAsync(game);
        }
        /// <summary>
        /// Games the center join queue.
        /// </summary>
        internal async Task GameCenterJoinQueue()
        {
            var gameId = Request.GetInteger();
            ServerMessage joinQueue = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterJoinGameQueueMessageComposer"));
            await joinQueue.AppendIntegerAsync(gameId);
            await Session.SendMessageAsync(joinQueue);

            var habboId = Session.GetHabbo().Id;
            string ssoTicket;
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id FROM user_auth_food WHERE user_id = '" + habboId + "' LIMIT 1");
                var data = dbClient.GetRow();
                //todo: delete when login
                if (data == null)
                {
                    ssoTicket = "Fastfood-" + GenerateSso(32) + "-" + Session.GetHabbo().Id;
                    dbClient.RunQuery("INSERT INTO user_auth_food(user_id, auth_ticket) VALUES ('" + habboId +
                                      "', '" +
                                      ssoTicket + "')");
                }
                else
                {
                    dbClient.SetQuery("SELECT auth_ticket FROM user_auth_food WHERE user_id = " + habboId);
                    ssoTicket = dbClient.GetRow()["auth_ticket"].ToString();
                }
            }


            ServerMessage loadGame = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLoadGameUrlMessageComposer"));
            await loadGame.AppendIntegerAsync(gameId);
            await loadGame.AppendStringAsync(Convert.ToString(Oblivion.GetUnixTimeStamp()));
            await loadGame.AppendStringAsync(ExtraSettings.GameCenterBaseJumpUrl + "BaseJump.swf");
            await loadGame.AppendStringAsync("best");
            await loadGame.AppendStringAsync("showAll");
            await loadGame.AppendIntegerAsync(60);
            await loadGame.AppendIntegerAsync(10);
            await loadGame.AppendIntegerAsync(8);
            await loadGame.AppendIntegerAsync(6);
            await loadGame.AppendStringAsync("assetUrl");
            await loadGame.AppendStringAsync(ExtraSettings.GameCenterBaseJumpUrl + "BasicAssets.swf");
            await loadGame.AppendStringAsync("habboHost");
            await loadGame.AppendStringAsync("http://fuseus-private-httpd-fe-1");
            await loadGame.AppendStringAsync("accessToken");
            await loadGame.AppendStringAsync(ssoTicket);
            await loadGame.AppendStringAsync("gameServerHost");
            await loadGame.AppendStringAsync(ExtraSettings.BaseJumpHost);
            await loadGame.AppendStringAsync("gameServerPort");
            await loadGame.AppendStringAsync(ExtraSettings.BaseJumpPort);
            await loadGame.AppendStringAsync("socketPolicyPort");
            await loadGame.AppendStringAsync(ExtraSettings.BaseJumpHost);

            await Session.SendMessageAsync(loadGame);
        }


        private static string GenerateSso(int length)
        {
            var random = new Random();
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                result.Append(characters[random.Next(characters.Length)]);
            return result.ToString();
        }
    }
}