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

namespace FrankenMAGateway
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Gateway Gateway { get; set; } = new Gateway();


        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Closing += MainWindow_Closing;
            Gateway.Start();

            Gateway.PropertyChanged += Gateway_PropertyChanged;

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "FrankenMA is not connected";
            m_notifyIcon.BalloonTipTitle = "FrankenMA";
            m_notifyIcon.Text = "NOT CONNECTED";
            m_notifyIcon.Icon = new System.Drawing.Icon("red.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = true;
        }

        private void m_notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void Gateway_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Gateway.TelnetStatus && Gateway.PortStatuses[0] && Gateway.PortStatuses[1] && Gateway.PortStatuses[2])
            {
                if (m_notifyIcon == null)
                {
                    return;
                }
                m_notifyIcon.Icon = new System.Drawing.Icon("green.ico");
                m_notifyIcon.Text = "CONNECTED";
                m_notifyIcon.BalloonTipText = "FrankenMA is connected";
            } else
            {
                if (m_notifyIcon == null)
                {
                    return;
                }
                m_notifyIcon.Icon = new System.Drawing.Icon("red.ico");
                m_notifyIcon.Text = "NOT CONNECTED";
                m_notifyIcon.BalloonTipText = "FrankenMA is not connected";
            }
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = true;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
            Gateway.Stop();
        }

    }
}
