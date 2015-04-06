namespace ColorFilter
{
    partial class Histogram
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.histogramBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.histogramBox)).BeginInit();
            this.SuspendLayout();
            // 
            // histogramBox
            // 
            this.histogramBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.histogramBox.Location = new System.Drawing.Point(31, 49);
            this.histogramBox.Name = "histogramBox";
            this.histogramBox.Size = new System.Drawing.Size(275, 181);
            this.histogramBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.histogramBox.TabIndex = 0;
            this.histogramBox.TabStop = false;
            // 
            // Histogram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 280);
            this.Controls.Add(this.histogramBox);
            this.Name = "Histogram";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Histogram_Paint);
            this.Resize += new System.EventHandler(this.Histogram_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.histogramBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox histogramBox;

    }
}