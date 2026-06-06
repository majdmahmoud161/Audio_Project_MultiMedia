using NAudio.Wave;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

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
            FileInfo fileInfo = new FileInfo(audioFile);


            double fileSizeInMB = (double)fileInfo.Length / (1024 * 1024);

            string encodingType = fileInfo.Extension.ToUpper().Replace(".", "");

            TimeSpan duration = audioFileReader.TotalTime;

            int sampleRate = audioFileReader.WaveFormat.SampleRate;
            int channels = audioFileReader.WaveFormat.Channels;
            int bitrate = audioFileReader.WaveFormat.BitsPerSample;

            string infoMessage = $" Audio Attribute:\n\n" +
                                 $" FileSize: {fileSizeInMB:F2} MB\n" +
                                 $" Duration : {duration.Minutes}:{duration.Seconds:D2} Minutes\n" +
                                 $" Sample Rate: {sampleRate} Hz\n" +
                                 $" Channels: {channels}\n" +
                                 $" Bit Rate: {bitrate} bits\n" +
                                 $" Encoding: {encodingType}";

            MessageBox.Show(infoMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (audioFile == null)
            {
                MessageBox.Show("أولا اختر الملف الصوتي");
                return;
            }

            string outputFile = Path.Combine(
                Path.GetDirectoryName(audioFile),
                Path.GetFileNameWithoutExtension(audioFile) + "_MuLaw.raw");


            using (var reader = new AudioFileReader(audioFile))
            using (var writer = new BinaryWriter(File.Create(outputFile)))
            {
                float[] buffer = new float[1024];
                int samplesRead;

                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < samplesRead; i++)
                    {
                        short pcmSample = (short)(buffer[i] * short.MaxValue);

                        byte compressedSample = NAudio.Codecs.MuLawEncoder.LinearToMuLawSample(pcmSample);

                        writer.Write(compressedSample);
                    }
                }
            }
            long bytesBefore = new FileInfo(audioFile).Length;
            long bytesAfter = new FileInfo(outputFile).Length;

            double kbBefore = bytesBefore / 1024.0;
            double mbBefore = kbBefore / 1024.0;

            double kbAfter = bytesAfter / 1024.0;
            double mbAfter = kbAfter / 1024.0;

            double savedRatio = (1.0 - ((double)bytesAfter / bytesBefore)) * 100;

            string resultMessage = $"مقارنة الحجم قبل وبعد الضغط لملف WAV:\n\n" +
                       $"━━━ [ الحجم قبل الضغط ] ━━━\n" +
                       $"بالكيلوبايت: {kbBefore:F2} KB\n" +
                       $"بالميغابايت: {mbBefore:F2} MB\n\n" +
                       $"━━━ [ الحجم بعد الضغط ] ━━━\n" +
                       $"بالكيلوبايت: {kbAfter:F2} KB\n" +
                       $"بالميغابايت: {mbAfter:F2} MB\n\n" +
                       $"نسبة المساحة الموفرة: {savedRatio:F1}%";

            MessageBox.Show(resultMessage, "Nonlinear Quantization");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (audioFile == null)
            {
                MessageBox.Show("الرجاء اختيار ملف WAV أولاً!");
                return;
            }

            string outputFile = Path.Combine(
                Path.GetDirectoryName(audioFile),
                Path.GetFileNameWithoutExtension(audioFile) + "_Delta.raw");

            int stepSize = 200;      // خطوة معقولة كبداية
            int predictedValue = 0;

            string tableText = "📋 أول 10 عينات:\n";
            tableText += "------------------------------------------------------------\n";
            tableText += "العينة الحقيقية   |  البت  |  التوقع الجديد\n";
            tableText += "------------------------------------------------------------\n";

            int printCounter = 0;

            using (var reader = new AudioFileReader(audioFile))
            using (var writer = new BinaryWriter(File.Create(outputFile)))
            {
                float[] buffer = new float[1024];
                int samplesRead;

                byte packedByte = 0;
                int bitPosition = 0;

                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < samplesRead; i++)
                    {
                        short currentSample =
                            (short)(buffer[i] * short.MaxValue);

                        byte bitResult;

                        if (currentSample > predictedValue)
                        {
                            bitResult = 1;
                            predictedValue += stepSize;
                        }
                        else
                        {
                            bitResult = 0;
                            predictedValue -= stepSize;
                        }

                        // تخزين البت داخل البايت
                        packedByte |= (byte)(bitResult << bitPosition);
                        bitPosition++;

                        if (bitPosition == 8)
                        {
                            writer.Write(packedByte);
                            packedByte = 0;
                            bitPosition = 0;
                        }

                        if (printCounter < 10)
                        {
                            tableText +=
                                $"{currentSample,-16} | {bitResult,-5} | {predictedValue}\n";

                            printCounter++;
                        }
                    }
                }

                // كتابة آخر بايت إذا لم يكتمل
                if (bitPosition > 0)
                {
                    writer.Write(packedByte);
                }
            }

            long sizeBefore = new FileInfo(audioFile).Length;
            long sizeAfter = new FileInfo(outputFile).Length;

            double ratio =
                (1.0 - ((double)sizeAfter / sizeBefore)) * 100;

            string finalReport =
                $"📊 مقارنة الأحجام\n\n" +
                $"الحجم قبل الضغط: {sizeBefore / 1024.0:F2} KB\n" +
                $"الحجم بعد الضغط: {sizeAfter / 1024.0:F2} KB\n" +
                $"نسبة التوفير: {ratio:F2}%\n\n" +
                tableText;

            MessageBox.Show(finalReport, "Delta Modulation");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (audioFile == null)
            {
                MessageBox.Show("اختر ملف WAV أولاً");
                return;
            }

            string outputFile = Path.Combine(
                Path.GetDirectoryName(audioFile),
                Path.GetFileNameWithoutExtension(audioFile) + "_ADM.raw");

            int predictedValue = 0;

            int stepSize = 500;
            int minStep = 100;
            int maxStep = 8000;

            int previousBit = -1;

            string tableText = "📋 أول 10 عينات ADM\n";
            tableText += "----------------------------------------------------------\n";
            tableText += "العينة الحقيقية | البت | التوقع | Step Size\n";
            tableText += "----------------------------------------------------------\n";

            int printCounter = 0;

            using (var reader = new AudioFileReader(audioFile))
            using (var writer = new BinaryWriter(File.Create(outputFile)))
            {
                float[] buffer = new float[1024];
                int samplesRead;

                byte packedByte = 0;
                int bitPosition = 0;

                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < samplesRead; i++)
                    {
                        short currentSample =
                            (short)(buffer[i] * short.MaxValue);

                        byte currentBit;

                        // Delta Decision
                        if (currentSample > predictedValue)
                        {
                            currentBit = 1;
                            predictedValue += stepSize;
                        }
                        else
                        {
                            currentBit = 0;
                            predictedValue -= stepSize;
                        }

                        // Adaptive Step Size
                        if (previousBit != -1)
                        {
                            if (currentBit == previousBit)
                            {
                                // نفس الاتجاه → كبر الخطوة
                                stepSize *= 2;

                                if (stepSize > maxStep)
                                    stepSize = maxStep;
                            }
                            else
                            {
                                // انعكاس الاتجاه → صغر الخطوة
                                stepSize /= 2;

                                if (stepSize < minStep)
                                    stepSize = minStep;
                            }
                        }

                        previousBit = currentBit;

                        // تخزين البتات داخل Bytes
                        packedByte |= (byte)(currentBit << bitPosition);
                        bitPosition++;

                        if (bitPosition == 8)
                        {
                            writer.Write(packedByte);
                            packedByte = 0;
                            bitPosition = 0;
                        }

                        // طباعة أول 10 عينات
                        if (printCounter < 10)
                        {
                            tableText +=
                                $"{currentSample,10} | " +
                                $"{currentBit,3} | " +
                                $"{predictedValue,8} | " +
                                $"{stepSize}\n";

                            printCounter++;
                        }
                    }
                }

                if (bitPosition > 0)
                    writer.Write(packedByte);
            }

            long sizeBefore = new FileInfo(audioFile).Length;
            long sizeAfter = new FileInfo(outputFile).Length;

            double ratio =
                (1.0 - ((double)sizeAfter / sizeBefore)) * 100;

            string report =
                $"📊 Adaptive Delta Modulation\n\n" +
                $"الحجم قبل الضغط: {sizeBefore / 1024.0:F2} KB\n" +
                $"الحجم بعد الضغط: {sizeAfter / 1024.0:F2} KB\n" +
                $"نسبة التوفير: {ratio:F2}%\n\n" +
                tableText;

            MessageBox.Show(report, "ADM");
        }

        
        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(audioFile))
            {
                MessageBox.Show("الرجاء اختيار ملف WAV الأصلي أولاً لتحديد المسار!");
                return;
            }

            string folder = Path.GetDirectoryName(audioFile);
            string baseName = Path.GetFileNameWithoutExtension(audioFile);

            string muLawFile = Path.Combine(folder, baseName + "_MuLaw.raw");
            string deltaFile = Path.Combine(folder, baseName + "_Delta.raw");
            string admFile = Path.Combine(folder, baseName + "_ADM.raw");

            string choice = Microsoft.VisualBasic.Interaction.InputBox(
                "اختر الخوارزمية المراد فك ضغطها وسماعها:\n\n1. Mu-Law\n2. Delta Modulation\n3. Adaptive Delta (ADM)",
                "فك الضغط والاستماع", "1");

            string decompressedWav = "";

            if (choice == "1")
            {
                decompressedWav = Path.Combine(folder, baseName + "_Decoded_MuLaw.wav");
                DecompressMuLaw(muLawFile, decompressedWav);
            }
            else if (choice == "2")
            {
                decompressedWav = Path.Combine(folder, baseName + "_Decoded_Delta.wav");
                DecompressDelta(deltaFile, decompressedWav);
            }
            else if (choice == "3")
            {
                decompressedWav = Path.Combine(folder, baseName + "_Decoded_ADM.wav");
                DecompressADM(admFile, decompressedWav);
            }
            else
            {
                return; 
            }

            
            PlayDecompressedAudio(decompressedWav);
        }

       
        private void DecompressMuLaw(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile)) { MessageBox.Show("الملف المضغوط غير موجود، اضغط أولاً!"); return; }

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(audioFileReader.WaveFormat.SampleRate, audioFileReader.WaveFormat.Channels);

            using (var rawReader = new BinaryReader(File.OpenRead(inputFile)))
            using (var wavWriter = new WaveFileWriter(outputFile, waveFormat))
            {
                while (rawReader.BaseStream.Position < rawReader.BaseStream.Length)
                {
                    byte compressedSample = rawReader.ReadByte();
                    short pcmSample = NAudio.Codecs.MuLawDecoder.MuLawToLinearSample(compressedSample);
                    float floatSample = pcmSample / (float)short.MaxValue;
                    wavWriter.WriteSample(floatSample);
                }
            }
        }

        private void DecompressDelta(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile)) { MessageBox.Show("الملف المضغوط غير موجود، اضغط أولاً!"); return; }

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(audioFileReader.WaveFormat.SampleRate, audioFileReader.WaveFormat.Channels);
            int predictedValue = 0;
            int stepSize = 500; 

            using (var rawReader = new BinaryReader(File.OpenRead(inputFile)))
            using (var wavWriter = new WaveFileWriter(outputFile, waveFormat))
            {
                while (rawReader.BaseStream.Position < rawReader.BaseStream.Length)
                {
                    byte packedByte = rawReader.ReadByte();
                    for (int bitPosition = 0; bitPosition < 8; bitPosition++)
                    {
                        int bitResult = (packedByte >> bitPosition) & 1;

                        if (bitResult == 1) predictedValue += stepSize;
                        else predictedValue -= stepSize;

                        predictedValue = Math.Max(short.MinValue, Math.Min(short.MaxValue, predictedValue));
                        float floatSample = predictedValue / (float)short.MaxValue;
                        wavWriter.WriteSample(floatSample);
                    }
                }
            }
        }

        private void DecompressADM(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile)) { MessageBox.Show("الملف المضغوط غير موجود، اضغط أولاً!"); return; }

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(audioFileReader.WaveFormat.SampleRate, audioFileReader.WaveFormat.Channels);
            int predictedValue = 0;
            int stepSize = 500;
            int minStep = 100;
            int maxStep = 8000;
            int previousBit = -1;

            using (var rawReader = new BinaryReader(File.OpenRead(inputFile)))
            using (var wavWriter = new WaveFileWriter(outputFile, waveFormat))
            {
                while (rawReader.BaseStream.Position < rawReader.BaseStream.Length)
                {
                    byte packedByte = rawReader.ReadByte();
                    for (int bitPosition = 0; bitPosition < 8; bitPosition++)
                    {
                        int currentBit = (packedByte >> bitPosition) & 1;

                        if (currentBit == 1) predictedValue += stepSize;
                        else predictedValue -= stepSize;

                        if (previousBit != -1)
                        {
                            if (currentBit == previousBit)
                            {
                                stepSize *= 2;
                                if (stepSize > maxStep) stepSize = maxStep;
                            }
                            else
                            {
                                stepSize /= 2;
                                if (stepSize < minStep) stepSize = minStep;
                            }
                        }

                        previousBit = currentBit;
                        predictedValue = Math.Max(short.MinValue, Math.Min(short.MaxValue, predictedValue));
                        float floatSample = predictedValue / (float)short.MaxValue;
                        wavWriter.WriteSample(floatSample);
                    }
                }
            }
        }

        private void PlayDecompressedAudio(string wavPath)
        {
            if (outputDevice != null) outputDevice.Stop();

            outputDevice = new WaveOutEvent();
            var playReader = new AudioFileReader(wavPath);
            outputDevice.Init(playReader);
            outputDevice.Play();

            MessageBox.Show($"تم فك الضغط بنجاح وحفظ الملف باسم:\n{Path.GetFileName(wavPath)}\n\nجاري تشغيل الصوت الآن للاستماع! 🎧", "نجاح فك الضغط");
        }
    }
}


