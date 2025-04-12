using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

// Assuming AutoCAD references are available if needed directly, though unlikely for the form itself.
// using Autodesk.AutoCAD.ApplicationServices;
// using Autodesk.AutoCAD.EditorInput;

namespace SpiralStair_4
{
    /// <summary>
    /// Defines the UI for gathering spiral stair parameters.
    /// </summary>
    public partial class SpiralStairForm : Form
    {
        private StairData internalStairData;

        public SpiralStairForm()
        {
            InitializeComponent();
            // Add event handlers defined in the designer or manually here
            this.Load += OnFormLoad;
            this.comboCenterPole.SelectedIndexChanged += OnComboCenterPole_SelectionChanged;
            this.btnGenerate.Click += OnButtonGenerate_Click;
            this.btnCancel.Click += OnButtonCancel_Click;
            // Disable handedness for now as per pseudocode
            this.radioClockwise.Enabled = false;
            this.radioCounterClockwise.Enabled = false;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            PopulateCenterPoleComboBox();
            // Set default selections if needed (e.g., first standard size)
            if (comboCenterPole.Items.Count > 1) // Ensure "Custom" isn't the only item
            {
                comboCenterPole.SelectedIndex = 0;
            }
            txtCustomPoleDiameter.Visible = false; // Initially hide custom input
        }

        private void PopulateCenterPoleComboBox()
        {
            comboCenterPole.Items.Clear();
            foreach (var size in StairData.StandardPoleSizes)
            {
                comboCenterPole.Items.Add(size.ToString(CultureInfo.InvariantCulture));
            }
            comboCenterPole.Items.Add("Custom");
        }

        private void OnComboCenterPole_SelectionChanged(object sender, EventArgs e)
        {
            if (comboCenterPole.SelectedItem?.ToString() == "Custom")
            {
                txtCustomPoleDiameter.Visible = true;
                txtCustomPoleDiameter.Focus();
            }
            else
            {
                txtCustomPoleDiameter.Visible = false;
                txtCustomPoleDiameter.Text = "";
            }
        }

        private void OnButtonGenerate_Click(object sender, EventArgs e)
        {
            internalStairData = new StairData();

            try
            {
                // Gather Center Pole Diameter
                if (comboCenterPole.SelectedItem?.ToString() == "Custom")
                {
                    if (!TryParseDouble(txtCustomPoleDiameter.Text, out double customPoleDia) || customPoleDia <= 0)
                    {
                        ShowError("Custom Center Pole Diameter must be a positive number.");
                        txtCustomPoleDiameter.Focus();
                        return;
                    }
                    internalStairData.CenterPoleDiameter = customPoleDia;
                }
                else if (comboCenterPole.SelectedItem != null)
                {
                    if (!TryParseDouble(comboCenterPole.SelectedItem.ToString(), out double standardPoleDia) || standardPoleDia <= 0)
                    {
                        // This shouldn't happen with standard sizes, but good practice
                        ShowError("Invalid selection for Standard Center Pole Diameter.");
                        comboCenterPole.Focus();
                        return;
                    }
                    internalStairData.CenterPoleDiameter = standardPoleDia;
                }
                else
                {
                    ShowError("Please select or enter a Center Pole Diameter.");
                    comboCenterPole.Focus();
                    return;
                }

                // Gather Other Inputs
                if (!TryParseDouble(txtOverallHeight.Text, out double overallHeight) || overallHeight <= 0)
                {
                    ShowError("Overall Height must be a positive number.");
                    txtOverallHeight.Focus();
                    return;
                }
                internalStairData.OverallHeight = overallHeight;

                if (!TryParseDouble(txtOutsideDiameter.Text, out double outsideDiameter) || outsideDiameter <= 0)
                {
                    ShowError("Outside Diameter must be a positive number.");
                    txtOutsideDiameter.Focus();
                    return;
                }
                internalStairData.OutsideDiameter = outsideDiameter;

                if (!TryParseDouble(txtTotalRotation.Text, out double totalRotation) || totalRotation <= 0)
                {
                    ShowError("Total Rotation must be a positive number.");
                    txtTotalRotation.Focus();
                    return;
                }
                internalStairData.TotalRotation = totalRotation;

                // Store Handedness (though not used in generation yet)
                internalStairData.Handedness = radioClockwise.Checked ? "Clockwise" : "CounterClockwise";

                // Basic Sanity Checks
                if (internalStairData.OutsideDiameter <= internalStairData.CenterPoleDiameter)
                {
                    ShowError("Outside Diameter must be greater than Center Pole Diameter.");
                    txtOutsideDiameter.Focus();
                    return;
                }

                // If all validation passes
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (FormatException ex)
            {
                // Catch potential errors from TryParseDouble if it threw (though it shouldn't with out param)
                ShowError($"Invalid numeric input detected: {ex.Message}");
            }
            catch (Exception ex) // Catch unexpected errors
            {
                ShowError($"An unexpected error occurred during input processing: {ex.Message}");
            }
        }

        private void OnButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Gets the populated StairData object after the form is closed with DialogResult.OK.
        /// </summary>
        /// <returns>The StairData object, or null if the form was cancelled or data wasn't generated.</returns>
        public StairData GetStairData()
        {
            return internalStairData;
        }

        // --- Helper Methods ---

        /// <summary>
        /// Safely parses a string to a double.
        /// </summary>
        private bool TryParseDouble(string textValue, out double result)
        {
            return double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Displays a standard error message box.
        /// </summary>
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Note: InitializeComponent() is called in the constructor.
        // The actual controls (TextBoxes, Labels, etc.) are defined in SpiralStairForm.Designer.cs
    }
}