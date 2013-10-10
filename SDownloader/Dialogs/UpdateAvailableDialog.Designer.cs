namespace SDownload.Dialogs
{
    partial class UpdateAvailableDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.versionNumberLabel = new System.Windows.Forms.Label();
            this.changeLogBox = new System.Windows.Forms.TextBox();
            this.yesResponseButton = new System.Windows.Forms.Button();
            this.noResponseButton = new System.Windows.Forms.Button();
            this.adWebBrowser = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial Narrow", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 163);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(487, 52);
            this.label1.TabIndex = 0;
            this.label1.Text = "There is a new version available for download! (Note: The browser extension updat" +
    "es automatically so it is highly recommended to update the helper application AS" +
    "AP!)\r\n";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SDownload.Properties.Resources.logo_with_text;
            this.pictureBox1.Location = new System.Drawing.Point(22, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(484, 137);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(19, 215);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 19);
            this.label2.TabIndex = 2;
            this.label2.Text = "Version:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(19, 243);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 19);
            this.label3.TabIndex = 3;
            this.label3.Text = "Changelog:";
            // 
            // versionNumberLabel
            // 
            this.versionNumberLabel.AutoSize = true;
            this.versionNumberLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionNumberLabel.Location = new System.Drawing.Point(98, 216);
            this.versionNumberLabel.Name = "versionNumberLabel";
            this.versionNumberLabel.Size = new System.Drawing.Size(43, 18);
            this.versionNumberLabel.TabIndex = 4;
            this.versionNumberLabel.Text = "2.0.0";
            // 
            // changeLogBox
            // 
            this.changeLogBox.BackColor = System.Drawing.Color.White;
            this.changeLogBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.changeLogBox.Location = new System.Drawing.Point(20, 266);
            this.changeLogBox.Multiline = true;
            this.changeLogBox.Name = "changeLogBox";
            this.changeLogBox.ReadOnly = true;
            this.changeLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.changeLogBox.Size = new System.Drawing.Size(486, 304);
            this.changeLogBox.TabIndex = 5;
            // 
            // yesResponseButton
            // 
            this.yesResponseButton.Location = new System.Drawing.Point(431, 576);
            this.yesResponseButton.Name = "yesResponseButton";
            this.yesResponseButton.Size = new System.Drawing.Size(75, 36);
            this.yesResponseButton.TabIndex = 6;
            this.yesResponseButton.Text = "Install";
            this.yesResponseButton.UseVisualStyleBackColor = true;
            // 
            // noResponseButton
            // 
            this.noResponseButton.Location = new System.Drawing.Point(323, 577);
            this.noResponseButton.Name = "noResponseButton";
            this.noResponseButton.Size = new System.Drawing.Size(102, 35);
            this.noResponseButton.TabIndex = 7;
            this.noResponseButton.Text = "Maybe Later";
            this.noResponseButton.UseVisualStyleBackColor = true;
            // 
            // adWebBrowser
            // 
            this.adWebBrowser.AllowWebBrowserDrop = false;
            this.adWebBrowser.Location = new System.Drawing.Point(512, 12);
            this.adWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.adWebBrowser.Name = "adWebBrowser";
            this.adWebBrowser.ScriptErrorsSuppressed = true;
            this.adWebBrowser.ScrollBarsEnabled = false;
            this.adWebBrowser.Size = new System.Drawing.Size(160, 600);
            this.adWebBrowser.TabIndex = 8;
            this.adWebBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // UpdateAvailableDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(682, 627);
            this.Controls.Add(this.adWebBrowser);
            this.Controls.Add(this.noResponseButton);
            this.Controls.Add(this.yesResponseButton);
            this.Controls.Add(this.changeLogBox);
            this.Controls.Add(this.versionNumberLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Name = "UpdateAvailableDialog";
            this.Text = "SDownload";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label versionNumberLabel;
        private System.Windows.Forms.TextBox changeLogBox;
        private System.Windows.Forms.Button yesResponseButton;
        private System.Windows.Forms.Button noResponseButton;
        private System.Windows.Forms.WebBrowser adWebBrowser;
    }
}