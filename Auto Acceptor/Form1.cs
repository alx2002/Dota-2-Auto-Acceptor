using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;
using CSCore.CoreAudioAPI;
using System.Media;
using System.Collections.Generic;
using AutoHotkey.Interop;

namespace Dota2AA
{
    public partial class Form1 : Form
    {


        IntPtr localhWnd;

        Boolean scanning = false;

        System.Timers.Timer checkTimer;
        System.Timers.Timer delayTimer;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);


        public Form1()
        {
            InitializeComponent();

            //Safe Windowhandle
            localhWnd = Handle;

            //Initialize the scanning timer
            checkTimer = new System.Timers.Timer(1000);
            checkTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent); //OnTimedEvent -> Gets called every interval.
            checkTimer.Enabled = false;

            //Initialize the delay timer for use after accepting to restart scanning
            delayTimer = new System.Timers.Timer(5000);
            delayTimer.Elapsed += (s, e) => { checkTimer.Enabled = true; Debug.WriteLine("DelayTimer Executed"); }; //Restart scanning
            delayTimer.Elapsed += (s, e) => { ((System.Timers.Timer)s).Enabled = false; }; //Stop the delay timer (Only exec once)
            delayTimer.Enabled = false;

        }


        private static bool checkForSoundPeak()
        {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnum = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnum)
                    {
                        using (var session2 = session.QueryInterface<AudioSessionControl2>())
                        using (var audioMeterInformation = session.QueryInterface<AudioMeterInformation>())
                        {
                            //See if Dota emits sound (If there are two Windows containing Dota in the title sound of either one will trigger!!
                            if (session2.Process.MainWindowTitle.Contains("Dota") && (audioMeterInformation.GetPeakValue() > 0.015))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {

                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }

        //Prevent user interference 
        private void HideDota()
        {
            IntPtr hWnd = FindWindow(null, "dota 2");
            if (!hWnd.Equals(IntPtr.Zero))
            {
                ShowWindowAsync(hWnd, SW_SHOWMINIMIZED);
            }
        }
        private void btn_toggleScan_Click(object sender, EventArgs e)
           {

            Process p = Process.GetProcessesByName("dota2").FirstOrDefault();
            if (p == null)
            {
                lbl_status.Text = "Dota 2 not found";
                return;
            };

            Process[] q = Process.GetProcessesByName("dota2");
            if (q.Length == 0)
                MessageBox.Show("Dota 2 not running");
            else
            {
                HideDota();
            }

            scanning = !scanning;
            checkTimer.Enabled = scanning;
            delayTimer.Enabled = false;
            string btntxt = "Start";

            if (scanning)
            {
                btntxt = "Stop";
            }

            btn_toggleScan.Text = btntxt;
            string lbltxt = "Disabled";

            if (scanning)
            {
                lbltxt = "Scanning";
            }

            lbl_status.Text = lbltxt;
        }
        bool matchfound = false;
        
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var script = AutoHotkeyEngine.Instance;
            if (checkForSoundPeak())
            {
                checkTimer.Enabled = false;

                script.ExecRaw("ControlSend,,{Enter}, Dota 2");

                SetForegroundWindow(localhWnd);
                delayTimer.Enabled = true;
                matchfound = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Label3_Click(object sender, EventArgs e)
        {

        }

        private void Lbl_deviceStatus_Click(object sender, EventArgs e)
        {

        }

        private void Lbl_status_Click(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox q = (CheckBox)sender;
            TopMost = false;
            if (q.Checked)
            {
                TopMost = true;
            }
        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.ShowAlways = true;
            tip.SetToolTip(btn_toggleScan, "If you override this, you could miss the match.");

            CheckBox b = (CheckBox)sender;
            scanning = false;
            if (b.Checked)
            {
                scanning = true;
            }
            else
            {
                DialogResult dr = MessageBox.Show("Are you sure?",
                      "Forcing this can get banned for 5 min if failed to accept.", MessageBoxButtons.YesNo);
                switch (dr)
                {
                    case DialogResult.Yes:
                        scanning = false;
                        break;
                    case DialogResult.No:
                        scanning = true;
                        this.checkBox2.Checked = true;
                        break;
                }
            }
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox d = (CheckBox)sender;
            if (d.Checked && matchfound && checkForSoundPeak())
            {
                SystemSounds.Beep.Play();
            }
        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public string InLock = "  Locked n Loaded";
        private void Button1_Click_1(object sender, EventArgs e)
        {  try
            {//To Finish
             // Read the value from the combobox1
                MessageBox.Show("This feature is not ready");
                KeyValuePair<string, string> kvp = (KeyValuePair<string, string>)comboBox1.SelectedItem;
                string key = kvp.Key.ToString();
                string value = kvp.Value.ToString();
                //lock on label
                //comboBox1.Text=key+""+value;
                comboBox1.Text = key + InLock;
            }
            catch { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(button1, "Incomplete.");
            toolTip1.SetToolTip(btn_toggleScan, "Start the match listener, safe to stop this when a match is found.");
            toolTip1.SetToolTip(checkBox1, "Keep this window above all.");
           /*
           comboBox1.Items.Add(new KeyValuePair<string, string>("Dazzle","0"));
           comboBox1.Items.Add(new KeyValuePair<string, string>("Mars", "1"));
           comboBox1.Items.Add(new KeyValuePair<string, string>("Bristleback", "2"));
           all more heroes
           comboBox1.DisplayMember = "key";
           */
        }

        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        private void Label3_Click_1(object sender, EventArgs e)
        {

        }

        private void Lbl_status_TextChanged(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            if (lbl_status.Text == "Disabled")
            {
                toolTip1.SetToolTip(lbl_status, "Nothing is happening.");
            }
            if (lbl_status.Text == "Scanning")
            {
                toolTip1.SetToolTip(lbl_status, "Waiting for a accept button, you can continue to be AFK!");
            }
        }
    }
}