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
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;

namespace clientWindows
{
    public partial class Form1 : Form
    {
        private Stream stm;
        private TcpClient tcpClient;
        delegate void newMessageCallback(string msg);
        delegate void enableAllCallback(Boolean isConnected);

        public Form1()
        {
            InitializeComponent();

            this.ipBox.Text = "127.0.0.1";
            this.portBox.Text = "8000";

            enableAll(false);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            connect();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            disconnect();
        }


        private void connect()
        {
            //Chech the IP's validity
            Regex ipRegex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
            Regex portRegex = new Regex(@"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$");
            Regex IDRegex = new Regex(@"^[a-zA-Z0-9]+$");
            if (ipRegex.IsMatch(this.ipBox.Text) && portRegex.IsMatch(this.portBox.Text) && IDRegex.IsMatch(this.IDBox.Text)) {
                try {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(this.ipBox.Text, Int32.Parse(this.portBox.Text));
                    stm = tcpClient.GetStream();
                    sendMessage(this.IDBox.Text);
                    Thread listener = new Thread(new ThreadStart(waitForMessage));
                    listener.SetApartmentState(ApartmentState.STA);
                    listener.Start();

                    this.AcceptButton = this.sendButton;
                    newMessage("Connected on " + this.ipBox.Text + ":" + this.portBox.Text);
                    enableAll(true);
                } catch (SocketException ex) {
                    newMessage("Connection failed :(");
                    newMessage(ex.ToString());
                } catch (Exception ex) {
                    newMessage(ex.ToString());
                }

            } else {
                System.Windows.Forms.MessageBox.Show("Invalid IP adress, port or ID", "Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void newMessage(string msg)
        {
            if (this.messagesLabel.InvokeRequired) {
                newMessageCallback d = new newMessageCallback(newMessage);
                this.Invoke(d, new object[] { msg });
            } else {
                if (this.messagesLabel.Text == "") {
                    this.messagesLabel.Text = msg;
                } else {
                    this.messagesLabel.Text += Environment.NewLine + msg;
                }
            }
        }

        private void disconnect()
        {
            try {
                tcpClient.Close();

                newMessage("Disconnected");
                this.AcceptButton = this.buttonConnect;
                enableAll(false);
            } catch {
                newMessage("Error");
            }


        }

        private void enableAll(Boolean isConnected)
        {
            if (this.InvokeRequired) {
                enableAllCallback d = new enableAllCallback(enableAll);
                this.Invoke(d, new object[] { isConnected });
            } else {
                //insert    isConnected if the object must be enable when user is connected
                //  else    !isConnected

                this.buttonConnect.Enabled = !isConnected;
                this.buttonDisconnect.Enabled = isConnected;

                this.ipBox.Enabled = !isConnected;
                this.portBox.Enabled = !isConnected;
                this.IDBox.Enabled = !isConnected;

                this.sendButton.Enabled = isConnected;
                this.messageBox.Enabled = isConnected;
            }

        }

        private void sendMessage(string message)
        {
            byte[] msg = System.Text.Encoding.Unicode.GetBytes(message);
            stm.Write(msg, 0, msg.Length);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            sendMessage(this.messageBox.Text);
            newMessage("Message was sent");
        }

        private void waitForMessage()
        {
            byte[] messageByte = new byte[51200];
            string message;
            int bytesRead;

            while (true) {
                bytesRead = 0;

                try {
                    bytesRead = stm.Read(messageByte, 0, messageByte.Length);

                } catch {
                    break;
                }
                if (bytesRead == 0) {
                    //the client has disconnected from the server
                    break;
                }
                message = System.Text.Encoding.Unicode.GetString(messageByte, 0, bytesRead);
                newMessage("Server speak : " + message);
                Clipboard.SetText(message);
            }
            newMessage("Disconnected");
            enableAll(false);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Ctrl + Alt + C
            if (e.Control && e.Alt && e.KeyCode == Keys.C) {
                sendMessage(Clipboard.GetText());
            }
        }
    }
}
