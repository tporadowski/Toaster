using System;
using System.Linq;
using System.Windows.Forms;

namespace Sample
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			for (var i = 0; i <= 4; i++)
			{
				var notification = Toaster.Toast.ShowNotification("New message from slackbot " + i,
					"Hey, you asked me to remind you to contact payroll regarding invoice changes.\nwhat if we had some more text here? what to do then?", "https://slack.global.ssl.fastly.net/20655/img/slack_growl_icon.png", "Tag_123456789", 15000);
				notification.FormClosed += delegate { System.Diagnostics.Debug.WriteLine(notification.Dismissed ? notification.Tag + " was dismissed." : notification.Tag + " was not dismissed."); };
				notification.FormClosed += delegate { if (Toaster.Toast.ActiveToasts.Count == 0) this.Close(); };
			}
		}
	}
}