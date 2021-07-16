using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace mckielce_helper {
    /// <summary>
    /// Logika interakcji dla klasy Overlay.xaml
    /// </summary>
    public partial class Overlay : Window {

        string line1 = "";
        string line2 = "";
        string line3 = "";

        public Overlay() {
            InitializeComponent();
            Stopwatch sw = Stopwatch.StartNew();
            long start = 0;
            long end = sw.ElapsedMilliseconds;
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += delegate {
                time += end-start;
                start = end;
                end = sw.ElapsedMilliseconds;
                long lefttime = totaltime - time;
                long min = lefttime / 60000;
                long sec = (lefttime - (min * 60000)) / 1000;
                line2 = "Pozostało " + min + "m " + sec + "s";
                text.Content = line1 + Environment.NewLine + line2 + Environment.NewLine + line3;
            };
            timer.Start();
            Point location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - 100, 0);
            this.Top = location.Y;
            this.Left = location.X;
        }

        int totaltime = 3000;
        long time;

        public void ModifyInfo(int i, int count, float x, float z) {
            totaltime = count * 900 + 6000 + 3000 + 200 + 600;
            float progress2 = ((float)i / (float)count) * 100;
            progress.Value = (int)progress2;
            line1 = "Wykonano " + i.ToString() + " / " + count.ToString();
            line3 = "Pozycja " + x.ToString().Replace(",", ".") + " / " + z.ToString().Replace(",", ".");
        }
    }
}
