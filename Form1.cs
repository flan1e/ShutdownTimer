using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShutdownTimer
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMinutes.Text, out int minutes) || minutes <= 0)
            {
                MessageBox.Show("Введите положительное число минут!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cts = new CancellationTokenSource();
            btnStart.Enabled = false;
            btnCancel.Enabled = true;

            try
            {
                await Task.Delay(minutes * 60 * 1000, cts.Token);
                Process.Start("shutdown", "/s /t 0");
                Application.Exit();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            Process.Start("shutdown", "/a"); 
            btnStart.Enabled = true;
            btnCancel.Enabled = false;
            MessageBox.Show("Выключение отменено!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}