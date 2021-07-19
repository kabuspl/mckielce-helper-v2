using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using Fleck;

namespace mckielce_helper {
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        MainPage mainpage;
        SettingsPage settingspage;

        bool firstcheck = true;
        bool updFromBtn = false;

        public MainWindow() {
            InitializeComponent();
            SettingsHandler.InitSettings();
            mainpage = new MainPage();
            settingspage = new SettingsPage();
            Content.Content = mainpage;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.Start("https://raw.githubusercontent.com/kabuspl/mckielce-helper-v2/main/UpdateInfo.xml");

            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            timer.Tick += delegate {
                updFromBtn = false;
                firstcheck = false;
                AutoUpdater.Start("https://raw.githubusercontent.com/kabuspl/mckielce-helper-v2/main/UpdateInfo.xml");
            };
            timer.Start();

            WebSocketServer wsserver = new WebSocketServer("ws://0.0.0.0:2138");

            wsserver.Start(socket => {
                socket.OnOpen = () => { };
                socket.OnClose = () => { };
                socket.OnMessage = message => {
                    this.Dispatcher.Invoke(() => {
                        mainpage.setCode(message);
                        this.Activate();
                    });
                };
            });

            this.Title = this.Title + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args) {
            if (args.Error == null) {
                if (args.IsUpdateAvailable) {
                    bool install = false;
                    if ((bool)SettingsHandler.Get("auto_update_install") && firstcheck && !updFromBtn) {
                        install = true;
                    } else if (!firstcheck && !updFromBtn) {
                        updatebtn.Source = new BitmapImage(new Uri("Resources/down-arrow-a.png", UriKind.Relative));
                        updatebtn2.ToolTip = "Zaktualizuj";
                    } else {
                        DialogResult result = System.Windows.Forms.MessageBox.Show("Nowa wersja jest dostępna. Czy chcesz zainstalować?", "Aktualizacja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        install = result == System.Windows.Forms.DialogResult.Yes;
                    }

                    if (install && (firstcheck || updFromBtn)) {
                        try {
                            if (AutoUpdater.DownloadUpdate(args)) {
                                System.Windows.Application.Current.Shutdown();
                            } else {
                                System.Windows.Forms.MessageBox.Show("zjebalo sie");
                            }
                        } catch (Exception) {
                            System.Windows.Forms.MessageBox.Show("Zjebało się");
                        }
                    }
                } else if (updFromBtn) {
                    System.Windows.Forms.MessageBox.Show("Posiadasz aktualną wersję 😀");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            Content.Content = mainpage;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            Content.Content = settingspage;
        }

        private void updatebtn2_Click(object sender, RoutedEventArgs e) {
            updFromBtn = true;
            AutoUpdater.Start("https://raw.githubusercontent.com/kabuspl/mckielce-helper-v2/main/UpdateInfo.xml");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {

        }
    }
}
