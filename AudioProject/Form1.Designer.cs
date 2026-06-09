namespace AudioProject
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
            button1 = new Button();
            button2 = new Button();
            button5 = new Button();
            button3 = new Button();
            button4 = new Button();
            button6 = new Button();
            button7 = new Button();
            label3 = new Label();
            numStepSize = new NumericUpDown();
            label4 = new Label();
            numMinStep = new NumericUpDown();
            label5 = new Label();
            numMaxStep = new NumericUpDown();
            progressBar1 = new ProgressBar();
            lblProgress = new Label();
            btnCancel = new Button();
            panelGraph = new Panel();
            panelSpeed = new Panel();
            lblSpeed = new Label();
            cmbSampleRate = new ComboBox();
            label1 = new Label();
            button8 = new Button();
            ((System.ComponentModel.ISupportInitialize)numStepSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinStep).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxStep).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(11, 12);
            button1.Name = "button1";
            button1.Size = new Size(245, 101);
            button1.TabIndex = 0;
            button1.Text = "Load Audio File";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(11, 129);
            button2.Name = "button2";
            button2.Size = new Size(245, 128);
            button2.TabIndex = 1;
            button2.Text = "PlayAudio";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button5
            // 
            button5.Location = new Point(11, 281);
            button5.Name = "button5";
            button5.Size = new Size(245, 104);
            button5.TabIndex = 5;
            button5.Text = "ShowAudioProperties";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button3
            // 
            button3.Location = new Point(11, 407);
            button3.Margin = new Padding(3, 4, 3, 4);
            button3.Name = "button3";
            button3.Size = new Size(245, 93);
            button3.TabIndex = 6;
            button3.Text = "Nonlinear Quantization";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(273, 12);
            button4.Margin = new Padding(3, 4, 3, 4);
            button4.Name = "button4";
            button4.Size = new Size(211, 101);
            button4.TabIndex = 7;
            button4.Text = "Delta Modulation";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button6
            // 
            button6.Location = new Point(273, 129);
            button6.Margin = new Padding(3, 4, 3, 4);
            button6.Name = "button6";
            button6.Size = new Size(211, 128);
            button6.TabIndex = 8;
            button6.Text = "Adaptive Delta Modulation";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button7
            // 
            button7.Location = new Point(273, 281);
            button7.Margin = new Padding(3, 4, 3, 4);
            button7.Name = "button7";
            button7.Size = new Size(211, 104);
            button7.TabIndex = 9;
            button7.Text = "Decompression";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(526, 30);
            label3.Name = "label3";
            label3.Size = new Size(70, 20);
            label3.TabIndex = 15;
            label3.Text = "Step Size";
            // 
            // numStepSize
            // 
            numStepSize.Location = new Point(530, 53);
            numStepSize.Name = "numStepSize";
            numStepSize.Size = new Size(66, 27);
            numStepSize.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(626, 30);
            label4.Name = "label4";
            label4.Size = new Size(68, 20);
            label4.TabIndex = 17;
            label4.Text = "Min Step";
            // 
            // numMinStep
            // 
            numMinStep.Location = new Point(615, 53);
            numMinStep.Name = "numMinStep";
            numMinStep.Size = new Size(79, 27);
            numMinStep.TabIndex = 18;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(725, 30);
            label5.Name = "label5";
            label5.Size = new Size(71, 20);
            label5.TabIndex = 19;
            label5.Text = "Max Step";
            // 
            // numMaxStep
            // 
            numMaxStep.Location = new Point(725, 53);
            numMaxStep.Name = "numMaxStep";
            numMaxStep.Size = new Size(91, 27);
            numMaxStep.TabIndex = 20;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(554, 143);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(125, 29);
            progressBar1.TabIndex = 21;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(589, 120);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(33, 20);
            lblProgress.TabIndex = 22;
            lblProgress.Text = "0 %";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(725, 143);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 29);
            btnCancel.TabIndex = 24;
            btnCancel.Text = "إلغاء";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // panelGraph
            // 
            panelGraph.BackColor = SystemColors.ControlText;
            panelGraph.Location = new Point(530, 210);
            panelGraph.Name = "panelGraph";
            panelGraph.Size = new Size(371, 251);
            panelGraph.TabIndex = 25;
            panelGraph.Paint += panelGraph_Paint;
            // 
            // panelSpeed
            // 
            panelSpeed.BackColor = SystemColors.ControlText;
            panelSpeed.Location = new Point(932, 211);
            panelSpeed.Name = "panelSpeed";
            panelSpeed.Size = new Size(384, 251);
            panelSpeed.TabIndex = 26;
            panelSpeed.Paint += panelSpeed_Paint;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(1090, 183);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(51, 20);
            lblSpeed.TabIndex = 27;
            lblSpeed.Text = "Speed";
            // 
            // cmbSampleRate
            // 
            cmbSampleRate.FormattingEnabled = true;
            cmbSampleRate.Items.AddRange(new object[] { "44100", "22050", "11025" });
            cmbSampleRate.Location = new Point(839, 53);
            cmbSampleRate.Name = "cmbSampleRate";
            cmbSampleRate.Size = new Size(89, 28);
            cmbSampleRate.TabIndex = 28;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(839, 30);
            label1.Name = "label1";
            label1.Size = new Size(93, 20);
            label1.TabIndex = 29;
            label1.Text = "Sample Rate";
            // 
            // button8
            // 
            button8.Location = new Point(273, 407);
            button8.Name = "button8";
            button8.Size = new Size(211, 93);
            button8.TabIndex = 30;
            button8.Text = "SaveAudioFile";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1441, 748);
            Controls.Add(button8);
            Controls.Add(label1);
            Controls.Add(cmbSampleRate);
            Controls.Add(lblSpeed);
            Controls.Add(panelSpeed);
            Controls.Add(panelGraph);
            Controls.Add(btnCancel);
            Controls.Add(lblProgress);
            Controls.Add(progressBar1);
            Controls.Add(numMaxStep);
            Controls.Add(label5);
            Controls.Add(numMinStep);
            Controls.Add(label4);
            Controls.Add(numStepSize);
            Controls.Add(label3);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button5);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            DragDrop += Form1_DragDrop;
            DragEnter += Form1_DragEnter;
            ((System.ComponentModel.ISupportInitialize)numStepSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinStep).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxStep).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button5;
        private Button button3;
        private Button button4;
        private Button button6;
        private Button button7;
        private Label label3;
        private NumericUpDown numStepSize;
        private Label label4;
        private NumericUpDown numMinStep;
        private Label label5;
        private NumericUpDown numMaxStep;
        private ProgressBar progressBar1;
        private Label lblProgress;
        private Button btnCancel;
        private Panel panelGraph;
        private Panel panelSpeed;
        private Label lblSpeed;
        private ComboBox cmbSampleRate;
        private Label label1;
        private Button button8;
    }
}
