using System;
using System.Windows.Forms;
using POCOGenerator;
using POCOGeneratorUI.ConnectionDialog;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region Connect

		private RDBMS _rdbms;
		private string _connectionString;
		private IGenerator _generator;

		private void btnConnect_Click(object sender, EventArgs e)
		{
			RDBMS rdbms = _rdbms;
			string connectionString = _connectionString;
			Exception error = null;
			bool succeeded = GetConnectionString(ref rdbms, ref connectionString, ref error);
			if (succeeded)
			{
				if (rdbms != RDBMS.None && string.IsNullOrEmpty(connectionString) == false)
				{
					IGenerator generator = GetGenerator(rdbms, connectionString);
					if (generator != null)
					{
						_rdbms = rdbms;
						_connectionString = connectionString;
						_generator = generator;
						BuildServerTree();
					}
				}
			}
			else
			{
				string errorMessage = "Failed to retrieve connection string.";
				if (error != null)
				{
					errorMessage += Environment.NewLine + error.Message;
				}
				MessageBox.Show(this, errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool GetConnectionString(ref RDBMS rdbms, ref string connectionString, ref Exception error)
		{
			try
			{
				DataConnectionDialog dcd = new(rdbms, connectionString);
				if (dcd.ShowDialog(this) == DialogResult.OK)
				{
					rdbms = dcd.RDBMS;
					connectionString = dcd.ConnectionString;
				}
				else
				{
					rdbms = RDBMS.None;
				}
				return true;
			}
			catch (Exception ex)
			{
				error = ex;
				return false;
			}
		}

		#endregion
	}
}
