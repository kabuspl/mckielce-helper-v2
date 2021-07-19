using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;
using NHotkey.Wpf;
using Fleck;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace mckielce_helper {
    /// <summary>
    /// Logika interakcji dla klasy MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl {

        public bool loaded = false;

        string emergK;
        string startK;
        string modeK;

        List<string> blocks = new List<string>() {
            "grass_block",
            "gray_concrete",
            "gravel",
            "cobblestone",
            "dirt_path",
            "air",
            "bricks",
            "stone_bricks",
            "smooth_stone",
            "iron_bars",
            "bedrock"
        };

        public MainPage() {
            InitializeComponent();
            
            fromBlock.ItemsSource = blocks;
            toBlock.ItemsSource = blocks;
            fromBlock.SelectedItem = "grass_block";
            toBlock.SelectedItem = "gray_concrete";
            emergK = (string)SettingsHandler.Get("emergStop");
            startK = (string)SettingsHandler.Get("start");
            modeK = (string)SettingsHandler.Get("mode");
            Key key = new Key();
            Enum.TryParse<Key>(emergK, out key);
            HotkeyManager.Current.AddOrReplace("emergency", key, ModifierKeys.None, EmergHandler);
            Key key2 = new Key();
            Enum.TryParse<Key>(startK, out key2);
            HotkeyManager.Current.AddOrReplace("start", key2, ModifierKeys.None, StartHandler);
            Key key3 = new Key();
            Enum.TryParse<Key>(modeK, out key3);
            HotkeyManager.Current.AddOrReplace("mode", key3, ModifierKeys.None, ModeHandler);
            loaded = true;
        }

        public void setCode(string text) {
            code.Text = text;
        }

        private void EmergHandler(object sender, NHotkey.HotkeyEventArgs e) {
            failSafe = true;
            NotificationHandler.Send(emergK, "Zatrzymano awaryjnie");
        }

        private void StartHandler(object sender, NHotkey.HotkeyEventArgs e) {
            if (startbtn.IsEnabled == false) return;
            NotificationHandler.Send(startK, "Uruchomiono akcję");
            startbtn.IsEnabled = false;
            runAction();
        }

        private void ModeHandler(object sender, NHotkey.HotkeyEventArgs e) {
            if (modes.SelectedIndex + 1 > modes.Items.Count - 1) {
                modes.SelectedIndex = 0;
            } else {
                modes.SelectedIndex++;
            }
            NotificationHandler.Send(modeK, "Ustawiono tryb:" + Environment.NewLine + modes.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            renderPreview();
        }
        BitmapImage BitmapToImageSource(Bitmap bitmap) {
            using (MemoryStream memory = new MemoryStream()) {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        string errors = "";

        private void renderPreview() {
            if (code.Text == "") {
                code.ClearValue(TextBox.BorderBrushProperty);
                code.ClearValue(TextBox.BackgroundProperty);
                return;
            }
            try {
                code.ClearValue(TextBox.BorderBrushProperty);
                code.ClearValue(TextBox.BackgroundProperty);
                Bitmap bitmap;
                byte[] binary = Convert.FromBase64String(code.Text);
                string json = ASCIIEncoding.ASCII.GetString(binary);
                McKielceData data = JsonConvert.DeserializeObject<McKielceData>(json);
                if (data == null) throw new Exception("e1");
                McKielceCoords[] coords = data.coords;
                McKielceCoords mincoords = new McKielceCoords(Int32.MaxValue, Int32.MaxValue);
                McKielceCoords maxcoords = new McKielceCoords(Int32.MinValue, Int32.MinValue);
                foreach (McKielceCoords coord in coords) {
                    if (coord.x < mincoords.x) mincoords.x = coord.x;
                    if (coord.y < mincoords.y) mincoords.y = coord.y;
                    if (coord.x > maxcoords.x) maxcoords.x = coord.x;
                    if (coord.y > maxcoords.y) maxcoords.y = coord.y;
                }
                float maxx = 0;
                float maxy = 0;
                float wx = Math.Abs(mincoords.x - maxcoords.x);
                float wy = Math.Abs(mincoords.y - maxcoords.y);
                foreach (McKielceCoords coord in coords) {
                    float wspx = 1;
                    float wspy = wy / wx;
                    if (wx > wy) {
                        wspy = 1;
                        wspx = wx / wy;
                    }
                    float x1 = Math.Abs(mincoords.x - coord.x) / wx * 256 * wspx;
                    float y1 = Math.Abs(mincoords.y - coord.y) / wy * 256 * wspy;
                    if (x1 > maxx) maxx = x1;
                    if (y1 > maxy) maxy = y1;
                }
                bitmap = new Bitmap((int)maxx, (int)maxy);
                Graphics img = Graphics.FromImage(bitmap);
                int i = 0;
                McKielceCoords old = new McKielceCoords(0, 0);

                foreach (McKielceCoords coord in coords) {
                    if (i > 0) {
                        float x1 = Math.Abs(mincoords.x - old.x) / wx * maxx;
                        float y1 = Math.Abs(mincoords.y - old.y) / wy * maxy;
                        float x2 = Math.Abs(mincoords.x - coord.x) / wx * maxx;
                        float y2 = Math.Abs(mincoords.y - coord.y) / wy * maxy;
                        //Console.WriteLine(x1+";"+y1+"/"+x2+";"+y2);
                        img.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Red, 5), x1, y1, x2, y2);
                    }
                    old = coord;
                    i++;
                }
                float bx1 = Math.Abs(mincoords.x - old.x) / wx * maxx;
                float by1 = Math.Abs(mincoords.y - old.y) / wy * maxy;
                float bx2 = Math.Abs(mincoords.x - coords[0].x) / wx * maxx;
                float by2 = Math.Abs(mincoords.y - coords[0].y) / wy * maxy;
                img.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Green, 5), bx1, by1, bx2, by2);
                //bitmap.Save("preview.png");
                canvas.Source = BitmapToImageSource(bitmap);
            } catch (Exception ex) {
                code.BorderBrush = System.Windows.Media.Brushes.Red;
                code.Background = System.Windows.Media.Brushes.LightPink;
                errors += Environment.NewLine + "================" + Environment.NewLine + ex.Message + ";;;" + ex.Source + ";;;" + ex.StackTrace;
                return;
            }
        }

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e) {
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e) {
            //OpenWithDefaultProgram("preview.png");
        }

        public static void OpenWithDefaultProgram(string path) {
            Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            code.Text = Clipboard.GetText();
        }

        Overlay overlay;

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            startbtn.IsEnabled = false;
            runAction();
        }

        bool failSafe = false;

        bool debug = false;

        private async void runAction() {
            try {
                int mode = modes.SelectedIndex;
                int sp = (int)speed.Value;
                overlay = new Overlay();
                overlay.Show();
                failSafe = false;
                await Task.Delay(3000);
                byte[] binary = Convert.FromBase64String(code.Text);
                string json = ASCIIEncoding.ASCII.GetString(binary);
                McKielceData data = JsonConvert.DeserializeObject<McKielceData>(json);
                if (data == null) throw new Exception();
                McKielceCoords[] coords = data.coords;
                InputSimulator sim = new InputSimulator();
                if (mode != 2) {
                    await typeOnChat("//sel poly", sim);
                } else {
                    await typeOnChat("//sel cuboid", sim);
                }
                int i = 1, i2 = 1;
                foreach (McKielceCoords coord in coords) {
                    if (failSafe) break;
                    float progress = ((float)i / (float)coords.Length) * 100;
                    progressb.Value = progress;
                    progressl.Content = i.ToString() + " / " + coords.Length.ToString();
                    overlay.ModifyInfo(i, coords.Length, coord.x, coord.y);
                    if (height.Value != 40) {
                        await typeOnChat("/setblock " + Math.Floor(coord.x).ToString() + " " + height.Value + " " + Math.Floor(coord.y).ToString() + " minecraft:glass", sim);
                    }
                    if (failSafe) break;
                    await typeOnChat("/tp " + coord.x.ToString().Replace(",", ".") + " " + (height.Value + 1) + " " + coord.y.ToString().Replace(",", "."), sim);
                    if (i == 1) await Task.Delay(12000 / sp);
                    await Task.Delay(1000 / sp);
                    if (failSafe) break;
                    if (i2 == 1) {
                        if (!debug) sim.Mouse.LeftButtonClick();
                    } else {
                        if (!debug) sim.Mouse.RightButtonClick();
                    }
                    await Task.Delay(600 / sp);
                    if (mode == 2 && i > 1) {
                        await typeOnChat("//line " + toBlock.Text, sim);
                    }
                    i++;
                    i2++;
                    if (i2 > 2 && mode == 2) i2 = 1;
                }
                if (failSafe) {
                    progressl.Content = "Przerwanie awaryjne!";
                    overlay.Close();
                    startbtn.IsEnabled = true;
                    return;
                }
                progressl.Content = "Wykonywanie akcji...";
                if (mode != 2) {
                    switch (mode) {
                        case 0:
                            await typeOnChat("//replace " + fromBlock.Text + " " + toBlock.Text, sim);
                            break;
                        case 1:
                            await typeOnChat("//set " + toBlock.Text, sim);
                            break;
                        case 3:
                            await typeOnChat("//forest " + toBlock.Text + " " + fromBlock.Text,sim);
                            break;
                    }
                }
                progressl.Content = "Czekam na akcję...";
                progressb.Value = 0;
                overlay.Close();
                startbtn.IsEnabled = true;
            } catch (Exception) {
                progressl.Content = "Czekam na akcję...";
                progressb.Value = 0;
                overlay.Close();
                startbtn.IsEnabled = true;
                return;
            }
        }

        private async Task typeOnChat(string msg, InputSimulator sim) {
            int sp = (int)speed.Value;
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_T);
            await Task.Delay(200 / sp);
            sim.Keyboard.TextEntry(msg);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(200 / sp);
        }

        string oldSelect = "";
        string oldSelect2 = "";

        private void modes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!loaded) return;
            fromBlock.ItemsSource = blocks;
            toBlock.ItemsSource = blocks;
            fromBlockBox.Header = "Pierwotny blok";
            toBlockBox.Header = "Docelowy blok";
            fromBlock.SelectedItem = oldSelect;
            toBlock.SelectedItem = oldSelect2;
            if (modes.SelectedIndex == 0) {
                fromBlock.IsEnabled = true;
            }else if (modes.SelectedIndex == 3) {
                oldSelect = fromBlock.Text;
                oldSelect2 = toBlock.Text;
                fromBlock.IsEnabled = true;
                fromBlockBox.Header = "Gęstość lasu";
                toBlockBox.Header = "Rodzaj drzewa";
                fromBlock.ItemsSource = new List<string>() {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5"
                };
                toBlock.ItemsSource = new List<string>() {
                    "largeoak",
                    "oak",
                    "spruce"
                };
                fromBlock.SelectedItem = "4";
                toBlock.SelectedItem = "largeoak";
            } else {
                fromBlock.IsEnabled = false;
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {
            Clipboard.SetText(errors);
            MessageBox.Show("======== SKOPIOWANO: ========" + errors);
        }

        private void speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            speed.ClearValue(Slider.EffectProperty);
            if (speed.Value == 4) {
                speed.Effect = new DropShadowEffect {
                    Color = new System.Windows.Media.Color { A = 255, R = 255, G = 0, B = 0 },
                    Direction = 0,
                    ShadowDepth = 0,
                    Opacity = 1,
                    BlurRadius = 15
                };
            }
        }
    }

    public class McKielceCoords {
        public float x;
        public float y;

        public McKielceCoords(float x, float y) {
            this.x = x;
            this.y = y;
        }
    }

    public class McKielceData {
        public int version;
        public McKielceCoords[] coords;
    }

    public static class Constants {
        //windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }


}
