using NAudio.Wave;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave.SampleProviders;
namespace AudioProject


{
    public partial class Form1 : Form
    {
        private string audioFile;
        private AudioFileReader audioFileReader;
        private WaveOutEvent outputDevice;
        private bool cancelRequested = false;
        private List<double> compressionRatioPoints = new List<double>();//////////////أنشأت قائمة لتخزين نسبة الضغط
        private long currentBytesWritten = 0;
        private long originalFileSize = 0;
        private List<double> speedPoints = new List<double>();
        private DateTime processingStartTime;/////////////////////أسجل وقت بداية الضغط ثم أشغل Timer لكي يحسب السرعة بشكل دوري أثناء التنفيذ
        private System.Windows.Forms.Timer speedTimer;//////////////////
        String compressedFilePath;
        public Form1()
        {
            InitializeComponent();


            numStepSize.Maximum = 10000;
            numStepSize.Value = 500;

            numMinStep.Maximum = 5000;
            numMinStep.Value = 100;

            numMaxStep.Maximum = 50000;
            numMaxStep.Value = 8000;

            cmbSampleRate.SelectedIndex = 0;

            speedTimer = new System.Windows.Forms.Timer();
            speedTimer.Interval = 500;                     ////////////Timer يعمل كل 500ms
            speedTimer.Tick += SpeedTimer_Tick;         ///////////وفي كل مرة يستدعيSpeedTimer_Tick()
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
            cancelRequested = false;
            compressionRatioPoints.Clear();
            speedPoints.Clear();
            currentBytesWritten = 0;
            panelGraph.Invalidate();
            panelSpeed.Invalidate();

            if (audioFile == null)
            {
                MessageBox.Show("أولا اختر الملف الصوتي");
                return;
            }

            originalFileSize = new FileInfo(audioFile).Length;
            progressBar1.Value = 0;//////////////
            lblProgress.Text = "0 %";///////////نسبة الشريط (شريط الانجاز) قبل البدء

            lblSpeed.Text = "⚡ جاري...";
            processingStartTime = DateTime.Now;/////////////////////يسجل وقت البداية
            speedTimer.Start();////////////////////يشغل المؤقت.

            string outputFile = Path.Combine(
                Path.GetDirectoryName(audioFile),
                Path.GetFileNameWithoutExtension(audioFile) + "_MuLaw.raw");

            using (var reader = new AudioFileReader(audioFile))
            {
                int targetSampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());////مشان النستخدم يختار قيمة من comobox

                var resampler = new WdlResamplingSampleProvider(reader, targetSampleRate);/////////////تعيد أخذ عينات الملف بالمعدل الجديد.

                using (var writer = new BinaryWriter(File.Create(outputFile)))
                {
                    {
                        long totalSamples = reader.Length / 4;
                        long processedSamples = 0;
                        float[] buffer = new float[1024];
                        int samplesRead;

                        while ((samplesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (cancelRequested)
                            {
                                speedTimer.Stop();
                                lblSpeed.Text = "❌ تم الإلغاء";
                                progressBar1.Value = 0;
                                lblProgress.Text = "Cancelled";
                                MessageBox.Show("تم إلغاء عملية Nonlinear Quantization");
                                return;
                            }

                            for (int i = 0; i < samplesRead; i++)
                            {
                                short pcmSample = (short)(buffer[i] * short.MaxValue);
                                byte compressedSample = NAudio.Codecs.MuLawEncoder.LinearToMuLawSample(pcmSample);
                                writer.Write(compressedSample);

                                currentBytesWritten++;////////////////////////////////كلما كتبت بيانات مضغوطة
                                processedSamples++;

                                int progress = (int)((processedSamples * 100) / totalSamples);/////////////////أثناء التنفيذ حساب نسبة الانجاز
                                if (progress > 100) progress = 100;

                                if (progress != progressBar1.Value)
                                {
                                    progressBar1.Value = progress;//////////////////////لتحديث شريط التقدم
                                    lblProgress.Text = progress + " %";///////////////////لعرض النسبة رقمياً

                                    double ratio = (currentBytesWritten * 100.0) / originalFileSize;////////////////حساب double ratio مشان نسبة الضغط
                                    if (ratio > 100) ratio = 100;
                                    compressionRatioPoints.Add(ratio);/////////////////////  هون عم نخزن  القيمة يلي عم ترجع  
                                    panelGraph.Invalidate();////////////////////////////يجبر الـ Panel على إعادة الرسم مباشرة
                                    Application.DoEvents();/////////////////////////////////للسماح للواجهة بالتحديث أثناء تنفيذ
                                }
                            }
                        }
                    }

                    speedTimer.Stop();
                    progressBar1.Value = 100;
                    lblProgress.Text = "100 %";
                    TimeSpan totalTime = DateTime.Now - processingStartTime;/////////يحسب كم ثانية مرت منذ بدء عملية الضغط
                    lblSpeed.Text = $"✅ انتهى في {totalTime.TotalSeconds:F1} ثانية";

                    long bytesBefore = new FileInfo(audioFile).Length;
                    long bytesAfter = new FileInfo(outputFile).Length;
                    double kbBefore = bytesBefore / 1024.0;
                    double mbBefore = kbBefore / 1024.0;
                    double kbAfter = bytesAfter / 1024.0;
                    double mbAfter = kbAfter / 1024.0;
                    double savedRatio = (1.0 - ((double)bytesAfter / bytesBefore)) * 100;

                    string resultMessage =
                        $"مقارنة الحجم قبل وبعد الضغط لملف WAV:\n\n" +
                         $"━━━ [ إعدادات المعالجة ] ━━━\n" +
                        $"Sample Rate: {targetSampleRate} Hz\n\n" +
                        $"━━━ [ الحجم قبل الضغط ] ━━━\n" +
                        $"بالكيلوبايت: {kbBefore:F2} KB\n" +
                        $"بالميغابايت: {mbBefore:F2} MB\n\n" +
                        $"━━━ [ الحجم بعد الضغط ] ━━━\n" +
                        $"بالكيلوبايت: {kbAfter:F2} KB\n" +
                        $"بالميغابايت: {mbAfter:F2} MB\n\n" +
                        $"نسبة المساحة الموفرة: {savedRatio:F1}%";
                    compressedFilePath = outputFile;
                    MessageBox.Show(resultMessage, "Nonlinear Quantization");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            cancelRequested = false;
            compressionRatioPoints.Clear();
            speedPoints.Clear();
            currentBytesWritten = 0;
            panelGraph.Invalidate();
            panelSpeed.Invalidate();

            if (audioFile == null)
            {
                MessageBox.Show("الرجاء اختيار ملف WAV أولاً!");
                return;
            }

            originalFileSize = new FileInfo(audioFile).Length;
            progressBar1.Value = 0;
            lblProgress.Text = "0 %";
            lblSpeed.Text = "⚡ جاري...";
            processingStartTime = DateTime.Now;
            speedTimer.Start();

            string outputFile = Path.Combine(
                Path.GetDirectoryName(audioFile),
                Path.GetFileNameWithoutExtension(audioFile) + "_Delta.raw");

            int targetSampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());
            int stepSize = (int)numStepSize.Value;///////////////التحكم بحجم خطوة التكميم
            int predictedValue = 0;

            string tableText = "📋 أول 10 عينات:\n";
            tableText += "------------------------------------------------------------\n";
            tableText += "العينة الحقيقية   |  البت  |  التوقع الجديد\n";
            tableText += "------------------------------------------------------------\n";
            int printCounter = 0;

            using (var reader = new AudioFileReader(audioFile))
            using (var writer = new BinaryWriter(File.Create(outputFile)))
            {
                long totalSamples = reader.Length / 4;
                long processedSamples = 0;
                float[] buffer = new float[1024];
                int samplesRead;
                byte packedByte = 0;
                int bitPosition = 0;

                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (cancelRequested)
                    {
                        speedTimer.Stop();
                        lblSpeed.Text = "❌ تم الإلغاء";
                        progressBar1.Value = 0;
                        lblProgress.Text = "Cancelled";
                        MessageBox.Show("تم إلغاء عملية Delta Modulation");
                        return;
                    }

                    for (int i = 0; i < samplesRead; i++)
                    {
                        short currentSample = (short)(buffer[i] * short.MaxValue);
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

                        packedByte |= (byte)(bitResult << bitPosition);
                        bitPosition++;

                        if (bitPosition == 8)
                        {
                            writer.Write(packedByte);
                            currentBytesWritten++;
                            packedByte = 0;
                            bitPosition = 0;
                        }

                        if (printCounter < 10)
                        {
                            tableText += $"{currentSample,-16} | {bitResult,-5} | {predictedValue}\n";
                            printCounter++;
                        }

                        processedSamples++;
                        int progress = (int)((processedSamples * 100) / totalSamples);
                        if (progress > 100) progress = 100;

                        if (progress != progressBar1.Value)
                        {
                            progressBar1.Value = progress;
                            lblProgress.Text = progress + " %";

                            double ratio = (currentBytesWritten * 100.0) / originalFileSize;
                            if (ratio > 100) ratio = 100;
                            compressionRatioPoints.Add(ratio);
                            panelGraph.Invalidate();
                            Application.DoEvents();
                        }
                    }
                }

                if (bitPosition > 0)
                    writer.Write(packedByte);
            }

            speedTimer.Stop();
            progressBar1.Value = 100;
            lblProgress.Text = "100 %";
            TimeSpan totalTime = DateTime.Now - processingStartTime;
            lblSpeed.Text = $"✅ انتهى في {totalTime.TotalSeconds:F1} ثانية";

            long sizeBefore = new FileInfo(audioFile).Length;
            long sizeAfter = new FileInfo(outputFile).Length;
            double ratio2 = (1.0 - ((double)sizeAfter / sizeBefore)) * 100;

            string finalReport =
                 $"━━━ [ إعدادات المعالجة ] ━━━\n" +
                 $"Sample Rate: {targetSampleRate} Hz\n" +
                 $"Step Size: {stepSize}\n\n" +
                $"📊 مقارنة الأحجام\n\n" +
                $"⚙️ حجم الخطوة المستخدم (Step Size): {stepSize}\n" +
                $"الحجم قبل الضغط: {sizeBefore / 1024.0:F2} KB\n" +
                $"الحجم بعد الضغط: {sizeAfter / 1024.0:F2} KB\n" +
                $"نسبة التوفير: {ratio2:F2}%\n\n" +
                tableText;
            compressedFilePath = outputFile;
            MessageBox.Show(finalReport, "Delta Modulation");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            cancelRequested = false;
            compressionRatioPoints.Clear();
            speedPoints.Clear();
            currentBytesWritten = 0;
            panelGraph.Invalidate();
            panelSpeed.Invalidate();

            if (audioFile == null)
            {
                MessageBox.Show("اختر ملف WAV أولاً");
                return;
            }

            originalFileSize = new FileInfo(audioFile).Length;
            progressBar1.Value = 0;
            lblProgress.Text = "0 %";
            lblSpeed.Text = "⚡ جاري...";
            processingStartTime = DateTime.Now;
            speedTimer.Start();

            string outputFile = Path.Combine(
                Path.GetDirectoryName(audioFile),
                Path.GetFileNameWithoutExtension(audioFile) + "_ADM.raw");

            int predictedValue = 0;
            int targetSampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());
            int initialStepSize = (int)numStepSize.Value;//////////////هون القيمة يلي نحنا اخترناها البدائية
            int stepSize = initialStepSize;                 /////////////////////القيمة يلي وصلتلها الخوارزمية بعد التكيف مع الاشارة
            int minStep = (int)numMinStep.Value;/////////////التحكم بحدود الخوارزمية
            int maxStep = (int)numMaxStep.Value;/////////////التحكم بحدود الخوارزمية
            int previousBit = -1;

            string tableText = "📋 أول 10 عينات ADM\n";
            tableText += "----------------------------------------------------------\n";
            tableText += "العينة الحقيقية | البت | التوقع | Step Size\n";
            tableText += "----------------------------------------------------------\n";
            int printCounter = 0;

            using (var reader = new AudioFileReader(audioFile))
            using (var writer = new BinaryWriter(File.Create(outputFile)))
            {
                long totalSamples = reader.Length / 4;
                long processedSamples = 0;
                float[] buffer = new float[1024];
                int samplesRead;
                byte packedByte = 0;
                int bitPosition = 0;

                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (cancelRequested)
                    {
                        speedTimer.Stop();
                        lblSpeed.Text = "❌ تم الإلغاء";
                        progressBar1.Value = 0;
                        lblProgress.Text = "Cancelled";
                        MessageBox.Show("تم إلغاء عملية ADM");
                        return;
                    }

                    for (int i = 0; i < samplesRead; i++)
                    {
                        short currentSample = (short)(buffer[i] * short.MaxValue);
                        byte currentBit;

                        if (currentSample > predictedValue)
                            currentBit = 1;
                        else
                            currentBit = 0;

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

                        if (currentBit == 1)
                            predictedValue += stepSize;
                        else
                            predictedValue -= stepSize;

                        previousBit = currentBit;

                        packedByte |= (byte)(currentBit << bitPosition);
                        bitPosition++;

                        if (bitPosition == 8)
                        {
                            writer.Write(packedByte);
                            currentBytesWritten++;
                            packedByte = 0;
                            bitPosition = 0;
                        }

                        if (printCounter < 10)
                        {
                            tableText +=
                                $"{currentSample,10} | {currentBit,3} | {predictedValue,8} | {stepSize}\n";
                            printCounter++;
                        }

                        processedSamples++;
                        int progress = (int)((processedSamples * 100) / totalSamples);
                        if (progress > 100) progress = 100;

                        if (progress != progressBar1.Value)
                        {
                            progressBar1.Value = progress;
                            lblProgress.Text = progress + " %";

                            double ratio = (currentBytesWritten * 100.0) / originalFileSize;
                            if (ratio > 100) ratio = 100;
                            compressionRatioPoints.Add(ratio);
                            panelGraph.Invalidate();
                            Application.DoEvents();
                        }
                    }
                }

                if (bitPosition > 0)
                    writer.Write(packedByte);
            }

            speedTimer.Stop();
            progressBar1.Value = 100;
            lblProgress.Text = "100 %";
            TimeSpan totalTime = DateTime.Now - processingStartTime;
            lblSpeed.Text = $"✅ انتهى في {totalTime.TotalSeconds:F1} ثانية";

            long sizeBefore = new FileInfo(audioFile).Length;
            long sizeAfter = new FileInfo(outputFile).Length;
            double ratio2 = (1.0 - ((double)sizeAfter / sizeBefore)) * 100;

            string report =
                $"📊 Adaptive Delta Modulation\n\n" +
                $"━━━ [ إعدادات المعالجة ] ━━━\n" +
                $"Sample Rate: {targetSampleRate} Hz\n" +
                $"Initial Step Size: {initialStepSize}\n" +
                $"Final Step Size: {stepSize}\n" +
                 $"Min Step: {minStep}\n" +
              $"Max Step: {maxStep}\n\n" +
                $"الحجم قبل الضغط: {sizeBefore / 1024.0:F2} KB\n" +
                $"الحجم بعد الضغط: {sizeAfter / 1024.0:F2} KB\n" +
                $"نسبة التوفير: {ratio2:F2}%\n\n" +
                tableText;
            compressedFilePath = outputFile;
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
            int stepSize = (int)numStepSize.Value;

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
            int stepSize = (int)numStepSize.Value;
            int minStep = (int)numMinStep.Value;
            int maxStep = (int)numMaxStep.Value;
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

        private void cmbQuantization_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbSampleRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancelRequested = true;
        }

        private void panelGraph_Paint(object sender, PaintEventArgs e)
        {
            if (compressionRatioPoints.Count < 2) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = panelGraph.Width;
            int h = panelGraph.Height;

            // خلفية
            g.Clear(Color.FromArgb(20, 20, 30));

            // رسم الـ Grid
            Pen gridPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1);
            for (int i = 0; i <= 10; i++)
            {
                int y = i * h / 10;
                int x = i * w / 10;
                g.DrawLine(gridPen, 0, y, w, y);
                g.DrawLine(gridPen, x, 0, x, h);
            }

            // تسميات المحور Y
            Font labelFont = new Font("Arial", 7);
            for (int i = 0; i <= 10; i++)
            {
                int percent = 100 - (i * 10);
                int y = i * h / 10;
                g.DrawString(percent + "%", labelFont, Brushes.LightGray, 2, y);
            }

            // عنوان
            g.DrawString("نسبة الضغط أثناء التنفيذ", new Font("Arial", 9, FontStyle.Bold), Brushes.White, w / 2 - 70, 5);

            // خط الرسم
            Pen linePen = new Pen(Color.Cyan, 2);
            int total = compressionRatioPoints.Count;

            for (int i = 1; i < total; i++)
            {
                int x1 = (i - 1) * w / (total - 1);
                int x2 = i * w / (total - 1);

                // نسبة الضغط: كلما صغرت النسبة → الضغط أكثر → ارسمها من الأعلى
                int y1 = h - (int)(compressionRatioPoints[i - 1] * h / 100.0);
                int y2 = h - (int)(compressionRatioPoints[i] * h / 100.0);

                g.DrawLine(linePen, x1, y1, x2, y2);
            }

            // النقطة الأخيرة مع قيمتها
            if (total >= 1)
            {
                double lastVal = compressionRatioPoints[total - 1];//////////////////////الحصول على آخر قيمة ضغط تم الوصول إليها
                int lx = (total - 1) * w / Math.Max(total - 1, 1);
                int ly = h - (int)(lastVal * h / 100.0);
                g.FillEllipse(Brushes.Yellow, lx - 4, ly - 4, 8, 8);////////////وضعت دائرة صفراء على آخر نقطة لتكون واضحة للمستخدم
                g.DrawString($"{lastVal:F1}%", new Font("Arial", 8, FontStyle.Bold), Brushes.Yellow, lx + 5, ly - 10);
            }
        }

        private void SpeedTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - processingStartTime;////////////////يحسب كم ثانية مرت منذ بدء عملية الضغط.

            if (elapsed.TotalSeconds > 0 && currentBytesWritten > 0)////////////نتأكد أن هناك وقت مرّ فعلاً وأن البرنامج كتب بيانات للملف.
            {
                double speedMBps = (currentBytesWritten / 1024.0 / 1024.0) / elapsed.TotalSeconds;/////حساب سرعة المعالجة
                speedPoints.Add(speedMBps);/////////////////كل قيمة سرعة يتم حسابها تضاف إلى القائمة
                panelSpeed.Invalidate();//////////////اعادة الر سم على panel

                if (speedMBps >= 1)
                    lblSpeed.Text = $"⚡ {speedMBps:F2} MB/s";
                else
                    lblSpeed.Text = $"⚡ {speedMBps * 1024:F1} KB/s";
            }
        }

        private void panelSpeed_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = panelSpeed.Width;
            int h = panelSpeed.Height;

            g.Clear(Color.FromArgb(20, 30, 20));

            Pen gridPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1);
            for (int i = 0; i <= 10; i++)
            {
                g.DrawLine(gridPen, 0, i * h / 10, w, i * h / 10);
                g.DrawLine(gridPen, i * w / 10, 0, i * w / 10, h);
            }

            g.DrawString("سرعة المعالجة (MB/s)", new Font("Arial", 9, FontStyle.Bold), Brushes.White, w / 2 - 70, 5);

            if (speedPoints.Count < 2) return;

            double maxSpeed = 0;
            foreach (double s in speedPoints)
                if (s > maxSpeed) maxSpeed = s;
            if (maxSpeed == 0) maxSpeed = 1;

            Font labelFont = new Font("Arial", 7);
            for (int i = 0; i <= 5; i++)
            {
                double val = maxSpeed * (5 - i) / 5.0;
                int y = i * h / 5;
                g.DrawString($"{val:F2}", labelFont, Brushes.LightGreen, 2, y);
            }

            int total = speedPoints.Count;

            // تعبئة تحت الخط
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddLine(0, h, 0, h - (int)(speedPoints[0] * h / maxSpeed));
            for (int i = 1; i < total; i++)
            {
                int x = i * w / (total - 1);
                int y = h - (int)(speedPoints[i] * h / maxSpeed);
                path.AddLine((i - 1) * w / (total - 1), h - (int)(speedPoints[i - 1] * h / maxSpeed), x, y);
            }
            path.AddLine((total - 1) * w / (total - 1), h - (int)(speedPoints[total - 1] * h / maxSpeed), w, h);
            path.CloseFigure();
            g.FillPath(new SolidBrush(Color.FromArgb(40, 0, 255, 0)), path);

            // رسم الخط
            Pen linePen = new Pen(Color.LimeGreen, 2);
            for (int i = 1; i < total; i++)
            {
                int x1 = (i - 1) * w / (total - 1);
                int x2 = i * w / (total - 1);
                int y1 = h - (int)(speedPoints[i - 1] * h / maxSpeed);
                int y2 = h - (int)(speedPoints[i] * h / maxSpeed);
                g.DrawLine(linePen, x1, y1, x2, y2);
            }

            // النقطة الأخيرة
            double lastSpeed = speedPoints[total - 1];
            int lx = w - 1;
            int ly = h - (int)(lastSpeed * h / maxSpeed);
            g.FillEllipse(Brushes.Yellow, lx - 4, ly - 4, 8, 8);
            g.DrawString($"{lastSpeed:F2} MB/s", new Font("Arial", 8, FontStyle.Bold), Brushes.Yellow, lx - 75, ly - 15);
        }
        //SaveAudioFile
        private void button8_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(audioFile))

            {
                MessageBox.Show("chose audio files", "majd", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            if (string.IsNullOrEmpty(compressedFilePath) || !File.Exists(compressedFilePath))

            {
                MessageBox.Show("Compress Audio File before saving ", "majd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "RAW Audio Files (*.raw)|*.raw|All Files (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.Copy(compressedFilePath, sfd.FileName, true);
            }
        }

        //reset audio value
        private void button9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(audioFile))
            {
                MessageBox.Show("chose audio file first", "majd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            numStepSize.Value = 500;
            numMinStep.Value = 100;
            numMaxStep.Value = 8000;
            cmbSampleRate.SelectedIndex = 0;


            cancelRequested = false;
            currentBytesWritten = 0;
            compressedFilePath = null;


            compressionRatioPoints.Clear();
            speedPoints.Clear();
            panelGraph.Invalidate(); 
            panelSpeed.Invalidate();


            progressBar1.Value = 0;
            lblProgress.Text = "0 %";
            lblSpeed.Text = " Ready";


            MessageBox.Show("Reset done!", "reset values", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
    }
    }




}


