using System;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsApp_Logger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            button1.Click += new EventHandler(button1_Click);
            button2.Click += new EventHandler(button2_Click);
            button3.Click += new EventHandler(button3_Click);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Logger.Instance.LogData(LogLevel.Debug, "Message 1 (Debug mode activated)");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Logger.Instance.LogData(LogLevel.Warning, "Message 2 (System warning detected)");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Thread thread1 = new Thread(SetMessage);
            Thread thread2 = new Thread(SetMessage);

            thread1.Start(1);
            thread2.Start(2);
        }

        private void SetMessage(object n)
        {
            int number = (int)n;
            Logger.Instance.LogData(LogLevel.Error, $"Message from different thread. Number = {number}");
        }
    }
}
