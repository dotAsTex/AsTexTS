using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;

namespace DotaLeagueForm.Actions
{
    public class StartLobby
    {

        private SteamClient steamClient;
        private SteamGameCoordinator steamGC;
        private SteamFriends steamFriends;
        private SteamGameCoordinator steamGameCoordinator;
        private CallbackManager manager;
        private CMsgDOTAMatch dotaMatch;
        Dictionary<ulong, bool> matchList = new Dictionary<ulong, bool>();
        private int matchId;
        SteamUser steamUser;

        bool isRunning;
        private bool matchFinished;

        public const int APPID = 570;

        private CSODOTALobby Lobby;
        private ulong chatChannel = 0;
        private string LobbyPAssword;
        public string user, pass;

        public delegate void LobbyUpdate(CSODOTALobby lobby);
        public event LobbyUpdate Updated;

        public delegate void Log(string message);
        public event Log NewMessage;

        public delegate void SetPassword(string password);
        public event SetPassword Password;

        public delegate void SetPlayerStatus(string PlayerNick);
        public event SetPlayerStatus Ready;
        public event SetPlayerStatus NotReady;

        public delegate void Disconnected();
        public event Disconnected SteamCrash;

        public delegate void MatchFinished(CMsgDOTAMatch match);
        public event MatchFinished Finished;

        private int lastUserCount;



        public void Start()
        {
            LobbyPAssword = RandomPassword();
            // create our steamclient instance
            steamClient = new SteamClient();

            manager = new CallbackManager(steamClient);
            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();
            // get the steam friends handler, which is used for interacting with friends on the network after logging on
            steamFriends = steamClient.GetHandler<SteamFriends>();
            steamGameCoordinator = steamClient.GetHandler<SteamGameCoordinator>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
            new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, manager);

            // we use the following callbacks for friends related activities
            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);
            new Callback<SteamFriends.FriendsListCallback>(OnFriendsList, manager);
            new Callback<SteamFriends.PersonaStateCallback>(OnPersonaState, manager);
            new Callback<SteamFriends.FriendAddedCallback>(OnFriendAdded, manager);

            new Callback<SteamGameCoordinator.MessageCallback>(OnGCMessage, manager);

            isRunning = true;

            NewMessage("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();
            //new Thread(HeartBeet).Start();
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(0.5));
            }
        }

        void HeartBeet()
        {
            steamClient.Send(new ClientMsgProtobuf<CMsgClientHeartBeat>(EMsg.ClientHeartBeat));
            Thread.Sleep(5000);
        }
        // create our callback handling loop
        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                NewMessage(string.Format("Unable to connect to Steam: {0}", callback.Result));
                SteamCrash();
                isRunning = false;
                return;
            }

            NewMessage(string.Format("Connected to Steam! Logging in '{0}'...", user));

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
            });
        }

        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            NewMessage("Disconnected from Steam");
            SteamCrash();
            isRunning = false;
        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 6 for how SteamGuard can be handled

                    NewMessage("Unable to logon to Steam: This account is SteamGuard protected.");
                    SteamCrash();
                    isRunning = false;
                    return;
                }

                NewMessage(string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));
                SteamCrash();
                isRunning = false;
                return;
            }

            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(APPID),
            });

            steamClient.Send(playGame);
            Thread.Sleep(5000);

            NewMessage("Successfully logged on!");

            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            steamGameCoordinator.Send(clientHello, APPID);
        }

        void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            // at this point, the client has received it's friends list

            int friendCount = steamFriends.GetFriendCount();

            NewMessage(string.Format("We have {0} friends", friendCount));

            for (int x = 0; x < friendCount; x++)
            {
                // steamids identify objects that exist on the steam network, such as friends, as an example
                SteamID steamIdFriend = steamFriends.GetFriendByIndex(x);

                // we'll just display the STEAM_ rendered version
                //    NewMessage("Friend: {0}", steamIdFriend.Render());
            }

            // we can also iterate over our friendslist to accept or decline any pending invites

            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    // this user has added us, let's add him back
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            // someone accepted our friend request, or we accepted one
            NewMessage(string.Format("{0} is now a friend", callback.PersonaName));
        }

        void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            // this callback is received when the persona state (friend information) of a friend changes

            // for this sample we'll simply display the names of the friends
            // NewMessage( "State change: {0}", callback.Name );
        }

        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            NewMessage(string.Format("Logged off of Steam: {0}", callback.Result));
        }

        void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
        {
            // setup our dispatch table for messages
            // this makes the code cleaner and easier to maintain
            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { ( uint )EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { ( uint) ESOMsg.k_ESOMsg_CacheSubscribed, OnPracticeLobbyCreate},
                { ( uint)EDOTAGCMsg.k_EMsgGCJoinChatChannelResponse, OnJoinChatChannelResponse},
                { ( uint)EDOTAGCMsg.k_EMsgDOTAChatChannelMemberUpdate, OnMemberUpdate},
                {( uint) ESOMsg.k_ESOMsg_CacheUnsubscribed, OnPracticeLobbyLeave},
                {( uint) ESOMsg.k_ESOMsg_UpdateMultiple, OnUpdateMultiple},
                {( uint) EDOTAGCMsg.k_EMsgGCChatMessage, OnChatMessage},
                { ( uint )EDOTAGCMsg.k_EMsgGCMatchDetailsResponse, OnMatchDetails },
                 {(uint)EDOTAGCMsg.k_EMsgGCSourceTVGamesResponse, OnSourceTVGameUpdate }
            };

            Action<IPacketGCMsg> func;
            if (!messageMap.TryGetValue(callback.EMsg, out func))
            {
                if (callback.EMsg != 7402)
                {
                    NewMessage("Missed packet: " + callback.EMsg);

                } // this will happen when we recieve some GC messages that we're not handling
                // this is okay because we're handling every essential message, and the rest can be ignored
                return;
            }

            func(callback.Message);
        }

        private void OnSourceTVGameUpdate(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgSourceTVGamesResponse>(obj);
            NewMessage(msg.Body.games[0].tower_state.ToString());
        }

        private void OnMatchDetails(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgGCMatchDetailsResponse>(obj);

            EResult result = (EResult)msg.Body.result;
            
            if (result != EResult.OK)
            {
                Console.WriteLine("Unable to request match details: {0}", result);
            }
            else
            {
                matchList[msg.Body.match.match_id] = true;
                dotaMatch = msg.Body.match;
                Finished(dotaMatch);
            }
        }

        private void OnUpdateMultiple(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgSOMultipleObjects>(obj);
            foreach (var modifiedObject in msg.Body.objects_modified)
            {
                if (modifiedObject.type_id == 2004)
                    HandleLobby(modifiedObject.object_data);
            }
        }

        private void OnPracticeLobbyLeave(IPacketGCMsg obj)
        {
            //NewMessage("Leaving and try again");
            var sub = new ClientGCMsgProtobuf<CMsgSOCacheUnsubscribed>(obj);
            if (sub.Body.owner_soid.type == 3)
            {
                //   NewMessage("Lobby leaved " + sub.Body.owner_soid.id);
            }
            else
            {
                var leave = new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
                steamGameCoordinator.Send(leave, APPID);
            }
            CreateLobby();
        }

        private void OnPracticeLobbyCreate(IPacketGCMsg obj)
        {
            var sub = new ClientGCMsgProtobuf<CMsgSOCacheSubscribed>(obj);
            if (sub.Body.owner_soid.type == 3)
            {
                //NewMessage("Lobby created " + sub.Body.owner_soid.id);
                var joinChat =
                    new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannel>((uint)EDOTAGCMsg.k_EMsgGCJoinChatChannel);
                joinChat.Body.channel_name = "Lobby_" + sub.Body.owner_soid.id;
                foreach (var scheme in sub.Body.objects)
                {
                    if (scheme.type_id == 2004)
                    {

                        HandleLobby(scheme.object_data[0]);
                    }
                }
                joinChat.Body.channel_type = DOTAChatChannelType_t.DOTAChannelType_Lobby;
                steamGameCoordinator.Send(joinChat, APPID);

            }
            else
            {
                var leave = new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
                steamGameCoordinator.Send(leave, APPID);
            }
        }
        void OnClientWelcome(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);
            NewMessage(string.Format("GC is welcoming us. Version: {0}", msg.Body.version));
            NewMessage("Requesting lobby Create");

            CreateLobby();

        }
        private void OnJoinChatChannelResponse(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannelResponse>(obj);
            //NewMessage("Joined Lobby Chat");
            chatChannel = msg.Body.channel_id;
            NewMessage("Chat channel = " + chatChannel);
            foreach (var member in msg.Body.members)
            {

                NewMessage("Chat member: " + member.persona_name);
            }
            Password(string.Format("Lobby Password: {0}", LobbyPAssword));
            GoToBroadcasters();
        }

        private void OnMemberUpdate(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgDOTAChatChannelMemberUpdate>(obj);
            foreach (var member in msg.Body.joined_members)
            {
                NewMessage("Member joined chat: " + member.persona_name);
            }
        }

        private void HandleLobby(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                Lobby = Serializer.Deserialize<CSODOTALobby>(stream);
            }
            NewMessage("Lobby update");
            if (Lobby.members.Count != lastUserCount)
            {
                lastUserCount = Lobby.members.Count;
                SendMessage();
            }
            Updated(Lobby);

        }

        private void GoToBroadcasters()
        {
            var change =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinBroadcastChannel>(
                    (uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinBroadcastChannel);
            change.Body.channel = 0;
            steamGameCoordinator.Send(change, APPID);
            NewMessage("//Now we are in broadcasters\\");
        }

        private void CreateLobby()
        {
            var create = new ClientGCMsgProtobuf<CMsgPracticeLobbyCreate>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyCreate);
            LobbyPAssword = RandomPassword();
            create.Body.pass_key = LobbyPAssword;
            
            create.Body.lobby_details = new CMsgPracticeLobbySetDetails { pass_key = LobbyPAssword, game_mode = (uint)DOTA_GameMode.DOTA_GAMEMODE_1V1MID};
            steamGameCoordinator.Send(create, APPID);
        }

        private string RandomPassword()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, 5)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());

        }

        private void OnChatMessage(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>(obj);

            if (msg.Body.text == "rdy" && msg.Body.channel_id == chatChannel)
                Ready(msg.Body.persona_name);
            if (msg.Body.text == "n" && msg.Body.channel_id == chatChannel)
                NotReady(msg.Body.persona_name);

        }

        private void SendMessage()
        {
            var create = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>((uint)EDOTAGCMsg.k_EMsgGCChatMessage);
            create.Body.channel_id = chatChannel;
            create.Body.text = "Если вы готовы к игре - отправьте в чат rdy, если нет- n. Игра начнется как только все 10 игроков будут готовы";
            steamGameCoordinator.Send(create, APPID);
        }

        public void Disconnect()
        {
            steamClient.Disconnect();
            isRunning = false;
        }

        public void LaunchLobby()
        {
            var start =
               new ClientGCMsgProtobuf<CMsgPracticeLobbyLaunch>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyLaunch);
            steamGameCoordinator.Send(start, APPID);
        }


        public void AbandonGame()
        {
            var abandon = new ClientGCMsgProtobuf<CMsgAbandonCurrentGame>((uint)EDOTAGCMsg.k_EMsgGCAbandonCurrentGame);
            matchList.Add(Lobby.match_id,false);
            steamGameCoordinator.Send(abandon, 570);
            
        }

        public void LeaveLobby()
        {
			var leaveLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave> ((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
			
            this.Lobby = null;
			steamGameCoordinator.Send(leaveLobby, 570);

        }

        public void ReceiveMatchData()
        {
            while (true)
            {
                try
                {
                    foreach (var match in matchList)
                    {
                        if (match.Value != false) continue;
                        var requestMatch =
                            new ClientGCMsgProtobuf<CMsgGCMatchDetailsRequest>(
                                (uint) EDOTAGCMsg.k_EMsgGCMatchDetailsRequest);

                        requestMatch.Body.match_id = match.Key;
                        var ab =
                            new ClientGCMsgProtobuf<CMsgFindSourceTVGames>(
                                (uint)EDOTAGCMsg.k_EMsgGCFindSourceTVGames);
                        ab.Body.custom_game_id = match.Key;
                        ab.Body.num_games = 1;
                        
                        steamGameCoordinator.Send(ab,APPID);
                        steamGameCoordinator.Send(requestMatch, APPID);
                        var abc =
                            new ClientGCMsgProtobuf<CMsgGameChatLog>(
                                (uint)EDOTAGCMsg.k_EMsgGCGameChatLog);
                        abc.Body.match_id = match.Key;
                        Thread.Sleep(5000);
                    }
                }
                catch { }

                Thread.Sleep(5000);
                
            }
        }

        private void RequestMatchDataAgain(uint matchId)
        {
            var requestMatch = new ClientGCMsgProtobuf<CMsgGCMatchDetailsRequest>((uint)EDOTAGCMsg.k_EMsgGCMatchDetailsRequest);
            requestMatch.Body.match_id = matchId;
            steamGameCoordinator.Send(requestMatch, APPID);
        }
        


    }
}