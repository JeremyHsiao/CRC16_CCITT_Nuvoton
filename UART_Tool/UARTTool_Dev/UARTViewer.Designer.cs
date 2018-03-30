namespace UARTViewer
{
    partial class UARTViewer
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            MySerialPort.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UARTViewer));
            this.rtbSignalData = new System.Windows.Forms.RichTextBox();
            this.btnRefreshCOMNo = new System.Windows.Forms.Button();
            this.lstMyComPort = new System.Windows.Forms.ListBox();
            this.btnConnectionControl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbSignalData
            // 
            this.rtbSignalData.Location = new System.Drawing.Point(6, 64);
            this.rtbSignalData.Name = "rtbSignalData";
            this.rtbSignalData.ReadOnly = true;
            this.rtbSignalData.Size = new System.Drawing.Size(610, 421);
            this.rtbSignalData.TabIndex = 7;
            this.rtbSignalData.Text = "";
            // 
            // btnRefreshCOMNo
            // 
            this.btnRefreshCOMNo.Location = new System.Drawing.Point(7, 12);
            this.btnRefreshCOMNo.Name = "btnRefreshCOMNo";
            this.btnRefreshCOMNo.Size = new System.Drawing.Size(49, 46);
            this.btnRefreshCOMNo.TabIndex = 16;
            this.btnRefreshCOMNo.Text = "Refresh COM";
            this.btnRefreshCOMNo.UseVisualStyleBackColor = true;
            this.btnRefreshCOMNo.Click += new System.EventHandler(this.btnFreshCOMNo_Click);
            // 
            // lstMyComPort
            // 
            this.lstMyComPort.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstMyComPort.FormattingEnabled = true;
            this.lstMyComPort.ItemHeight = 14;
            this.lstMyComPort.Location = new System.Drawing.Point(62, 12);
            this.lstMyComPort.Name = "lstMyComPort";
            this.lstMyComPort.Size = new System.Drawing.Size(58, 46);
            this.lstMyComPort.TabIndex = 17;
            // 
            // btnConnectionControl
            // 
            this.btnConnectionControl.Enabled = false;
            this.btnConnectionControl.Location = new System.Drawing.Point(126, 12);
            this.btnConnectionControl.Name = "btnConnectionControl";
            this.btnConnectionControl.Size = new System.Drawing.Size(68, 46);
            this.btnConnectionControl.TabIndex = 18;
            this.btnConnectionControl.Text = "Connect UART";
            this.btnConnectionControl.UseVisualStyleBackColor = true;
            this.btnConnectionControl.Click += new System.EventHandler(this.btnConnectionControl_Click);
            // 
            // UARTViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 488);
            this.Controls.Add(this.btnConnectionControl);
            this.Controls.Add(this.lstMyComPort);
            this.Controls.Add(this.btnRefreshCOMNo);
            this.Controls.Add(this.rtbSignalData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UARTViewer";
            this.Text = "BlueRat Development Viewer";
            this.Load += new System.EventHandler(this.MyUARTViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox rtbSignalData;
        private System.Windows.Forms.Button btnRefreshCOMNo;
        private System.Windows.Forms.ListBox lstMyComPort;
        private System.Windows.Forms.Button btnConnectionControl;
    }
}

