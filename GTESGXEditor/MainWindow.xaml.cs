using GTESGXEditor.Entities;
using Microsoft.WindowsAPICodePack.Dialogs;
using Syroot.BinaryData;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace GTESGXEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ESGXEntry esgx = new ESGXEntry();
        public SGXDEntry selectedSgxd = new SGXDEntry();
        int activeSGXDIndex;
        bool changedByUser = true, isActiveFile = false;
        float fileSize = 0.0f;

        public MainWindow()
        {
            InitializeComponent();
        }

        public byte[] ReadVAG(string path)
        {
            byte[] audioStream;
            var bytes = File.ReadAllBytes(path);

            using (var stream = new BinaryStream(new MemoryStream(bytes)))
            {
                if (stream.ReadString(3) != "VAG")
                    throw new InvalidDataException("Not a VAG file. Please open a VAG file and try again.");

                stream.Position = 48;

                audioStream = stream.ReadBytes(bytes.Length - 48);
            }

            return audioStream;
        }

        public uint ReadVAGSampleRate(string path)
        {
            var bytes = File.ReadAllBytes(path);

            uint sampleRate = 0;

            using (var stream = new BinaryStream(new MemoryStream(bytes), ByteConverter.Big))
            {
                if (stream.ReadString(3) != "VAG")
                    throw new InvalidDataException("Not a VAG file. Please open a VAG file and try again.");

                stream.Position = 16;

                sampleRate = stream.ReadUInt32();
            }

            return sampleRate;
        }

        private void RefreshSGXDEntries(bool fromListChange = false, bool skipEstimate = false)
        {
            int lastSelectedIndex = lstEsgx.SelectedIndex;
            if (lstEsgx != null)
            {
                if (!fromListChange)
                {
                    lstEsgx.Items.Clear();

                    fileSize = 0.0f;

                    for (int i = 0; i < esgx.sgxdEntries.Count; i++)
                    {
                        lstEsgx.Items.Add(string.Format("{0} - {1}", i, esgx.sgxdEntries[i].nameChunk.fileName));
                        fileSize += esgx.sgxdEntries[i].fileSize;
                    }

                    if (!skipEstimate)
                    {
                        lblEstimate.Content = string.Format("Estimated File Size: {0}KB", Math.Round(fileSize / 1000, 1));
                        if (Math.Round(fileSize / 1000, 1) >= 200)
                        {
                            lblEstimate.Foreground = Brushes.Red;
                        }
                        else
                        {
                            lblEstimate.Foreground = Brushes.Black;
                        }
                    }

                    if (lstEsgx.Items.Count > 0)
                    {
                        btnDel.IsEnabled = true;
                    }
                    else
                    {
                        btnDel.IsEnabled = false;
                    }


                    lstEsgx.SelectedIndex = lastSelectedIndex;
                }

                mnuSave.IsEnabled = isActiveFile;

                txtName.IsEnabled = isActiveFile;
                txtStartSample.IsEnabled = isActiveFile;
                txtEndSample.IsEnabled = isActiveFile;
                txtRpmPitch.IsEnabled = isActiveFile;
                txtRpmStart.IsEnabled = isActiveFile;
                txtRpmEnd.IsEnabled = isActiveFile;
                txtRpmVolume.IsEnabled = isActiveFile;
                txtRpmFrequency.IsEnabled = isActiveFile;

                btnImportVag.IsEnabled = isActiveFile;
                btnExportVag.IsEnabled = isActiveFile;
            }
        }

        private void UpdateSGXDEntry(int index)
        {
            try
            {
                esgx.sgxdEntries[index].nameChunk.fileName = txtName.Text;
                esgx.sgxdEntries[index].waveChunk.loopStartSample = uint.Parse(txtStartSample.Text);
                esgx.sgxdEntries[index].waveChunk.loopEndSample = uint.Parse(txtEndSample.Text);
                esgx.sampleSettings[index].rpmPitch = short.Parse(txtRpmPitch.Text);
                esgx.sampleSettings[index].rpmStart = short.Parse(txtRpmStart.Text);
                esgx.sampleSettings[index].rpmEnd = short.Parse(txtRpmEnd.Text);
                esgx.sampleSettings[index].rpmVolume = short.Parse(txtRpmVolume.Text);
                esgx.sampleSettings[index].rpmFrequency = int.Parse(txtRpmFrequency.Text);
                lstEsgx.Items[index] = string.Format("{0} - {1}", index, esgx.sgxdEntries[index].nameChunk.fileName);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Invalid input format. Please input a value value and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "ESGX File|*.esgx";
            //openFile.InitialDirectory = Directory.GetCurrentDirectory();
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    esgx = new ESGXEntry();

                    esgx.ReadFile(openFile.FileName);

                    isActiveFile = true;

                    RefreshSGXDEntries();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void OpenES_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            //openFile.InitialDirectory = Directory.GetCurrentDirectory();
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    esgx = new ESGXEntry();

                    esgx.ReadESFile(openFile.FileName);

                    lblEstimate.Content = string.Format("Estimated File Size: {0}KB", Math.Round(double.Parse(new FileInfo(openFile.FileName).Length.ToString()) / 1000, 1).ToString());
                    if (Math.Round(fileSize / 1000, 1) >= 200)
                    {
                        lblEstimate.Foreground = Brushes.Red;
                    }
                    else
                    {
                        lblEstimate.Foreground = Brushes.Black;
                    }

                    isActiveFile = true;

                    RefreshSGXDEntries(false, true);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "ESGX File|*.esgx";
            //saveFile.InitialDirectory = Directory.GetCurrentDirectory();
            saveFile.RestoreDirectory = true;

            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (esgx.Validate())
                {
                    esgx.SaveFile(saveFile.FileName);
                }
            }
        }

        private void VAGExport_Click(object sender, RoutedEventArgs e)
        {
            var saveFile = new FolderBrowserDialog();

            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var sgxd in esgx.sgxdEntries)
                {
                    sgxd.WriteVAG(saveFile.SelectedPath);
                }

                System.Windows.Forms.MessageBox.Show(string.Format("Files successfully written to {0}", saveFile.SelectedPath), "Files saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void lstEsgx_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstEsgx.Items.Count > 0)
            {
                changedByUser = false;

                if (lstEsgx.SelectedIndex != -1)
                {
                    selectedSgxd = esgx.sgxdEntries[lstEsgx.SelectedIndex];

                    txtName.Text = selectedSgxd.nameChunk.fileName;
                    txtFileSize.Text = selectedSgxd.fileSize.ToString();
                    txtSampleRate.Text = selectedSgxd.waveChunk.soundSampleRate.ToString();
                    txtStartSample.Text = selectedSgxd.waveChunk.loopStartSample.ToString();
                    txtEndSample.Text = selectedSgxd.waveChunk.loopEndSample.ToString();
                    txtRpmPitch.Text = esgx.sampleSettings[lstEsgx.SelectedIndex].rpmPitch.ToString();
                    txtRpmStart.Text = esgx.sampleSettings[lstEsgx.SelectedIndex].rpmStart.ToString();
                    txtRpmEnd.Text = esgx.sampleSettings[lstEsgx.SelectedIndex].rpmEnd.ToString();
                    txtRpmVolume.Text = esgx.sampleSettings[lstEsgx.SelectedIndex].rpmVolume.ToString();
                    txtRpmFrequency.Text = esgx.sampleSettings[lstEsgx.SelectedIndex].rpmFrequency.ToString();
                    lblCurrentVag.Content = selectedSgxd.audioStreamName == "" ? "" : selectedSgxd.audioStreamName;

                    activeSGXDIndex = lstEsgx.SelectedIndex;
                }

                changedByUser = true;

                RefreshSGXDEntries(true);
            }
        }

        private void btnImportVag_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "VAG File|*.vag";
            //openFile.InitialDirectory = Directory.GetCurrentDirectory();
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;
            openFile.RestoreDirectory = true;
            byte[] audioStream;

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    selectedSgxd.audioStream = ReadVAG(openFile.FileName);
                    selectedSgxd.waveChunk.soundSampleRate = ReadVAGSampleRate(openFile.FileName);
                    selectedSgxd.fileSize = (ushort)selectedSgxd.audioStream.Length;
                    selectedSgxd.audioStreamName = Path.GetFileName(openFile.FileName);
                    txtFileSize.Text = selectedSgxd.fileSize.ToString();
                    txtSampleRate.Text = selectedSgxd.waveChunk.soundSampleRate.ToString();
                    lblCurrentVag.Content = selectedSgxd.audioStreamName;

                    RefreshSGXDEntries();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void txtName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtName.Text.Contains("_"))
                {
                    if (int.TryParse(txtName.Text.Split('_')[1], out int t))
                    {
                        txtRpmPitch.Text = txtName.Text.Split('_')[1];
                        txtRpmStart.Text = txtName.Text.Split('_')[1];
                        txtRpmEnd.Text = txtName.Text.Split('_')[1];
                    }
                }
                //esgx.sgxdEntries[lstEsgx.SelectedIndex].nameChunk.fileName = txtName.Text;
            }
        }

        private void txtStartSample_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtStartSample.Text.Length > 0)
                {
                    //esgx.sgxdEntries[lstEsgx.SelectedIndex].waveChunk.loopStartSample = (uint)int.Parse(txtStartSample.Text);
                }
            }
        }

        private void txtEndSample_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtEndSample.Text.Length > 0)
                {
                    //esgx.sgxdEntries[lstEsgx.SelectedIndex].waveChunk.loopEndSample = (uint)int.Parse(txtEndSample.Text);
                }
            }
        }

        private void txtRpmPitch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtRpmPitch.Text.Length > 0)
                {
                    //esgx.sampleSettings[lstEsgx.SelectedIndex].rpmPitch = short.Parse(txtRpmPitch.Text);
                }
            }
        }

        private void txtRpmStart_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtRpmStart.Text.Length > 0)
                {
                    //esgx.sampleSettings[lstEsgx.SelectedIndex].rpmStart = short.Parse(txtRpmStart.Text);
                }
            }
        }

        private void txtRpmEnd_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            { 
                if (txtRpmEnd.Text.Length > 0)
                {
                    //esgx.sampleSettings[lstEsgx.SelectedIndex].rpmEnd = short.Parse(txtRpmEnd.Text);
                }
            }
        }

        private void txtRpmVolume_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtRpmVolume.Text.Length > 0)
                {
                    //esgx.sampleSettings[lstEsgx.SelectedIndex].rpmVolume = short.Parse(txtRpmVolume.Text);
                }
            }
        }

        private void txtRpmFrequency_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (changedByUser && lstEsgx.SelectedIndex != -1)
            {
                if (txtRpmFrequency.Text.Length > 0)
                {
                    //esgx.sampleSettings[lstEsgx.SelectedIndex].rpmFrequency = short.Parse(txtRpmFrequency.Text);
                }
            }
        }

        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (activeSGXDIndex != -1)
            {
                UpdateSGXDEntry(activeSGXDIndex);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            esgx = new ESGXEntry();

            isActiveFile = true;

            RefreshSGXDEntries();

            foreach (System.Windows.Controls.TextBox textbox in grdSGXD.Children.OfType<System.Windows.Controls.TextBox>())
            {
                textbox.Text = "";
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            esgx.sampleSettings.Add(new SampleSetting());
            esgx.sgxdEntries.Add(new SGXDEntry());
            esgx.sampleAmount++;

            isActiveFile = true;

            RefreshSGXDEntries();

            lstEsgx.SelectedIndex = (int)esgx.sampleAmount - 1;
        }
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstEsgx.SelectedIndex != -1)
            {
                esgx.sgxdEntries.RemoveAt(lstEsgx.SelectedIndex);
                esgx.sampleAmount--;

                lstEsgx.Items.RemoveAt(lstEsgx.SelectedIndex);

                RefreshSGXDEntries();

                lstEsgx.SelectedIndex = lstEsgx.Items.Count - 1;
            }
        }
    }
}
