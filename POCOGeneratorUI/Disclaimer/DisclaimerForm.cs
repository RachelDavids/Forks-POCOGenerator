using System.Drawing;
using System.Windows.Forms;

namespace POCOGeneratorUI.Disclaimer
{
	public sealed partial class DisclaimerForm
		: Form
	{
		internal DisclaimerForm(string disclaimer)
		{
			InitializeComponent();
			BackColor = Color.White;
			warningIcon.Image = SystemIcons.Warning.ToBitmap();
			lblDisclaimer.Text = disclaimer;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData is Keys.Enter or Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
