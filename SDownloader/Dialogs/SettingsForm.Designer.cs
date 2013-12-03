using System;

namespace SDownload.Dialogs
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.selectDownloadFolderBtn = new System.Windows.Forms.Button();
            this.downloadFolderBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.iTunesEnabled = new System.Windows.Forms.CheckBox();
            this.iTunesCopy = new System.Windows.Forms.CheckBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.authorFolderSort = new System.Windows.Forms.CheckBox();
            this.useDownloadLink = new System.Windows.Forms.CheckBox();
            this.confirmExitCheckBox = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkForUpdates = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // selectDownloadFolderBtn
            // 
            this.selectDownloadFolderBtn.Location = new System.Drawing.Point(448, 145);
            this.selectDownloadFolderBtn.Name = "selectDownloadFolderBtn";
            this.selectDownloadFolderBtn.Size = new System.Drawing.Size(34, 23);
            this.selectDownloadFolderBtn.TabIndex = 0;
            this.selectDownloadFolderBtn.Text = "...";
            this.selectDownloadFolderBtn.UseVisualStyleBackColor = true;
            // 
            // downloadFolderBox
            // 
            this.downloadFolderBox.Location = new System.Drawing.Point(11, 147);
            this.downloadFolderBox.Name = "downloadFolderBox";
            this.downloadFolderBox.Size = new System.Drawing.Size(431, 20);
            this.downloadFolderBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 128);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Download Folder";
            // 
            // iTunesEnabled
            // 
            this.iTunesEnabled.AutoSize = true;
            this.iTunesEnabled.Checked = true;
            this.iTunesEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.iTunesEnabled.Location = new System.Drawing.Point(11, 197);
            this.iTunesEnabled.Name = "iTunesEnabled";
            this.iTunesEnabled.Size = new System.Drawing.Size(92, 17);
            this.iTunesEnabled.TabIndex = 3;
            this.iTunesEnabled.Text = "Add to iTunes";
            this.iTunesEnabled.UseVisualStyleBackColor = true;
            // 
            // iTunesCopy
            // 
            this.iTunesCopy.AutoSize = true;
            this.iTunesCopy.Location = new System.Drawing.Point(29, 220);
            this.iTunesCopy.Name = "iTunesCopy";
            this.iTunesCopy.Size = new System.Drawing.Size(166, 17);
            this.iTunesCopy.TabIndex = 4;
            this.iTunesCopy.Text = "Keep song in download folder";
            this.iTunesCopy.UseVisualStyleBackColor = true;
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(408, 255);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 5;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            // 
            // authorFolderSort
            // 
            this.authorFolderSort.AutoSize = true;
            this.authorFolderSort.Location = new System.Drawing.Point(11, 174);
            this.authorFolderSort.Name = "authorFolderSort";
            this.authorFolderSort.Size = new System.Drawing.Size(116, 17);
            this.authorFolderSort.TabIndex = 6;
            this.authorFolderSort.Text = "Sort songs by Artist";
            this.authorFolderSort.UseVisualStyleBackColor = true;
            // 
            // useDownloadLink
            // 
            this.useDownloadLink.AutoSize = true;
            this.useDownloadLink.Location = new System.Drawing.Point(11, 243);
            this.useDownloadLink.Name = "useDownloadLink";
            this.useDownloadLink.Size = new System.Drawing.Size(201, 17);
            this.useDownloadLink.TabIndex = 7;
            this.useDownloadLink.Text = "Use the download link when possible";
            this.useDownloadLink.UseVisualStyleBackColor = true;
            // 
            // confirmExitCheckBox
            // 
            this.confirmExitCheckBox.AutoSize = true;
            this.confirmExitCheckBox.Location = new System.Drawing.Point(265, 174);
            this.confirmExitCheckBox.Name = "confirmExitCheckBox";
            this.confirmExitCheckBox.Size = new System.Drawing.Size(181, 17);
            this.confirmExitCheckBox.TabIndex = 8;
            this.confirmExitCheckBox.Text = "Confirm before exiting application";
            this.confirmExitCheckBox.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SDownload.Properties.Resources.logo_with_text;
            this.pictureBox1.Location = new System.Drawing.Point(15, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(468, 92);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // checkForUpdates
            // 
            this.checkForUpdates.AutoSize = true;
            this.checkForUpdates.Location = new System.Drawing.Point(265, 197);
            this.checkForUpdates.Name = "checkForUpdates";
            this.checkForUpdates.Size = new System.Drawing.Size(177, 17);
            this.checkForUpdates.TabIndex = 10;
            this.checkForUpdates.Text = "Automatically check for updates";
            this.checkForUpdates.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 287);
            this.Controls.Add(this.checkForUpdates);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.confirmExitCheckBox);
            this.Controls.Add(this.useDownloadLink);
            this.Controls.Add(this.authorFolderSort);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.iTunesCopy);
            this.Controls.Add(this.iTunesEnabled);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.downloadFolderBox);
            this.Controls.Add(this.selectDownloadFolderBtn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "SDownload Settings";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button selectDownloadFolderBtn;
        private System.Windows.Forms.TextBox downloadFolderBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox iTunesEnabled;
        private System.Windows.Forms.CheckBox iTunesCopy;
        private System.Windows.Forms.Button saveBtn;
        partial void SettingsFormLoad(object sender, EventArgs e);

        private System.Windows.Forms.CheckBox authorFolderSort;
        private System.Windows.Forms.CheckBox useDownloadLink;
        private System.Windows.Forms.CheckBox confirmExitCheckBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox checkForUpdates;
    }
}