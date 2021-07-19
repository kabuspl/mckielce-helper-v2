using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace mckielce_helper {
    /// <summary>
    /// Logika interakcji dla klasy SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl {
        public SettingsPage() {
            InitializeComponent();
            if ((bool)SettingsHandler.Get("auto_update_install")) {
                autoinstall.IsChecked = true;
            } else {
                askforinstall.IsChecked = true;
            }
            emergStop.SelectedValue = SettingsHandler.Get("emergStop");
            start.SelectedValue = SettingsHandler.Get("start");
            mode.SelectedValue = SettingsHandler.Get("mode");
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {

        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            SettingsHandler.Set("auto_update_install", (bool)autoinstall.IsChecked);
            SettingsHandler.Set("emergStop", emergStop.SelectedValue.ToString());
            SettingsHandler.Set("start", start.SelectedValue.ToString());
            SettingsHandler.Set("mode", mode.SelectedValue.ToString());
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}
