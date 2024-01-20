using System;
using POCOGeneratorUI.Disclaimer;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region Disclaimer

		private void btnDisclaimer_Click(object sender, EventArgs e) => ShowDisclaimer();
		private DisclaimerForm DisclaimerForm;

		private void ShowDisclaimer()
		{
			DisclaimerForm ??= new DisclaimerForm(POCOGenerator.Disclaimer.Message.Replace(Environment.NewLine, " "));
			DisclaimerForm.ShowDialog(this);
		}

		#endregion
	}
}
