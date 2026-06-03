using System;
using System.Windows.Forms;
using NAudio.Wave;
using System.IO;

namespace AudioProject


{
    public partial class Form1 : Form
    {
        private string audioFile;
        private AudioFileReader audioFileReader;
        private WaveOutEvent outputDevice;
        public Form1()
        {
            InitializeComponent();
        }

        //Bring Audio From File
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Audio Files|*.wav;*.mp3";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                audioFile = ofd.FileName;
                audioFileReader = new AudioFileReader(audioFile);
            }
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);

            if (data != null)
            {

                var filename = data as string[];

                if (filename.Length > 0)
                {

                    audioFile = filename[0];
                    audioFileReader = new AudioFileReader(audioFile);
                }
            }
        }


        //Play Audio
        private void button2_Click(object sender, EventArgs e)
        {
            if (audioFileReader == null) return;

            outputDevice = new WaveOutEvent();
            audioFileReader.Position = 0;
            outputDevice.Init(audioFileReader);
            outputDevice.Play();
        }
        //Audio Properties
        private void button5_Click(object sender, EventArgs e)
        {

        }

      

       
       

      
    }
}
