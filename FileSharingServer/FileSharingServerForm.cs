using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

namespace FileSharingServer
{
    public partial class FileSharingServerForm : Form
    {
        
        public delegate void FileRecievedEventHandler(object source, string fileName);
        public event FileRecievedEventHandler NewFileRecieved;  

        public FileSharingServerForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.NewFileRecieved+=new FileRecievedEventHandler(Form1_NewFileRecieved);
        }

        private void Form1_NewFileRecieved(object sender, string fileName)
        {
            this.BeginInvoke(                
            new Action(
            delegate()
            {
                lbl_notification.Text = "New File Recieved\n" + fileName;
                lbl_notification.ForeColor = Color.Green;
                System.Diagnostics.Process.Start("explorer", @"c:\");
            }));
        }

        

        private void btnListen_Click(object sender, EventArgs e)
        {
            int port = int.Parse(txtHost.Text);
            Task.Factory.StartNew(() => HandleIncomingFile(port));
            btnListen.Text = "Listening";
            btnListen.ForeColor = Color.Green;
            btnListen.Enabled = false;
        }


        public void HandleIncomingFile(int port)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(port);
                tcpListener.Start();
                while (true)
                {
                    Socket handlerSocket = tcpListener.AcceptSocket();
                    if (handlerSocket.Connected)
                    {
                        string fileName = string.Empty;
                        NetworkStream networkStream = new NetworkStream(handlerSocket);
                        int thisRead = 0;
                        int blockSize = 1024;
                        Byte[] dataByte = new Byte[blockSize];
                        lock (this)
                        {
                            string folderPath = @"c:\";
                            handlerSocket.Receive(dataByte);
                            int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                            fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);
                            Stream fileStream = File.OpenWrite(folderPath + fileName);
                            fileStream.Write(dataByte, 4+fileNameLen,(1024-(4+fileNameLen)));
                            while (true)
                            {
                                thisRead = networkStream.Read(dataByte, 0, blockSize);
                                fileStream.Write(dataByte, 0,thisRead);
                                if (thisRead == 0)
                                    break;
                            }
                            fileStream.Close();
                        }
                        if (NewFileRecieved != null)
                        {
                            NewFileRecieved(this, fileName);
                        }
                        handlerSocket = null;
                    }
                }
            }
            catch (Exception ex)
            {
                lbl_notification.Text = "Error: " + ex.ToString();
                lbl_notification.ForeColor = Color.Red;
            }
        }
    }
}
