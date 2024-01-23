using PavelTorgashov.Forms;

namespace FastColoredTextBoxDemo
{
	sealed partial class DemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			txtPocoEditor = new PavelTorgashov.Forms.FastColoredTextBox();
			btnGenerate = new System.Windows.Forms.Button();
			btnClose = new System.Windows.Forms.Button();
			btnClear = new System.Windows.Forms.Button();
			txtConnectionString = new System.Windows.Forms.TextBox();
			btnCopy = new System.Windows.Forms.Button();
			flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			rdbLightTheme = new System.Windows.Forms.RadioButton();
			rdbDarkTheme = new System.Windows.Forms.RadioButton();
			flowLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// txtPocoEditor
			// 
			txtPocoEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			txtPocoEditor.BackColor = System.Drawing.Color.White;
			txtPocoEditor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			txtPocoEditor.Language = Language.CSharp;
			//txtPocoEditor.DetectUrls = false;
			txtPocoEditor.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			txtPocoEditor.Location = new System.Drawing.Point(0, 0);
			txtPocoEditor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			txtPocoEditor.Name = "txtPocoEditor";
			txtPocoEditor.ReadOnly = true;
			txtPocoEditor.Size = new System.Drawing.Size(1098, 452);
			txtPocoEditor.TabIndex = 0;
			txtPocoEditor.Text = "";
			txtPocoEditor.WordWrap = false;
			// 
			// btnGenerate
			// 
			btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			btnGenerate.AutoSize = true;
			btnGenerate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			btnGenerate.Location = new System.Drawing.Point(18, 461);
			btnGenerate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			btnGenerate.Name = "btnGenerate";
			btnGenerate.Size = new System.Drawing.Size(89, 25);
			btnGenerate.TabIndex = 1;
			btnGenerate.Text = "Generate";
			btnGenerate.UseVisualStyleBackColor = true;
			btnGenerate.Click += OnGenerateClick;
			// 
			// btnClose
			// 
			btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnClose.AutoSize = true;
			btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			btnClose.Location = new System.Drawing.Point(1020, 495);
			btnClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			btnClose.Name = "btnClose";
			btnClose.Size = new System.Drawing.Size(62, 25);
			btnClose.TabIndex = 5;
			btnClose.Text = "Close";
			btnClose.UseVisualStyleBackColor = true;
			btnClose.Click += OnCloseClick;
			// 
			// btnClear
			// 
			btnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			btnClear.AutoSize = true;
			btnClear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			btnClear.Location = new System.Drawing.Point(189, 461);
			btnClear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			btnClear.Name = "btnClear";
			btnClear.Size = new System.Drawing.Size(62, 25);
			btnClear.TabIndex = 4;
			btnClear.Text = "Clear";
			btnClear.UseVisualStyleBackColor = true;
			btnClear.Click += OnClearClick;
			// 
			// txtConnectionString
			// 
			txtConnectionString.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			txtConnectionString.Location = new System.Drawing.Point(18, 493);
			txtConnectionString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			txtConnectionString.Name = "txtConnectionString";
			txtConnectionString.Size = new System.Drawing.Size(990, 22);
			txtConnectionString.TabIndex = 2;
			// 
			// btnCopy
			// 
			btnCopy.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			btnCopy.AutoSize = true;
			btnCopy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			btnCopy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			btnCopy.Location = new System.Drawing.Point(118, 461);
			btnCopy.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			btnCopy.Name = "btnCopy";
			btnCopy.Size = new System.Drawing.Size(53, 25);
			btnCopy.TabIndex = 3;
			btnCopy.Text = "Copy";
			btnCopy.UseVisualStyleBackColor = true;
			btnCopy.Click += OnCopyClick;
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			flowLayoutPanel1.AutoSize = true;
			flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			flowLayoutPanel1.Controls.Add(rdbLightTheme);
			flowLayoutPanel1.Controls.Add(rdbDarkTheme);
			flowLayoutPanel1.Location = new System.Drawing.Point(260, 461);
			flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new System.Drawing.Size(255, 25);
			flowLayoutPanel1.TabIndex = 6;
			// 
			// rdbLightTheme
			// 
			rdbLightTheme.AutoSize = true;
			rdbLightTheme.Checked = true;
			rdbLightTheme.Location = new System.Drawing.Point(4, 3);
			rdbLightTheme.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			rdbLightTheme.Name = "rdbLightTheme";
			rdbLightTheme.Size = new System.Drawing.Size(124, 19);
			rdbLightTheme.TabIndex = 0;
			rdbLightTheme.TabStop = true;
			rdbLightTheme.Text = "Light Theme";
			rdbLightTheme.UseVisualStyleBackColor = true;
			// 
			// rdbDarkTheme
			// 
			rdbDarkTheme.AutoSize = true;
			rdbDarkTheme.Location = new System.Drawing.Point(136, 3);
			rdbDarkTheme.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			rdbDarkTheme.Name = "rdbDarkTheme";
			rdbDarkTheme.Size = new System.Drawing.Size(115, 19);
			rdbDarkTheme.TabIndex = 1;
			rdbDarkTheme.Text = "Dark Theme";
			rdbDarkTheme.UseVisualStyleBackColor = true;
			// 
			// DemoForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(9F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = btnClose;
			ClientSize = new System.Drawing.Size(1101, 533);
			Controls.Add(flowLayoutPanel1);
			Controls.Add(btnClose);
			Controls.Add(btnCopy);
			Controls.Add(btnClear);
			Controls.Add(txtPocoEditor);
			Controls.Add(txtConnectionString);
			Controls.Add(btnGenerate);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "DemoForm";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "FastColoredTextBox Demo";
			flowLayoutPanel1.ResumeLayout(false);
			flowLayoutPanel1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private PavelTorgashov.Forms.FastColoredTextBox txtPocoEditor;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.RadioButton rdbLightTheme;
        private System.Windows.Forms.RadioButton rdbDarkTheme;
    }
}

