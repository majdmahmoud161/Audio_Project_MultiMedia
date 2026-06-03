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
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(244, 102);
            button1.TabIndex = 0;
            button1.Text = "Load Audio File";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(12, 129);
            button2.Name = "button2";
            button2.Size = new Size(244, 128);
            button2.TabIndex = 1;
            button2.Text = "PlayAudio";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button5
            // 
            button5.Location = new Point(12, 281);
            button5.Name = "button5";
            button5.Size = new Size(244, 104);
            button5.TabIndex = 5;
            button5.Text = "ShowAudioProperties";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1323, 748);
            Controls.Add(button5);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            DragDrop += Form1_DragDrop;
            DragEnter += Form1_DragEnter;
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button5;
    }
}
