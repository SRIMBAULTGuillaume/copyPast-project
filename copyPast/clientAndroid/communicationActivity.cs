using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace clientAndroid
{
    [Activity(Label = "clientAndroid")]
    class communicationActivity : Activity
    {
        public TcpClient tcpClient;
        public Stream stm;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Communication);

            var toolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            var button = FindViewById<Button>(Resource.Id.sendButton);

            SetActionBar(toolbar);
            ActionBar.Title = "Copy/Paste";
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ActionBar.SetHomeButtonEnabled(true);

            button.Click += onConnectButtonClick;

            try {
                string myTcpIP   = Intent.GetStringExtra("myTcpIP");
                string myTcpPort = Intent.GetStringExtra("myTcpPort");
                string myTcpID   = Intent.GetStringExtra("myTcpID");

                tcpClient = new TcpClient();
                tcpClient.Connect(myTcpIP, Int32.Parse(myTcpPort));
                stm = tcpClient.GetStream();
                sendMessage(myTcpID);


                Thread listener = new Thread(new ThreadStart(waitForMessage));
                listener.SetApartmentState(ApartmentState.STA);
                listener.Start();

            } catch (SocketException ex){
                Toast.MakeText(this, "Error during connection, please try again.", ToastLength.Short).Show();
                Finish();
            } catch (Exception ex) {
                Toast.MakeText(this, "Error", ToastLength.Short).Show();
                Finish();
            }

        }

        public void onConnectButtonClick(object sender, System.EventArgs e)
        {
            var textBox = FindViewById<TextView>(Resource.Id.editText);

            if (textBox.Text != "") {
                sendMessage(textBox.Text);
                newMessage(textBox.Text);
                textBox.Text = "";
            }
        }

        public void newMessage(string msg)
        {
            var textView = FindViewById<TextView>(Resource.Id.textView);

            if (textView.Text == "") {
                textView.Text = msg;
            } else {
                textView.Text += "\n" + msg;
            }
        }

        private void sendMessage(string message)
        {
            byte[] msg = System.Text.Encoding.Unicode.GetBytes(message);
            stm.Write(msg, 0, msg.Length);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home) {
                tcpClient.Close();
                Finish();
            } else {
                Toast.MakeText(this, "Action not implemented yet !", ToastLength.Short).Show();
            }

            return base.OnOptionsItemSelected(item);
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
            }
            tcpClient.Close();
            Finish();
        }
        
    }
}