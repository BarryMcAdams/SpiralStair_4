namespace SpiralStair_4
{
    partial class SpiralStairForm
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
            this.lblCenterPole = new System.Windows.Forms.Label();
            this.comboCenterPole = new System.Windows.Forms.ComboBox();
            this.txtCustomPoleDiameter = new System.Windows.Forms.TextBox();
            this.lblOverallHeight = new System.Windows.Forms.Label();
            this.txtOverallHeight = new System.Windows.Forms.TextBox();
            this.lblOutsideDiameter = new System.Windows.Forms.Label();
            this.txtOutsideDiameter = new System.Windows.Forms.TextBox();
            this.lblTotalRotation = new System.Windows.Forms.Label();
            this.txtTotalRotation = new System.Windows.Forms.TextBox();
            this.lblHandedness = new System.Windows.Forms.Label();
            this.radioClockwise = new System.Windows.Forms.RadioButton();
            this.radioCounterClockwise = new System.Windows.Forms.RadioButton();
            this.lblUnitsNotice = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblCenterPole
            // 
            this.lblCenterPole.AutoSize = true;
            this.lblCenterPole.Location = new System.Drawing.Point(12, 15);
            this.lblCenterPole.Name = "lblCenterPole";
            this.lblCenterPole.Size = new System.Drawing.Size(108, 13);
            this.lblCenterPole.TabIndex = 0;
            this.lblCenterPole.Text = "Center Pole Diameter:";
            // 
            // comboCenterPole
            // 
            this.comboCenterPole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCenterPole.FormattingEnabled = true;
            this.comboCenterPole.Location = new System.Drawing.Point(150, 12);
            this.comboCenterPole.Name = "comboCenterPole";
            this.comboCenterPole.Size = new System.Drawing.Size(121, 21);
            this.comboCenterPole.TabIndex = 1;
            // 
            // txtCustomPoleDiameter
            // 
            this.txtCustomPoleDiameter.Location = new System.Drawing.Point(277, 12);
            this.txtCustomPoleDiameter.Name = "txtCustomPoleDiameter";
            this.txtCustomPoleDiameter.Size = new System.Drawing.Size(75, 20);
            this.txtCustomPoleDiameter.TabIndex = 2;
            this.txtCustomPoleDiameter.Visible = false; // Initially hidden
            // 
            // lblOverallHeight
            // 
            this.lblOverallHeight.AutoSize = true;
            this.lblOverallHeight.Location = new System.Drawing.Point(12, 42);
            this.lblOverallHeight.Name = "lblOverallHeight";
            this.lblOverallHeight.Size = new System.Drawing.Size(121, 13);
            this.lblOverallHeight.TabIndex = 3;
            this.lblOverallHeight.Text = "Overall Height (FF to FF):";
            // 
            // txtOverallHeight
            // 
            this.txtOverallHeight.Location = new System.Drawing.Point(150, 39);
            this.txtOverallHeight.Name = "txtOverallHeight";
            this.txtOverallHeight.Size = new System.Drawing.Size(121, 20);
            this.txtOverallHeight.TabIndex = 4;
            // 
            // lblOutsideDiameter
            // 
            this.lblOutsideDiameter.AutoSize = true;
            this.lblOutsideDiameter.Location = new System.Drawing.Point(12, 69);
            this.lblOutsideDiameter.Name = "lblOutsideDiameter";
            this.lblOutsideDiameter.Size = new System.Drawing.Size(91, 13);
            this.lblOutsideDiameter.TabIndex = 5;
            this.lblOutsideDiameter.Text = "Outside Diameter:";
            // 
            // txtOutsideDiameter
            // 
            this.txtOutsideDiameter.Location = new System.Drawing.Point(150, 66);
            this.txtOutsideDiameter.Name = "txtOutsideDiameter";
            this.txtOutsideDiameter.Size = new System.Drawing.Size(121, 20);
            this.txtOutsideDiameter.TabIndex = 6;
            // 
            // lblTotalRotation
            // 
            this.lblTotalRotation.AutoSize = true;
            this.lblTotalRotation.Location = new System.Drawing.Point(12, 96);
            this.lblTotalRotation.Name = "lblTotalRotation";
            this.lblTotalRotation.Size = new System.Drawing.Size(118, 13);
            this.lblTotalRotation.TabIndex = 7;
            this.lblTotalRotation.Text = "Total Rotation (Degrees):";
            // 
            // txtTotalRotation
            // 
            this.txtTotalRotation.Location = new System.Drawing.Point(150, 93);
            this.txtTotalRotation.Name = "txtTotalRotation";
            this.txtTotalRotation.Size = new System.Drawing.Size(121, 20);
            this.txtTotalRotation.TabIndex = 8;
            // 
            // lblHandedness
            // 
            this.lblHandedness.AutoSize = true;
            this.lblHandedness.Location = new System.Drawing.Point(12, 123);
            this.lblHandedness.Name = "lblHandedness";
            this.lblHandedness.Size = new System.Drawing.Size(68, 13);
            this.lblHandedness.TabIndex = 9;
            this.lblHandedness.Text = "Handedness:";
            // 
            // radioClockwise
            // 
            this.radioClockwise.AutoSize = true;
            this.radioClockwise.Checked = true;
            this.radioClockwise.Location = new System.Drawing.Point(150, 121);
            this.radioClockwise.Name = "radioClockwise";
            this.radioClockwise.Size = new System.Drawing.Size(148, 17);
            this.radioClockwise.TabIndex = 10;
            this.radioClockwise.TabStop = true;
            this.radioClockwise.Text = "Clockwise / Left-Hand-Up";
            this.radioClockwise.UseVisualStyleBackColor = true;
            // 
            // radioCounterClockwise
            // 
            this.radioCounterClockwise.AutoSize = true;
            this.radioCounterClockwise.Location = new System.Drawing.Point(150, 144);
            this.radioCounterClockwise.Name = "radioCounterClockwise";
            this.radioCounterClockwise.Size = new System.Drawing.Size(181, 17);
            this.radioCounterClockwise.TabIndex = 11;
            this.radioCounterClockwise.Text = "Counter-Clockwise / Right-Hand-Up";
            this.radioCounterClockwise.UseVisualStyleBackColor = true;
            // 
            // lblUnitsNotice
            // 
            this.lblUnitsNotice.AutoSize = true;
            this.lblUnitsNotice.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblUnitsNotice.Location = new System.Drawing.Point(12, 175);
            this.lblUnitsNotice.Name = "lblUnitsNotice";
            this.lblUnitsNotice.Size = new System.Drawing.Size(110, 13);
            this.lblUnitsNotice.TabIndex = 12;
            this.lblUnitsNotice.Text = "Units: Decimal Inches";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(196, 200);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 13;
            this.btnGenerate.Text = "Generate Stair";
            this.btnGenerate.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(277, 200);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SpiralStairForm
            // 
            this.AcceptButton = this.btnGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(364, 235);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.lblUnitsNotice);
            this.Controls.Add(this.radioCounterClockwise);
            this.Controls.Add(this.radioClockwise);
            this.Controls.Add(this.lblHandedness);
            this.Controls.Add(this.txtTotalRotation);
            this.Controls.Add(this.lblTotalRotation);
            this.Controls.Add(this.txtOutsideDiameter);
            this.Controls.Add(this.lblOutsideDiameter);
            this.Controls.Add(this.txtOverallHeight);
            this.Controls.Add(this.lblOverallHeight);
            this.Controls.Add(this.txtCustomPoleDiameter);
            this.Controls.Add(this.comboCenterPole);
            this.Controls.Add(this.lblCenterPole);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpiralStairForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Spiral Stair Generator Input";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCenterPole;
        private System.Windows.Forms.ComboBox comboCenterPole;
        private System.Windows.Forms.TextBox txtCustomPoleDiameter;
        private System.Windows.Forms.Label lblOverallHeight;
        private System.Windows.Forms.TextBox txtOverallHeight;
        private System.Windows.Forms.Label lblOutsideDiameter;
        private System.Windows.Forms.TextBox txtOutsideDiameter;
        private System.Windows.Forms.Label lblTotalRotation;
        private System.Windows.Forms.TextBox txtTotalRotation;
        private System.Windows.Forms.Label lblHandedness;
        private System.Windows.Forms.RadioButton radioClockwise;
        private System.Windows.Forms.RadioButton radioCounterClockwise;
        private System.Windows.Forms.Label lblUnitsNotice;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnCancel;
    }
}