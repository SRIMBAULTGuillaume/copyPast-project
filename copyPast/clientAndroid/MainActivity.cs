using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System;
using System.IO;

namespace clientAndroid
{
    [Activity(Label = "clientAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            var button = FindViewById<Button>(Resource.Id.connectButton);

            SetActionBar(toolbar);
            ActionBar.Title = "Copy/Paste";
                        
            button.Click += onConnectButtonClick;
        }

        public void onConnectButtonClick(object sender, System.EventArgs e)
        {
            //Setting the input boxes
            var ipBox = FindViewById<EditText>(Resource.Id.ipBox);
            var portBox = FindViewById<EditText>(Resource.Id.portBox);
            var IDBox = FindViewById<EditText>(Resource.Id.IDBox);
            
            //Chech the IP's validity
            Regex ipRegex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
            Regex portRegex = new Regex(@"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$");
            Regex IDRegex = new Regex(@"^[a-zA-Z0-9]+$");
            if (ipRegex.IsMatch(ipBox.Text) && portRegex.IsMatch(portBox.Text) && IDRegex.IsMatch(IDBox.Text)) {
                //Creating activity
                try {
                    var myNewActivity = new Android.Content.Intent(this, typeof(communicationActivity));
                    myNewActivity.PutExtra("myTcpIP", ipBox.Text);
                    myNewActivity.PutExtra("myTcpPort", portBox.Text);
                    myNewActivity.PutExtra("myTcpID", IDBox.Text);
                    StartActivity(myNewActivity);
                } catch (Exception ex) {
                    Toast.MakeText(this, "Error during activity creation", ToastLength.Short).Show();
                }
            } else {
                Toast.MakeText(this, "Invalid IP adress, port or ID", ToastLength.Short).Show();
            }
                
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Toast.MakeText(this, "Action not implemented yet !", ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }
    }
}

