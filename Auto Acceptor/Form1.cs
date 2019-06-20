using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;
using CSCore.CoreAudioAPI;


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
        

        private static bool checkForSoundPeak() {


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



        private void accept_match()
        {
            //Disable Scanning
            checkTimer.Enabled = false;
            
            //Execute the Keystroke Simulator to submit enter
            String appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var programPath = System.IO.Path.Combine(appStartPath, "keystroke_simulator.exe");
            Debug.WriteLine(programPath);
            ProcessStartInfo kssi = new ProcessStartInfo(programPath);
            Process kss = Process.Start(kssi);
            kss.WaitForExit(3000);

            Debug.WriteLine("Keystokes sent");
            SetForegroundWindow(localhWnd);

            //Restart scanning after 5 seconds in case of declination
            delayTimer.Enabled = true;  
        }



        private void btn_toggleScan_Click(object sender, EventArgs e)
        {
            //Search for the Dota 2 Process
            Process p = Process.GetProcessesByName("dota2").FirstOrDefault();
            if (p == null)
            {
                lbl_status.Text = "Dota 2 not found";
                return;
            };
           

            //Implement Toggle behaviour of the button
            scanning = !scanning;
            checkTimer.Enabled = scanning;
            delayTimer.Enabled = false;
            string btntxt = "Start";

            if(scanning){

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

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (checkForSoundPeak())
            {
                accept_match();
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
    }
}