using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace mckielce_helper {
    /// <summary>
    /// Logika interakcji dla klasy NotificationOverlay.xaml
    /// </summary>
    public partial class NotificationOverlay : Window {

        public bool instantClose = false;

        public NotificationOverlay(string lefts, string rights) {
            InitializeComponent();
            this.Top = 20;
            this.Left = 20;
            left.Content = lefts;
            right.Text = rights;
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += delegate {
                timer.Stop();
                this.Close();
            };
            timer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            Closing -= Window_Closing;
            if (instantClose) {
                return;
            }
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(1));
            anim.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
