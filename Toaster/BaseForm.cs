using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Toaster
{
	public partial class BaseForm : Form
	{
		#region Public Fields

		/// <summary>
		/// The dismissed indicator.
		/// </summary>
		public Boolean Dismissed = false;

		#endregion Public Fields

		#region Internal Fields

		internal DateTime DateShown = DateTime.Now;

		#endregion Internal Fields

		#region Private Fields

		private String iconUrl = "";

		private Bitmap NotificationIcon = null;

		private Timer timer = new Timer();

		#endregion Private Fields

		#region Public Properties

		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// <value>The body.</value>
		public String Body { get { return this.bodyLabel.Text; } set { this.bodyLabel.Text = value; } }

		/// <summary>
		/// Gets or sets the duration.
		/// </summary>
		/// <value>The duration.</value>
		public int Duration { get { return this.timer.Interval; } set { this.timer.Interval = value; } }

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public String Title { get { return this.titleLabel.Text; } set { this.titleLabel.Text = value; } }

		/// <summary>
		/// Sets the toast icon.
		/// </summary>
		/// <value>The toast icon.</value>
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

		/// <summary>
		/// Gets or sets the toast icon URL.
		/// </summary>
		/// <value>The toast icon URL.</value>
		public String ToastIconUrl
		{
			get
			{
				return this.iconUrl;
			}
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

		#endregion Public Properties

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseForm"/> class.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="body">The body.</param>
		/// <param name="duration">The duration.</param>
		public BaseForm(String title, String body, int duration = 15000)
		{
			InitializeComponent();
			this.Title = title;
			this.Body = body;
			this.Duration = duration;
		}

		#endregion Public Constructors

		#region Public Events

		/// <summary>
		/// Occurs when any control (except closeLabel) is clicked.
		/// </summary>
		public event EventHandler<EventArgs> FormClick;

		#endregion Public Events

		#region Internal Methods

		/// <summary>
		/// Fades out this form.
		/// </summary>
		/// <param name="duration">The duration.</param>
		internal void FadeOut(int duration = 500)
		{
			var amount = (double) 15 / duration;
			var fadeoutTimer = new Timer { Interval = 15, Enabled = true };
			fadeoutTimer.Tick += delegate
			{
				this.Opacity = Math.Max(0, this.Opacity - amount);
				if (this.Opacity <= 0) { this.Close(); }
			};
		}

		/// <summary>
		/// Repositions this form.
		/// </summary>
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

		#endregion Internal Methods

		#region Protected Methods

		/// <summary>
		/// Called when any form control is clicked (except for special closeLabel).
		/// </summary>
		protected void OnFormClick()
		{
			if (FormClick != null)
			{
				FormClick(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Paints the background of the control.
		/// </summary>
		/// <param name="e">
		/// A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.
		/// </param>
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

		#endregion Protected Methods

		#region Private Methods

		/// <summary>
		/// Handles the Load event of the BaseForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

		/// <summary>
		/// Handles the MouseClick event of the BaseForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void BaseForm_MouseClick(object sender, MouseEventArgs e)
		{
			OnFormClick();
			this.FadeOut(150);
		}

		/// <summary>
		/// Handles the Click event of the closeLabel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void closeLabel_Click(object sender, EventArgs e)
		{
			this.Dismissed = true;
			this.FadeOut(150);
		}

		/// <summary>
		/// Handles the MouseEnter event of the closeLabel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void closeLabel_MouseEnter(object sender, EventArgs e)
		{
			closeLabel.BackColor = Color.FromArgb(225, 225, 225);
		}

		/// <summary>
		/// Handles the MouseLeave event of the closeLabel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void closeLabel_MouseLeave(object sender, EventArgs e)
		{
			closeLabel.BackColor = Color.FromArgb(240, 240, 240);
		}

		#endregion Private Methods
	}
}