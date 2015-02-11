using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DotaLeagueForm.Actions;
using DotaLeagueForm.Entities;
using SteamKit2;
using SteamKit2.GC.Dota.Internal;

namespace DotaLeagueForm
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Player> players = new List<Player>();
        private List<CMsgDOTAMatch> matches = new List<CMsgDOTAMatch>();
        List<Data> stats = new List<Data>();
        private StartLobby startLobby;
        private bool launch;
        private bool ThreadLaunched;
        private readonly string ApiKey = "35BCA72E5BF2B6A2E27CF28285106822";
        private Thread mainThread;
        public MainWindow()
        {
            InitializeComponent();
            startLobby = new StartLobby();
            startLobby.NewMessage += startLobby_NewMessage;
            startLobby.Updated += startLobby_Updated;
            startLobby.Password += startLobby_Password;
            startLobby.Ready += startLobby_Ready;
            startLobby.NotReady += startLobby_NotReady;
            startLobby.SteamCrash += startLobby_SteamCrash;
            startLobby.Finished += startLobby_Finished;
            startLobby.user = "skraskor";
            startLobby.pass = "760326AsTex";
            mainThread = new Thread(startLobby.Start);
            mainThread.Start();
        }

        void startLobby_Finished(CMsgDOTAMatch match)
        {
            matches.Add(match);
            Dispatcher.BeginInvoke(new Action(delegate
            {
                MatchesBox.Items.Add(match.match_id);
            }));
        }

        void startLobby_SteamCrash()
        {
            StartLobby startLobby = new StartLobby();
            startLobby.NewMessage += startLobby_NewMessage;
            startLobby.Updated += startLobby_Updated;
            startLobby.Password += startLobby_Password;
            startLobby.Ready += startLobby_Ready;
            startLobby.NotReady += startLobby_NotReady;
            startLobby.SteamCrash += startLobby_SteamCrash;
            startLobby.Finished += startLobby_Finished;
            startLobby.user = "skraskor";
            startLobby.pass = "760326AsTex";
            startLobby.Start();
        }

        void startLobby_NotReady(string playerNick)
        {
            players.First(x => x.Nickname == playerNick).Ready = false;
            UpdateLobby();
        }

        void startLobby_Ready(string playerNick)
        {
            players.First(x => x.Nickname == playerNick).Ready = true;
            UpdateLobby();
        }

        void startLobby_Password(string password)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                LobbyPasswordBox.Text = password;
            }));
        }

        void startLobby_Updated(CSODOTALobby lobby)
        {
            players.Clear();
            UpdateLogos(lobby);
            foreach (var member in lobby.members)
            {
                if (member.team == DOTA_GC_TEAM.DOTA_GC_TEAM_BAD_GUYS)
                {
                    players.Add(new Player { Nickname = member.name, Ready = false, Side = Side.Dire, Slot = (int)member.slot, UserId = member.id });
                }
                if (member.team == DOTA_GC_TEAM.DOTA_GC_TEAM_GOOD_GUYS)
                {
                    players.Add(new Player { Nickname = member.name, Ready = false, Side = Side.Radiant, Slot = (int)member.slot, UserId = member.id});
                }
            }
            UpdateLobby();
        }

        private void UpdateLogos(CSODOTALobby lobby)
        {
            if (lobby.match_id != 0)
            {
                startLobby_NewMessage("Match id: " + lobby.match_id);
                if (launch && !ThreadLaunched)
                {
                    ThreadLaunched = true;
                    startLobby.AbandonGame();
                    new Thread(startLobby.ReceiveMatchData).Start();

                }
            }
            if (lobby.team_details.Count > 0)
            {
                if (lobby.team_details[0].team_name != "")
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        LeftTeam.Content = lobby.team_details[0].team_tag;
                    }));
                    // var dict = new Dictionary<string, string>
                    //     {
                    //         {"ugcid", lobby.team_details[0].team_logo.ToString()},
                    //         {"appid", "570"}
                    //     };
                    // var kvp = WebAPI.GetInterface("ISteamRemoteStorage", ApiKey).Call("GetUGCFileDetails", args: dict);
                    // string imageLink = kvp["url"].Value;
                    // var bi = GetImage(imageLink);
                    // Dispatcher.BeginInvoke(new Action(delegate
                    // {
                    //     leftTeamLogo.Source = bi;
                    // }));
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        if ((string)LeftTeam.Content != "Radiant")
                        {
                            LeftTeam.Content = "Radiant";
                            // leftTeamLogo.Source = new BitmapImage(Resource1.Radiant_icon.);
                        }
                    }));
                }


                if (lobby.team_details[1].team_name != "")
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        RightTeam.Content = lobby.team_details[1].team_tag;
                    }));
                    // var dict = new Dictionary<string, string>
                    //     {
                    //         {"ugcid", lobby.team_details[1].team_logo.ToString()},
                    //         {"appid", "570"}
                    //     };
                    // var kvp = WebAPI.GetInterface("ISteamRemoteStorage", ApiKey)
                    //     .Call("GetUGCFileDetails", args: dict);
                    // string imageLink = kvp["url"].Value;
                    // var bi = GetImage(imageLink);
                    // Dispatcher.BeginInvoke(new Action(delegate
                    // {
                    //     rightTeamLogo.Source = GetImage(imageLink);
                    // }));

                    //  Console.WriteLine(imageLink);
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                   {
                       if ((string)RightTeam.Content != "Dire")
                       {
                           RightTeam.Content = "Dire";
                           //        rightTeamLogo.Source = new ImageSourceConverter().ConvertFromString("pack://siteoforigin:,,,/Resources/Dire_icon.png") as ImageSource;
                       }
                   }));
                }


            }

        }

        void startLobby_NewMessage(string message)
        {
            Dispatcher.BeginInvoke(new Action(() => LogBox.Items.Add("[" + DateTime.Now.ToShortTimeString() +
                                                                     "] " + message)));
        }

        BitmapImage GetImage(string link)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(link);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }
        public void UpdateLobby()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RadiantList.Items.Clear();
                DireList.Items.Clear();
                foreach (var player in players.OrderBy(x => x.Slot))
                {
                    if (player.Side == Side.Radiant)
                    {

                        RadiantList.Items.Add(player.Nickname + " - " + PlayerReady(player.Ready));
                    }
                    else
                    {
                        DireList.Items.Add(player.Nickname + " - " + PlayerReady(player.Ready));
                    }
                }
            }));
        }

        private static string PlayerReady(bool ready)
        {
            return ready ? "Готов" : "Не готов";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (startLobby != null)
                startLobby.Disconnect();
            try
            {
                mainThread.Abort();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void lobbyStartBtn_Click(object sender, RoutedEventArgs e)
        {
            startLobby.LaunchLobby();
            launch = true;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var match = matches.First(x => x.match_id == UInt64.Parse(MatchesBox.Items[MatchesBox.SelectedIndex].ToString()));
            ParsePlayers(match);
            MatchIdLabel.Content = "Match ID: " + match.match_id;
            TeamWinLabel.Foreground = match.good_guys_win ? Brushes.Green : Brushes.Red;
            TeamWinLabel.Content = match.good_guys_win ? "Radiant Team Win" : "Dire Team Win";
            MatchDurationLabel.Content = string.Format("Match duration: {0}:{1}", ((int)(match.duration / 60)).ToString("00"),
                ((int)(match.duration % 60)).ToString("00"));
            FirsTBloodLabel.Content = string.Format("First blood time {0}:{1}", (((int)match.first_blood_time / 60)).ToString("00"),
                ((int)(match.first_blood_time % 60)).ToString("00"));
        }

        private void ParsePlayers(CMsgDOTAMatch match)
        {
            
            stats.Clear();
            
            foreach (var player in match.players)
            {
                string binary = Convert.ToString(player.player_slot, 2);
                stats.Add(binary[0] == '0'
                    ? new Data
                    {
                        Team = "Radiant",
                        Hero = HeroesDB.GetHeroById(player.hero_id),
                        A = player.assists,
                        D = player.deaths,
                        XPM = player.XP_per_min,
                        GPM = player.gold_per_min,
                        K = player.kills,
                        Nick = player.player_name,
                        HD = player.hero_damage,
                        TD = player.tower_damage,
                        FeederOrLeaver = player.feeding_detected || player.leaver_status == (uint)DOTALeaverStatus_t.DOTA_LEAVER_ABANDONED || player.leaver_status == (uint)DOTALeaverStatus_t.DOTA_LEAVER_AFK || player.leaver_status == (uint)DOTALeaverStatus_t.DOTA_LEAVER_DISCONNECTED_TOO_LONG,
                        ID = player.account_id
                    }
                    : new Data
                    {
                        Team = "Dire",
                        Hero = HeroesDB.GetHeroById(player.hero_id),
                        A = player.assists,
                        D = player.deaths,
                        XPM = player.XP_per_min,
                        GPM = player.gold_per_min,
                        K = player.kills,
                        Nick = player.player_name,
                        HD = player.hero_damage,
                        TD = player.tower_damage,
                        FeederOrLeaver = player.feeding_detected || player.leaver_status == (uint)DOTALeaverStatus_t.DOTA_LEAVER_ABANDONED || player.leaver_status == (uint)DOTALeaverStatus_t.DOTA_LEAVER_AFK || player.leaver_status == (uint)DOTALeaverStatus_t.DOTA_LEAVER_DISCONNECTED_TOO_LONG,
                        ID = player.account_id
                    });
            }
            stats = stats.OrderBy(x => x.Team).ToList();
            PlayersStatsGrid.ItemsSource = stats;
            foreach (Data item in PlayersStatsGrid.ItemsSource)
            {
                var row = PlayersStatsGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    if (item.Team == "Dire")
                    {
                        row.Background = Brushes.LightCoral;

                    }
                    else
                    {
                        row.Background = Brushes.DarkSeaGreen;
                    }
                }
            }
            LeagueApi.SendMatchResult(new MatchResult(){MatchID = match.match_id, players = stats.ToList(),TeamWin = match.good_guys_win ? 0 : 1 });
            PlayersStatsGrid.UpdateLayout();
        }

        

    }

    public class Data
    {
        public string Team { get; set; }
        public string Hero { get; set; }
        public string Nick { get; set; }
        public uint K { get; set; }
        public uint D { get; set; }
        public uint A { get; set; }
        public uint GPM { get; set; }
        public uint XPM { get; set; }
        public uint TD { get; set; }
        public uint HD { get; set; }
        public bool FeederOrLeaver { get; set; }

        public uint ID { get; set; }


        
    }

}



