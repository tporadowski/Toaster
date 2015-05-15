using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Toaster
{
	public partial class BaseForm : Form
	{
		private Timer timer = new Timer();
		internal DateTime DateShown = DateTime.Now;
		private String iconUrl = "";

		public String ToastIconUrl
		{
			get { return this.iconUrl; }
			set
			{
				this.iconUrl = value;
				if (!String.IsNullOrEmpty(this.iconUrl))
				{
					System.Net.WebRequest request = System.Net.WebRequest.Create(this.iconUrl);
					using (System.Net.WebResponse response = request.GetResponse()) { this.NotificationIcon = new Bitmap(response.GetResponseStream()); }
				}
			}
		}

		public Icon ToastIcon
		{
			set
			{
				if (value != null)
				{
					NotificationIcon = value.ToBitmap();
				}
			}
		}

		public String Title { get { return this.titleLabel.Text; } set { this.titleLabel.Text = value; } }

		public String Body { get { return this.bodyLabel.Text; } set { this.bodyLabel.Text = value; } }

		public int Duration { get { return this.timer.Interval; } set { this.timer.Interval = value; } }

		private Bitmap NotificationIcon = null;
		public Boolean Dismissed = false;

		#region "Constructors"

		public BaseForm(String title, String body, int duration = 15000)
		{
			InitializeComponent();
			this.Title = title;
			this.Body = body;
			this.Duration = duration;
		}

		//public BaseForm(String title, String body, int duration = 15000) {
		//    InitializeComponent();
		//    this.titleLabel.Text = title;
		//    this.bodyLabel.Text = body;
		//    this.timer.Interval = duration;
		//}
		//public BaseForm(String title, String body, String iconUrl, int duration = 15000) {
		//    InitializeComponent();
		//    this.titleLabel.Text = title;
		//    this.bodyLabel.Text = body;
		//    this.IconUrl = new Uri(iconUrl);
		//    this.timer.Interval = duration;
		//    System.Net.WebRequest request = System.Net.WebRequest.Create(this.IconUrl.ToString());
		//    using (System.Net.WebResponse response = request.GetResponse()) { this.NotificationIcon = new Bitmap(response.GetResponseStream()); }
		//}

		#endregion "Constructors"

		internal void FadeOut(int duration = 500)
		{
			var amount = (double)15 / duration;
			var fadeoutTimer = new Timer { Interval = 15, Enabled = true };
			fadeoutTimer.Tick += delegate
			{
				this.Opacity = Math.Max(0, this.Opacity - amount);
				if (this.Opacity <= 0) { this.Close(); }
			};
		}

		internal void Reposition()
		{
			var newTop = Screen.PrimaryScreen.WorkingArea.Height - Toaster.Toast.ActiveToasts.Where(x => x.DateShown <= this.DateShown).Sum(x => x.Height + 5);
			var offset = (newTop - this.Top) / 10;
			var animateTimer = new Timer() { Interval = 10 };
			animateTimer.Tick += delegate
			{
				this.Top = Math.Min(newTop, this.Top + offset);
				if (this.Top == newTop)
				{
					animateTimer.Stop();
				}
			};
			animateTimer.Start();
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

			// Draw the background.
			var mainBackgroundBrush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(255, 255, 255), Color.FromArgb(235, 235, 235), LinearGradientMode.Vertical);
			e.Graphics.FillRectangle(mainBackgroundBrush, this.ClientRectangle);

			// If there is an icon, draw the icon and icon background.
			if (this.NotificationIcon != null)
			{
				var iconSectionRectable = new Rectangle(0, 0, this.ClientRectangle.Height, this.ClientRectangle.Height);
				var iconBackgroundBrush = new LinearGradientBrush(iconSectionRectable, Color.FromArgb(5, 0, 0, 0), Color.FromArgb(10, 0, 0, 0), LinearGradientMode.Vertical);
				e.Graphics.FillRectangle(iconBackgroundBrush, iconSectionRectable);
				e.Graphics.DrawImage(this.NotificationIcon, 20, this.Height / 2 - this.NotificationIcon.Height / 2);
			}

			// Draw the form border.
			var borderPen = new Pen(Color.FromArgb(200, 200, 200));
			e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
		}

		#region "Event Handlers"

		private void BaseForm_Load(object sender, EventArgs e)
		{
			// TODO: Determine which screen to show it on. For now we will show it on the primary screen.
			// TODO: Determine which corner to show it on. For now we will show it on the bottom right.

			this.Top = Screen.PrimaryScreen.WorkingArea.Height - Toaster.Toast.ActiveToasts.Sum(x => x.Height + 5);
			this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 5;

			// Shift the text over if we're going to place an icon.
			if (this.NotificationIcon != null)
			{
				titleLabel.Width -= 80;
				titleLabel.Left += 80;

				bodyLabel.Width -= 80;
				bodyLabel.Left += 80;
			}

			timer.Start();
			timer.Tick += delegate { timer.Enabled = false; this.FadeOut(); };
		}

		private void closeLabel_Click(object sender, EventArgs e)
		{
			this.Dismissed = true;
			this.FadeOut(150);
		}

		private void closeLabel_MouseEnter(object sender, EventArgs e)
		{
			closeLabel.BackColor = Color.FromArgb(225, 225, 225);
		}

		private void closeLabel_MouseLeave(object sender, EventArgs e)
		{
			closeLabel.BackColor = Color.FromArgb(240, 240, 240);
		}

		private void BaseForm_MouseClick(object sender, MouseEventArgs e)
		{
			this.FadeOut(150);
		}

		#endregion "Event Handlers"
	}
}