namespace SpiralStair_4
{
    partial class ViolationPromptForm
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
            this.lblViolationTitle = new System.Windows.Forms.Label();
            this.txtViolationDetails = new System.Windows.Forms.TextBox();
            this.lblSuggestion = new System.Windows.Forms.Label();
            this.btnProceedAnyway = new System.Windows.Forms.Button();
            this.btnGoBack = new System.Windows.Forms.Button();
            this.btnCancelGeneration = new System.Windows.Forms.Button();
            this.groupMidlanding = new System.Windows.Forms.GroupBox();
            this.comboMidlandingPosition = new System.Windows.Forms.ComboBox();
            this.lblMidlandingPrompt = new System.Windows.Forms.Label();
            this.groupMidlanding.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblViolationTitle
            // 
            this.lblViolationTitle.AutoSize = true;
            this.lblViolationTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblViolationTitle.Location = new System.Drawing.Point(12, 9);
            this.lblViolationTitle.Name = "lblViolationTitle";
            this.lblViolationTitle.Size = new System.Drawing.Size(215, 13);
            this.lblViolationTitle.TabIndex = 0;
            this.lblViolationTitle.Text = "Input Violations / Midlanding Required";
            // 
            // txtViolationDetails
            // 
            this.txtViolationDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtViolationDetails.Location = new System.Drawing.Point(15, 25);
            this.txtViolationDetails.Multiline = true;
            this.txtViolationDetails.Name = "txtViolationDetails";
            this.txtViolationDetails.ReadOnly = true;
            this.txtViolationDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtViolationDetails.Size = new System.Drawing.Size(437, 100);
            this.txtViolationDetails.TabIndex = 1;
            // 
            // lblSuggestion
            // 
            this.lblSuggestion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSuggestion.Location = new System.Drawing.Point(12, 132);
            this.lblSuggestion.Name = "lblSuggestion";
            this.lblSuggestion.Size = new System.Drawing.Size(440, 45);
            this.lblSuggestion.TabIndex = 2;
            this.lblSuggestion.Text = "[Suggestions will appear here]";
            // 
            // btnProceedAnyway
            // 
            this.btnProceedAnyway.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProceedAnyway.Location = new System.Drawing.Point(190, 266);
            this.btnProceedAnyway.Name = "btnProceedAnyway";
            this.btnProceedAnyway.Size = new System.Drawing.Size(100, 23);
            this.btnProceedAnyway.TabIndex = 5; // Adjusted tab index
            this.btnProceedAnyway.Text = "Proceed Anyway";
            this.btnProceedAnyway.UseVisualStyleBackColor = true;
            // 
            // btnGoBack
            // 
            this.btnGoBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoBack.Location = new System.Drawing.Point(296, 266);
            this.btnGoBack.Name = "btnGoBack";
            this.btnGoBack.Size = new System.Drawing.Size(75, 23);
            this.btnGoBack.TabIndex = 6; // Adjusted tab index
            this.btnGoBack.Text = "Go Back";
            this.btnGoBack.UseVisualStyleBackColor = true;
            // 
            // btnCancelGeneration
            // 
            this.btnCancelGeneration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelGeneration.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelGeneration.Location = new System.Drawing.Point(377, 266);
            this.btnCancelGeneration.Name = "btnCancelGeneration";
            this.btnCancelGeneration.Size = new System.Drawing.Size(75, 23);
            this.btnCancelGeneration.TabIndex = 7; // Adjusted tab index
            this.btnCancelGeneration.Text = "Cancel";
            this.btnCancelGeneration.UseVisualStyleBackColor = true;
            // 
            // groupMidlanding
            // 
            this.groupMidlanding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupMidlanding.Controls.Add(this.comboMidlandingPosition);
            this.groupMidlanding.Controls.Add(this.lblMidlandingPrompt);
            this.groupMidlanding.Location = new System.Drawing.Point(15, 180);
            this.groupMidlanding.Name = "groupMidlanding";
            this.groupMidlanding.Size = new System.Drawing.Size(437, 70);
            this.groupMidlanding.TabIndex = 3; // Adjusted tab index
            this.groupMidlanding.TabStop = false;
            this.groupMidlanding.Text = "Midlanding Selection";
            this.groupMidlanding.Visible = false; // Initially hidden
            // 
            // comboMidlandingPosition
            // 
            this.comboMidlandingPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMidlandingPosition.FormattingEnabled = true;
            this.comboMidlandingPosition.Location = new System.Drawing.Point(9, 37);
            this.comboMidlandingPosition.Name = "comboMidlandingPosition";
            this.comboMidlandingPosition.Size = new System.Drawing.Size(121, 21);
            this.comboMidlandingPosition.TabIndex = 4; // Adjusted tab index
            // 
            // lblMidlandingPrompt
            // 
            this.lblMidlandingPrompt.AutoSize = true;
            this.lblMidlandingPrompt.Location = new System.Drawing.Point(6, 21);
            this.lblMidlandingPrompt.Name = "lblMidlandingPrompt";
            this.lblMidlandingPrompt.Size = new System.Drawing.Size(240, 13);
            this.lblMidlandingPrompt.TabIndex = 0;
            this.lblMidlandingPrompt.Text = "Midlanding required. Select position (replaces tread #):";
            // 
            // ViolationPromptForm
            // 
            this.AcceptButton = this.btnProceedAnyway; // Or btnGoBack depending on desired default
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelGeneration;
            this.ClientSize = new System.Drawing.Size(464, 301);
            this.Controls.Add(this.groupMidlanding);
            this.Controls.Add(this.btnCancelGeneration);
            this.Controls.Add(this.btnGoBack);
            this.Controls.Add(this.btnProceedAnyway);
            this.Controls.Add(this.lblSuggestion);
            this.Controls.Add(this.txtViolationDetails);
            this.Controls.Add(this.lblViolationTitle);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 340);
            this.Name = "ViolationPromptForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Validation Issues";
            this.groupMidlanding.ResumeLayout(false);
            this.groupMidlanding.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblViolationTitle;
        private System.Windows.Forms.TextBox txtViolationDetails;
        private System.Windows.Forms.Label lblSuggestion;
        private System.Windows.Forms.Button btnProceedAnyway;
        private System.Windows.Forms.Button btnGoBack;
        private System.Windows.Forms.Button btnCancelGeneration;
        private System.Windows.Forms.GroupBox groupMidlanding;
        private System.Windows.Forms.ComboBox comboMidlandingPosition;
        private System.Windows.Forms.Label lblMidlandingPrompt;
    }
}