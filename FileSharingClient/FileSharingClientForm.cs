using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;


namespace FileSharingClient
{
    public partial class FileSharingClientForm : Form
    {
        private static string shortFileName = "";
        public FileSharingClientForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog filedlg = new OpenFileDialog();
            filedlg.Title = "File Sharing Client";
            filedlg.ShowDialog();
            txtFile.Text = filedlg.FileName;
            shortFileName = filedlg.SafeFileName;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string ipAddress = txtIPAddress.Text;
            int port = int.Parse(txtHost.Text);
            string fileName = txtFile.Text;
            Task.Factory.StartNew(() => SendFile(ipAddress,port,fileName,shortFileName));
            lbl_notification.Text = "File Sent!";
            lbl_notification.ForeColor = Color.Green;
        }

        public void SendFile(string remoteHostIP, int remoteHostPort, string longFileName, string shortFileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(remoteHostIP))
                {
                    byte[] fileNameByte = Encoding.ASCII.GetBytes(shortFileName);
                    byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                    byte[] fileData = File.ReadAllBytes(longFileName);
                    byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                    fileNameByte.CopyTo(clientData, 4);
                    fileNameLen.CopyTo(clientData, 0);
                    fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                    TcpClient clientSocket = new TcpClient(remoteHostIP, remoteHostPort);
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Write(clientData, 0, clientData.GetLength(0));
                    networkStream.Close();
                }
            }
            catch(Exception ex)
            {
                lbl_notification.Text = "Error: "+ ex.ToString();
            }
        }

    }
}
