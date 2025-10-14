using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ShutdownTimer
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            var arguments = ToastArguments.Parse(e.Argument);

            if (arguments.TryGetValue("action", out string action) && action == "cancel_shutdown")
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(CancelShutdownFromToast));
                }
                else
                {
                    CancelShutdownFromToast();
                }
            }
        }

        private void CancelShutdownFromToast()
        {
            if (cts != null && !cts.Token.IsCancellationRequested)
            {
                cts.Cancel();
                Process.Start("shutdown", "/a");
                btnStart.Enabled = true;
                btnCancel.Enabled = false;

                ShowToast("Shutdown Canceled!", "The timer is stopped");
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMinutes.Text, out int totalMinutes) || totalMinutes <= 0)
            {
                MessageBox.Show("Enter a positive number of minutes!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cts = new CancellationTokenSource();
            btnStart.Enabled = false;
            btnCancel.Enabled = true;

            int totalSeconds = totalMinutes * 60;

            if (totalSeconds > 60)
            {
                _ = Task.Delay((totalSeconds - 60) * 1000, cts.Token)
                    .ContinueWith(t =>
                    {
                        if (!t.IsCanceled && !cts.Token.IsCancellationRequested)
                        {
                            ShowToast("Shutdown is coming soon!", "The computer will be turned off in 1 minute.");
                        }
                    }, TaskScheduler.Default);
            }
            else
            {
                ShowWarningToastWithCancel();
            }

            try
            {
                ShowToast("The timer is set!", "The computer will shut down in " + txtMinutes.Text + " minutes. Please do not close the app.");
                await Task.Delay(totalSeconds * 1000, cts.Token);

                if (!cts.Token.IsCancellationRequested)
                {
                    Process.Start("shutdown", "/s /t 0");
                    Application.Exit();
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
        private void ShowWarningToastWithCancel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowWarningToastWithCancel));
                return;
            }

            const string cancelAction = "cancel_shutdown";

            new ToastContentBuilder()
                .AddText("The lights go out...")
                .AddText("The computer will be turned off in 1 minute.")
                .AddButton(new ToastButton()
                    .SetContent("Cancel shutdown")
                    .AddArgument("action", cancelAction) 
                )
                .Show();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            Process.Start("shutdown", "/a");
            btnStart.Enabled = true;
            btnCancel.Enabled = false;
            ShowToast("Shutdown canceled", "Timer is stopped.");
        }

        private void ShowToast(string title, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowToast(title, message)));
                return;
            }

            new ToastContentBuilder().AddText(title).AddText(message).Show(); 
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cts != null && !cts.Token.IsCancellationRequested)
            {
                cts.Cancel();
                Process.Start("shutdown", "/a"); 
            }
        }
    }
}