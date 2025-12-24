namespace XMMTwrapper1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            axxmmxy1 = new AxXMMTLib.AxXMMXY();
            axxmmWater1 = new AxXMMTLib.AxXMMWater();
            axXMMR1 = new AxXMMTLib.AxXMMR();
            axXMMSpec1 = new AxXMMTLib.AxXMMSpec();
            LStatus = new Label();
            axXMMLevel1 = new AxXMMTLib.AxXMMLvl();
            button1 = new Button();
            btnTestDraw = new Button();
            ((System.ComponentModel.ISupportInitialize)axxmmxy1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)axxmmWater1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)axXMMR1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)axXMMSpec1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)axXMMLevel1).BeginInit();
            SuspendLayout();
            // 
            // axxmmxy1
            // 
            axxmmxy1.Enabled = true;
            axxmmxy1.Location = new Point(638, 176);
            axxmmxy1.Name = "axxmmxy1";
            axxmmxy1.OcxState = (AxHost.State)resources.GetObject("axxmmxy1.OcxState");
            axxmmxy1.Size = new Size(114, 104);
            axxmmxy1.TabIndex = 14;
            axxmmxy1.OnLButtonClick += axxmmxy1_OnLButtonClick;
            // 
            // axxmmWater1
            // 
            axxmmWater1.Enabled = true;
            axxmmWater1.Location = new Point(271, 286);
            axxmmWater1.Name = "axxmmWater1";
            axxmmWater1.OcxState = (AxHost.State)resources.GetObject("axxmmWater1.OcxState");
            axxmmWater1.Size = new Size(353, 92);
            axxmmWater1.TabIndex = 13;
            // 
            // axXMMR1
            // 
            axXMMR1.Enabled = true;
            axXMMR1.Location = new Point(56, 328);
            axXMMR1.Name = "axXMMR1";
            axXMMR1.OcxState = (AxHost.State)resources.GetObject("axXMMR1.OcxState");
            axXMMR1.Size = new Size(100, 50);
            axXMMR1.TabIndex = 11;
            axXMMR1.OnConnected += axXMMR1_OnConnected_1;
            axXMMR1.OnNotifyFFT += axXMMR1_OnNotifyFFT;
            axXMMR1.OnNotifyXY += axXMMR1_OnNotifyXY;
            // 
            // axXMMSpec1
            // 
            axXMMSpec1.Enabled = true;
            axXMMSpec1.Location = new Point(271, 176);
            axXMMSpec1.Name = "axXMMSpec1";
            axXMMSpec1.OcxState = (AxHost.State)resources.GetObject("axXMMSpec1.OcxState");
            axXMMSpec1.Size = new Size(353, 104);
            axXMMSpec1.TabIndex = 10;
            axXMMSpec1.OnLMouseDown += axXMMSpec1_OnLMouseDown_1;
            // 
            // LStatus
            // 
            LStatus.AutoSize = true;
            LStatus.Location = new Point(271, 55);
            LStatus.Name = "LStatus";
            LStatus.Size = new Size(82, 15);
            LStatus.TabIndex = 6;
            LStatus.Text = "MMTTY status";
            // 
            // axXMMLevel1
            // 
            axXMMLevel1.Enabled = true;
            axXMMLevel1.Location = new Point(221, 176);
            axXMMLevel1.Name = "axXMMLevel1";
            axXMMLevel1.OcxState = (AxHost.State)resources.GetObject("axXMMLevel1.OcxState");
            axXMMLevel1.Size = new Size(35, 104);
            axXMMLevel1.TabIndex = 9;
            // 
            // button1
            // 
            button1.Location = new Point(65, 109);
            button1.Name = "button1";
            button1.Size = new Size(120, 23);
            button1.TabIndex = 15;
            button1.Text = "Re-start MMTTY";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnTestDraw
            // 
            btnTestDraw.Location = new Point(65, 55);
            btnTestDraw.Name = "btnTestDraw";
            btnTestDraw.Size = new Size(91, 23);
            btnTestDraw.TabIndex = 16;
            btnTestDraw.Text = "btnTestDraw";
            btnTestDraw.UseVisualStyleBackColor = true;
            btnTestDraw.Click += btnTestDraw_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnTestDraw);
            Controls.Add(button1);
            Controls.Add(axXMMLevel1);
            Controls.Add(LStatus);
            Controls.Add(axXMMSpec1);
            Controls.Add(axXMMR1);
            Controls.Add(axxmmWater1);
            Controls.Add(axxmmxy1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)axxmmxy1).EndInit();
            ((System.ComponentModel.ISupportInitialize)axxmmWater1).EndInit();
            ((System.ComponentModel.ISupportInitialize)axXMMR1).EndInit();
            ((System.ComponentModel.ISupportInitialize)axXMMSpec1).EndInit();
            ((System.ComponentModel.ISupportInitialize)axXMMLevel1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private AxXMMTLib.AxXMMXY axxmmxy1;
        private AxXMMTLib.AxXMMWater axxmmWater1;
        private AxXMMTLib.AxXMMR axXMMR1;
        private AxXMMTLib.AxXMMSpec axXMMSpec1;
        private Label LStatus;
        private AxXMMTLib.AxXMMLvl axXMMLevel1;
        private AxXMMTLib.AxXMMT axxmmt1;
        private Button button1;
        private Button btnTestDraw;
    }
}
