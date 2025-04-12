using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SpiralStair_4
{
    /// <summary>
    /// Form to display validation issues, suggestions, and handle midlanding selection.
    /// </summary>
    public partial class ViolationPromptForm : Form
    {
        public string UserAction { get; private set; } = "Cancel"; // Default action
        public int? SelectedMidlandingIndex { get; private set; } = null; // 0-based index

        /// <summary>
        /// Constructor for the Violation Prompt Form.
        /// </summary>
        /// <param name="violations">List of violation messages.</param>
        /// <param name="suggestions">String containing suggested fixes.</param>
        /// <param name="requiresMidlanding">Flag indicating if midlanding is mandatory.</param>
        /// <param name="numberOfTreads">Total number of treads (used for midlanding position selection).</param>
        public ViolationPromptForm(List<string> violations, string suggestions, bool requiresMidlanding, int numberOfTreads)
        {
            InitializeComponent();

            // Populate violation details
            txtViolationDetails.Text = string.Join(Environment.NewLine, violations ?? new List<string>());

            // Populate suggestions
            lblSuggestion.Text = string.IsNullOrWhiteSpace(suggestions) ? "No specific suggestions available." : suggestions;

            // Handle Midlanding Requirement
            if (requiresMidlanding)
            {
                groupMidlanding.Visible = true;
                lblViolationTitle.Text = "Midlanding Required & Input Violations"; // Update title

                // Populate Midlanding Position ComboBox (1-based index for user)
                comboMidlandingPosition.Items.Clear();
                if (numberOfTreads > 0)
                {
                    for (int i = 1; i <= numberOfTreads; i++) // User sees 1 to N
                    {
                        comboMidlandingPosition.Items.Add(i.ToString());
                    }
                    // Select the middle tread as a default suggestion, if possible
                    int defaultIndex = Math.Max(0, Math.Min(comboMidlandingPosition.Items.Count - 1, (numberOfTreads / 2)));
                    comboMidlandingPosition.SelectedIndex = defaultIndex;
                }
                else
                {
                    // Should not happen if midlanding is required, but handle defensively
                    comboMidlandingPosition.Enabled = false;
                    lblMidlandingPrompt.Text = "Midlanding required, but no treads exist to replace.";
                    btnProceedAnyway.Enabled = false; // Cannot proceed without a valid position
                }
            }
            else
            {
                groupMidlanding.Visible = false;
                lblViolationTitle.Text = "Input Violations / Warnings"; // Default title
            }

            // Wire up event handlers
            this.btnProceedAnyway.Click += OnButtonProceedAnyway_Click;
            this.btnGoBack.Click += OnButtonGoBack_Click;
            this.btnCancelGeneration.Click += OnButtonCancel_Click; // Renamed button in designer likely
        }

        private void OnButtonProceedAnyway_Click(object sender, EventArgs e)
        {
            if (groupMidlanding.Visible) // Check if midlanding selection is active
            {
                if (comboMidlandingPosition.SelectedItem == null)
                {
                    ShowError("Please select a tread position for the midlanding.");
                    comboMidlandingPosition.Focus();
                    return;
                }
                if (TryParseInt(comboMidlandingPosition.SelectedItem.ToString(), out int selectedIndexOneBased))
                {
                    SelectedMidlandingIndex = selectedIndexOneBased - 1; // Store 0-based index
                }
                else
                {
                    // Should not happen with populated list, but handle defensively
                    ShowError("Invalid midlanding position selected.");
                    comboMidlandingPosition.Focus();
                    return;
                }
            }
            UserAction = "Proceed";
            this.DialogResult = DialogResult.OK; // Signal successful completion of this form's interaction
            this.Close();
        }

        private void OnButtonGoBack_Click(object sender, EventArgs e)
        {
            UserAction = "GoBack";
            this.DialogResult = DialogResult.Retry; // Use Retry to indicate going back
            this.Close();
        }

        private void OnButtonCancel_Click(object sender, EventArgs e)
        {
            UserAction = "Cancel";
            this.DialogResult = DialogResult.Cancel; // Standard cancel
            this.Close();
        }

        // --- Helper Methods ---

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private bool TryParseInt(string textValue, out int result)
        {
            return int.TryParse(textValue, out result);
        }

        // Note: InitializeComponent() is called in the constructor.
        // Controls are defined in ViolationPromptForm.Designer.cs
    }
}