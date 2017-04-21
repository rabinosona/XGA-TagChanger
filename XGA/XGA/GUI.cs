using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XGA
{
    public partial class GUI : Form
    {
        public GUI()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            Status.Text = "Status: waiting for data.";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void GUI_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            XboxLiveWorker worker = new XboxLiveWorker();

            worker.input(this);
            int i = 0;

            while (worker.status.All(x => x))
            {
                Thread.Sleep(150);
                Parallel.For(i = 0, worker.gamer_tags.Count(s => s != null), new ParallelOptions() { MaxDegreeOfParallelism = 1 }, index =>
                {

                    worker.loginXboxLive(worker.username, worker.password, this);
                });
            }
        }
    }
}
