using System;
using System.Windows.Forms;

namespace SDownload.Dialogs
{
    /// <summary>
    /// Template dialog that presents a message to the user and has 1-2 possible responses.
    /// Dialogs can be set to be skipped next time by using the CheckBox callback for a Setting
    /// </summary>
    public partial class YesNoDialog : Form
    {
        /// <summary>
        /// Callback setter for when a response has been chosen
        /// </summary>
        public Action<bool> ResponseCallback
        {
            set
            {
                yesButton.Click += (sender, args) => value(true);
                noButton.Click += (sender, args) => value(false);
            }
        }

        /// <summary>
        /// Callback setter for the ability to skip this dialog the next time
        /// (By using a value from Settings)
        /// </summary>
        public Action<bool> CheckBoxSettingCallback
        {
            set
            {
                yesButton.Click += (sender, args) => value(askAgainCheckBox.Checked);
                noButton.Click += (sender, args) => value(askAgainCheckBox.Checked);
            }
        }

        /// <summary>
        /// Constructs a dialog based on the Yes/No template using the provided information
        /// </summary>
        /// <param name="question">The main description for the dialog</param>
        /// <param name="yesLabel">Positive response label, null to disable</param>
        /// <param name="noLabel">Negative response label, null to disable</param>
        /// <param name="state">State of the skip dialog checkbox. Possible status: Hidden/NotChecked/Checked</param>
        public YesNoDialog(String question, String yesLabel, String noLabel, CheckBoxState state = CheckBoxState.Hidden)
        {
            InitializeComponent();

            questionLabel.Text = question;

            // Only show buttons if a label was passed
            if (yesLabel != null)
                yesButton.Text = yesLabel;
            else
                yesButton.Visible = false;

            if (noLabel != null)
                noButton.Text = noLabel;
            else
                noButton.Visible = false;

            // Close dialog when a response has been chosen
            yesButton.Click += (sender, args) => Close();
            noButton.Click += (sender, args) => Close();

            // Load the checkbox state
            switch (state)
            {
                case CheckBoxState.Hidden:
                    askAgainCheckBox.Visible = false;
                    break;
                case CheckBoxState.Checked:
                    askAgainCheckBox.Visible = true;
                    askAgainCheckBox.Checked = true;
                    break;
                case CheckBoxState.NotChecked:
                    askAgainCheckBox.Checked = false;
                    askAgainCheckBox.Visible = true;
                    break;
            }
        }
    }

    /// <summary>
    /// Possible states for the skip dialog checkbox
    /// </summary>
    public enum CheckBoxState
    {
        /// <summary>
        /// Hide the checkbox
        /// </summary>
        Hidden,
        /// <summary>
        /// Checkbox should be shown, but not checked
        /// </summary>
        NotChecked,
        /// <summary>
        /// Checkbox shown and checked
        /// </summary>
        Checked
    }
}
