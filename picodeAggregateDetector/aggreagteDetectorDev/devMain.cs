using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;
using picodeAggregateDetector;

namespace aggreagteDetectorDev
{
    public partial class devMain : Form
    {

        private string[] frameIDs = null;
        public string resultFile = "aggregate.summary.csv";

        private picodeAggregateDetector.aggregateDetector dt;

        static private bool DEBUGMODE = false;

        public devMain(bool debug_mode=false)
        {
            InitializeComponent();
#if DEBUG
            DEBUGMODE = true;
#endif
            if (debug_mode)
                DEBUGMODE = true;

            this.clearPictureBoxes();

            if (Properties.Settings.Default.LoadPath == null || Properties.Settings.Default.LoadPath == "")
                Properties.Settings.Default.LoadPath = Application.StartupPath;

            dt = null;

            initialFrameImageID();
            refreshSetting();

            this.Text = String.Format("PiCode aggregation detector  {0}.{1}.{2} {3}:{4}"
                , ThisAssembly.Git.BaseVersion.Major
                , ThisAssembly.Git.BaseVersion.Minor
                , ThisAssembly.Git.BaseVersion.Patch
                , ThisAssembly.Git.Branch
                , ThisAssembly.Git.Commits);

            if (!DEBUGMODE)
                denkaRemove();
        }

        private void denkaRemove()
        {
            this.groupErode.Visible = false;
            this.gpBinary.Visible = false;
            this.groupCircle.Visible = false;
            this.gpComponents.Visible = false;
            this.saveParametersAsDefaultToolStripMenuItem.Visible = false;
            this.pnlBottom.Visible = false;
            this.lblBeadEmpty.Visible = false;
            this.lblBeadOverlap.Visible = false;
            this.numCircleCoverageThreshod.Visible = false;
            this.numCircleEmptyThreshold.Visible = false;
            this.lblOverlapped.Visible = false;
            this.lblHighCoveredBeads.Visible = false;
            this.lblNumEmpty.Visible = false;
            this.lblEmptyBeads.Visible = false;

            this.tabImage.TabPages.Remove(tabED1);
            this.tabImage.TabPages.Remove(tabBinary);
            this.tabImage.TabPages.Remove(tabED2);
        }

        private void updateSettings()
        {
            //binary
            Properties.Settings.Default.binaryMax = (int)this.numBinaryMaxValue.Value;
            Properties.Settings.Default.binaryPara1 = (int)this.numBinaryPara1.Value;
            Properties.Settings.Default.binaryAdaptive = this.cboBinaryAdaptive.SelectedIndex;
            Properties.Settings.Default.binaryThreshold = this.cboBinaryThreshold.SelectedIndex;
            Properties.Settings.Default.binaryBlockSize = (int)this.numBinaryBlockSize.Value;

            //Erode and dilate
            Properties.Settings.Default.erodeFirst = this.chkErodeFirst.Checked;
            Properties.Settings.Default.erodeIteration = (int)this.numErode.Value;
            Properties.Settings.Default.dilateIteration = (int)this.numDilate.Value;

            //Circle Detection
            Properties.Settings.Default.circleDp = (double)this.numDp.Value;
            Properties.Settings.Default.circleMinDist = (double)this.numMinDist.Value;
            Properties.Settings.Default.circlePara1 = (double)this.numCirclePara1.Value;
            Properties.Settings.Default.circlePara2 = (double)this.numCirclePara2.Value;
            Properties.Settings.Default.circleMinRadius = (int)this.numMinRadius.Value;
            Properties.Settings.Default.circleMaxRadius = (int)this.numMaxRadius.Value;

            //counting
            Properties.Settings.Default.circleAreaThreshold= (double)this.numCircleCoverageThreshod.Value;
            Properties.Settings.Default.circleEmptyThreshold = (double)this.numCircleEmptyThreshold.Value;

            Properties.Settings.Default.compCovThreshold = (double) this.numThresholdCover.Value;
            Properties.Settings.Default.compAreaThreshold = (int) this.numThresholdArea.Value;
            Properties.Settings.Default.compBearAreaThreshold = (int) this.numBeadArea.Value;


            Properties.Settings.Default.Save();
        }

        private void refreshSetting()
        {
            //binary
            this.numBinaryMaxValue.Value = Properties.Settings.Default.binaryMax;
            this.numBinaryPara1.Value = Properties.Settings.Default.binaryPara1;
            this.cboBinaryAdaptive.SelectedIndex = Properties.Settings.Default.binaryAdaptive;
            this.cboBinaryThreshold.SelectedIndex = Properties.Settings.Default.binaryThreshold;
            this.numBinaryBlockSize.Value = Properties.Settings.Default.binaryBlockSize;

            //Erode and dilate
            this.chkErodeFirst.Checked = Properties.Settings.Default.erodeFirst;
            this.numErode.Value = Properties.Settings.Default.erodeIteration;
            this.numDilate.Value = Properties.Settings.Default.dilateIteration;

            //Circle Detection
            this.numDp.Value = (decimal)Properties.Settings.Default.circleDp;
            this.numMinDist.Value = (decimal)Properties.Settings.Default.circleMinDist;
            this.numCirclePara1.Value = (decimal)Properties.Settings.Default.circlePara1;
            this.numCirclePara2.Value = (decimal)Properties.Settings.Default.circlePara2;
            this.numMinRadius.Value = Properties.Settings.Default.circleMinRadius;
            this.numMaxRadius.Value = Properties.Settings.Default.circleMaxRadius;

            //counting
            this.numCircleCoverageThreshod.Value = (Decimal)Properties.Settings.Default.circleAreaThreshold;
            this.numCircleEmptyThreshold.Value = (Decimal)Properties.Settings.Default.circleEmptyThreshold;
            this.numThresholdCover.Value = (Decimal)Properties.Settings.Default.compCovThreshold;
            this.numThresholdArea.Value = Properties.Settings.Default.compAreaThreshold;
            this.numBeadArea.Value = Properties.Settings.Default.compBearAreaThreshold;


        }
        private void devMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateSettings();
            if (MessageBox.Show("Exit?", "Exit aggregation detector", MessageBoxButtons.YesNo) != DialogResult.Yes)
                e.Cancel = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "image files|*.jpeg;*.jpg;*.bmp;*.png";
            ofd.InitialDirectory = (Directory.Exists(Properties.Settings.Default.LoadPath)) ?
                Properties.Settings.Default.LoadPath
                :
                Application.StartupPath;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.clearPictureBoxes();
                try
                {
                    picOrig.Image = Image.FromFile(ofd.FileName);
                    Properties.Settings.Default.LoadPath = Path.GetDirectoryName(ofd.FileName);
                    

                    dt = aggregateDetector.fromFile(ofd.FileName, getDetectorParams());
                    Properties.Settings.Default.Save();

                    this.picOrig.Image = dt.source.ToBitmap();
                    this.picBinary.Image = dt.bindaryImage.ToBitmap();
                    this.picED1.Image = dt.dilateErodeImage1.ToBitmap();
                    this.picED2.Image = dt.dilateErodeImage2.ToBitmap();
                    this.picCC.Image = dt.dilateErodeImage2.ToBitmap();
                    //this.picCC.Image = dt.CComponentsImage.ToBitmap();
                    this.picObjects.Image = dt.ResultImage.ToBitmap();
                    this.lblNumLabels.Text = dt.ccCount.ToString();
                    this.lblHighCover.Text = dt.ccHighCoveredCount.ToString();
                    this.lblLargeArea.Text = dt.ccLargeAreaCount.ToString();

                    this.lblNumBeads.Text = dt.beads.Length.ToString();
                    this.lblHighCoveredBeads.Text =  dt.highCoveredBeads.Length.ToString();
                    this.lblEmptyBeads.Text = dt.emptyBeads.Length.ToString();
                    this.lblNumAggCC.Text = dt.caWithSolidBead.Count.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image" + Environment.NewLine + ex.Message);
                    dt = null;
                }
            }

        }

        private aggregateDetectorParams getDetectorParams()
        {
            aggregateDetectorParams rtn = new aggregateDetectorParams();

            //binary
            rtn.binaryMaxValue = new Gray((int)this.numBinaryMaxValue.Value);
            rtn.binaryPara1Value = new Gray((int)this.numBinaryPara1.Value);
            rtn.binaryAdaptiveType = (this.cboBinaryAdaptive.SelectedIndex==0) ?
                Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC 
                : 
                Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC;
            rtn.binaryThresholdType = (this.cboBinaryThreshold.SelectedIndex == 0) ?
                Emgu.CV.CvEnum.ThresholdType.Binary
                : 
                Emgu.CV.CvEnum.ThresholdType.BinaryInv;
            rtn.binaryBlockSize = (int)this.numBinaryBlockSize.Value;

            //Erode and dilate
            rtn.edOrder = (this.chkErodeFirst.Checked) ? 
                aggregateDetectorParams.erodeDilateOrder.erodeFirst 
                : 
                aggregateDetectorParams.erodeDilateOrder.dilateFirst;
            rtn.ErodeIteration = (int)this.numErode.Value;
            rtn.DilateIteration = (int)this.numDilate.Value;

            //Circle Detection
            rtn.circleDp = (double)this.numDp.Value;
            rtn.circleMinDist = (double)this.numMinDist.Value;
            rtn.circlePara1 = (double)this.numCirclePara1.Value;
            rtn.circlePara2 = (double)this.numCirclePara2.Value;
            rtn.circleMinRadius = (int)this.numMinRadius.Value;
            rtn.circleMaxRadius = (int)this.numMaxRadius.Value;

            //counting
            rtn.circleCoverThreshold = (double)this.numCircleCoverageThreshod.Value;

            rtn.ccThresholdArea = (int)this.numThresholdArea.Value;
            rtn.ccThresholdBead = (int)this.numBeadArea.Value;
            rtn.ccThresholdCoverage = (double)this.numThresholdCover.Value;


            return rtn;
        }
        private void clearPictureBoxes()
        {
            picObjects.Image = null;
            picOrig.Image = null;
            picBinary.Image = null;
            picED1.Image = null;
            picED2.Image = null;
            picCC.Image = null;

            lblNumLabels.Text = "";
            inComponent = false;
            this.lblHighCoveredBeads.Text = "";
            this.lblHighCover.Text = "";
            this.lblNumBeads.Text = "";
            this.lblHighCoveredBeads.Text = "";
            this.lblLargeArea.Text = "";

            this.lblNumBeads.Text = "";
            this.lblHighCoveredBeads.Text = "";
            this.lblNumLabels.Text = "";
            this.lblHighCover.Text = "";
            this.lblLargeArea.Text = "";
        }

        private bool inComponent = false;
        private void picCC_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox bx = (PictureBox)sender;
            if (this.dt.CComponentsImage == null || bx.Image==null)
                return;

            if(inComponent || e.Button== MouseButtons.Right)
            {
                bx.Image = this.dt.dilateErodeImage2.ToBitmap();
                inComponent = false;
                return;
            }

            Point tPos = locationInZoomedImage(bx, e.Location);
            if (tPos.X < 0 || tPos.Y < 0 || tPos.X>=bx.Image.Width || tPos.Y>=bx.Image.Height)
                return;

            try
            {
                int label = (int)
                    this.dt.CComponentsImage[tPos.Y, tPos.X].Intensity;

                if (label != 0)
                {
                    Image<Gray, byte> tImg = this.dt.CComponentsImage.InRange(new Gray(label), new Gray(label))
                        .Dilate(10).Erode(20).Dilate(10);
                    //double covRate = this.dt.ccCoverageForTest(tImg, this.dt.CComponentsImage);

                    bx.Image = tImg.Bitmap;
                    inComponent = !inComponent;

                    MessageBox.Show(
                        "Area = " + this.dt.ccAreas[label].area.ToString()+" Pixels"
                        + Environment.NewLine
                        + "Coverage = " + this.dt.ccAreas[label].coverage.ToString("0.000")
                        );
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void numBinaryMaxValue_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown ud = (NumericUpDown)sender;
            if (ud.Value <= this.numBinaryPara1.Value)
                ud.Value = this.numBinaryPara1.Value + 1;
        }

        private void numBinaryPara1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown ud = (NumericUpDown)sender;
            if (ud.Value >= this.numBinaryMaxValue.Value)
                ud.Value = this.numBinaryMaxValue.Value - 1;
        }


        private void numMinRadius_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown ud = (NumericUpDown)sender;
            if (ud.Value > this.numMaxRadius.Value)
                ud.Value = this.numMaxRadius.Value;
        }

        private void numMaxRadius_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown ud = (NumericUpDown)sender;
            if (ud.Value < this.numMinRadius.Value)
                ud.Value = this.numMinRadius.Value;
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            if (this.dt!=null && this.picOrig.Image!=null)
            {
                this.clearPictureBoxes();

                try
                {
                    dt.doDetect(getDetectorParams());
                    Properties.Settings.Default.Save();

                    this.picOrig.Image = dt.source.ToBitmap();
                    this.picBinary.Image = dt.bindaryImage.ToBitmap();
                    this.picED1.Image = dt.dilateErodeImage1.ToBitmap();
                    this.picED2.Image = dt.dilateErodeImage2.ToBitmap();
                    this.picCC.Image = dt.dilateErodeImage2.ToBitmap();
                    this.picObjects.Image = dt.ResultImage.ToBitmap();
                    

                    this.lblNumBeads.Text = dt.beads.Length.ToString();
                    this.lblHighCoveredBeads.Text = dt.highCoveredBeads.Length.ToString();
                    this.lblEmptyBeads.Text = dt.emptyBeads.Length.ToString();
                    this.lblNumAggCC.Text = dt.caWithSolidBead.Count.ToString();

                    this.lblNumLabels.Text = dt.ccCount.ToString();
                    this.lblHighCover.Text = dt.ccHighCoveredCount.ToString();
                    this.lblLargeArea.Text = dt.ccLargeAreaCount.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to re-detect" + Environment.NewLine + ex.Message);
                    dt = null;
                }
            }
        }

        private void picCC_MouseHover(object sender, EventArgs e)
        {
        }

        private void picCC_MouseLeave(object sender, EventArgs e)
        {
            lblCursor.Text = "";
            lblBoxSize.Text = "";
            lblImageSize.Text="";
        }

        private void picCC_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox bx = (PictureBox)sender;
            
            lblBoxSize.Text= this.picCC.ClientSize.Width.ToString() + "," + this.picCC.ClientSize.Height.ToString();
            if (this.picCC.Image != null)
            {
                Point mp = locationInZoomedImage(bx, e.Location);
                lblImageSize.Text = bx.Image.Size.Width.ToString() + "," + bx.Image.Size.Height.ToString();


                lblCursor.Text = e.X.ToString() + "," + e.Y.ToString() + " [" + mp.X.ToString() + "," + mp.Y.ToString() + "]";
            }
                
        }


        private Point locationInZoomedImage(PictureBox bx, Point boxLocation)
        {
            if (bx == null || boxLocation == Point.Empty || bx.Image == null)
                return Point.Empty;

            double wRatio = bx.ClientSize.Width * 1.0 / bx.Image.Width;
            double hRatio = bx.ClientSize.Height * 1.0 / bx.Image.Height;

            if(wRatio < hRatio) //width 滿版 
            {
                //計算Height shift
                int hShift = (int)(bx.ClientSize.Height - (bx.Image.Height*wRatio)) / 2;
                return new Point(
                    (int)(boxLocation.X / wRatio)
                    , 
                    (int)((boxLocation.Y - hShift) / wRatio));

            }
            else //Height 滿版 
            {
                //計算Width shift
                int wShift = (int)(bx.ClientSize.Width - (bx.Image.Width * hRatio)) / 2;
                return new Point(
                    (int)((boxLocation.X-wShift) / hRatio)
                    ,
                    (int)(boxLocation.Y / hRatio));
            }

        }

        private void picObjects_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox bx = (PictureBox)sender;
            if (this.dt.CComponentsImage == null || bx.Image == null || this.dt.beads==null)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            Point tPos = locationInZoomedImage(bx, e.Location);
            if (tPos.X < 0 || tPos.Y < 0 || tPos.X >= bx.Image.Width || tPos.Y >= bx.Image.Height)
                return;

            try
            {
                CircleF cf = new CircleF();
                foreach(CircleF c in this.dt.beads)
                {
                    double tDist = Math.Pow(tPos.X - c.Center.X, 2) + Math.Pow(tPos.Y - c.Center.Y, 2);
                    if (Math.Pow(c.Radius, 2) > tDist)
                        if (cf.Radius <= 0 || tDist<(Math.Pow(cf.Center.X - c.Center.X, 2) + Math.Pow(cf.Center.Y - c.Center.Y, 2)))
                            cf = c;

                }

                Dictionary<int, bool> cas = new Dictionary<int, bool>();
                if (cf.Radius>0)
                {
                    String m = String.Format("Center : {0},{1}\nArea : {2}\nCover Rate : {3}", cf.Center.X, cf.Center.Y, (int)Math.Pow(cf.Radius,2), this.dt.beadCoverRate(cf,cas));
                    MessageBox.Show(m);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void batchFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.InitialDirectory = (Directory.Exists(Properties.Settings.Default.LoadPath))?
                Properties.Settings.Default.LoadPath
                :
                Application.StartupPath;
            dlg.Title = "Select assay image folder";
            dlg.IsFolderPicker = true;
            



            if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            Properties.Settings.Default.LoadPath = dlg.FileName;
            Properties.Settings.Default.Save();

            string reportFile = dlg.FileName + "\\" + this.resultFile;
            //result exist
            if (File.Exists(reportFile))
            {
                if (MessageBox.Show("Analysis report file exist. Replace?", "Report exist", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    statusMessage.Text = "Analysis abort!";
                    return;
                }

                File.Delete(reportFile);
            }

            mnuFile.Enabled = false;
            this.UseWaitCursor = true;
            this.clearPictureBoxes();
            Application.DoEvents();

            string[] folders = Directory.GetDirectories(dlg.FileName);
            aggregateDetectorParams para = getDetectorParams();

            Dictionary<string, string> results = new Dictionary<string, string>();
            foreach(string dir in folders)
            {
                try
                {
                    statusMessage.Text = dir;
                    Application.DoEvents();

                    string[] frames = frameFiles(dir);
                    string line = (DEBUGMODE) ? this.imageSummary(frames, para): this.simpleImageSummary(frames,para);
                    if(line.Length>0)
                        results.Add(Path.GetFileName(dir), line);
                }
                catch (Exception ex)
                {
                    string msg = "Failed in analyzing at well : " + dir;
                    msg += Environment.NewLine + ex.Message;
                    msg += Environment.NewLine + Environment.NewLine + "Abort?";

                    if (MessageBox.Show(msg, "Quit analysis?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        this.UseWaitCursor = false;
                        mnuFile.Enabled = true;
                        statusMessage.Text = "Analysis abort!";
                        return;
                    }
                }
            }

            //output result
            if(results.Count>0)
            {
                if(DEBUGMODE)
                {
                    this.saveResultToFile(results, reportFile);
                }
                else
                    this.saveSimpleResultToFile(results, reportFile);


                //MessageBox.Show("Analysis successfully completed");
                statusMessage.Text = "Analysis successfully completed";
                ProcessStartInfo startInfo = new ProcessStartInfo { Arguments = dlg.FileName, FileName = "explorer.exe" };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show("There is no result in the folder" + Environment.NewLine+dlg.FileName);
                statusMessage.Text = "Empty result";
            }

            //unlock operations

            this.UseWaitCursor = false;
            mnuFile.Enabled = true;
        }

        private  void saveResultToFile(Dictionary<string,string> results, string file)
        {
            string context = "well,Bead,Overlap,%Overlap,Hollow,%Hollow,Component,Solid,%Soled,Large,%Large";
            foreach (KeyValuePair<string, string> v in results)
                context += "\n" + v.Key + "," + v.Value;

            File.WriteAllText(file, context);
        }

        private void saveSimpleResultToFile(Dictionary<string, string> results, string file)
        {
            string context = "well,#Bead,#Aggregation";
            foreach (KeyValuePair<string, string> v in results)
                context += "\n" + v.Key + "," + v.Value;

            File.WriteAllText(file, context);
        }

        public string imageSummary(string[] frames,aggregateDetectorParams para)
        {
            string rtn = "";
            aggregateDetector tempDetector;
            int frameCnt = 0;
            int beads = 0;
            int solidBeads = 0;
            int emptyBeads = 0;
            int components = 0;
            int largeComponents = 0;
            int solidComponents = 0;

            foreach(string f in frames)
            {
                tempDetector=aggregateDetector.fromFile(f, para );
                ++frameCnt;
                beads += tempDetector.beads.Length;
                solidBeads += tempDetector.highCoveredBeads.Length;
                emptyBeads += tempDetector.emptyBeads.Length;
                components += tempDetector.ccCount;
                largeComponents += tempDetector.ccLargeAreaCount;
                solidComponents += tempDetector.ccHighCoveredCount;
            }


            rtn = beads.ToString();
            rtn += "," + solidBeads.ToString();
            rtn += (beads <= 0) ? "-" : "," + (solidBeads * 1.0f / beads).ToString("0.000");
            rtn += "," + emptyBeads.ToString();
            rtn += (beads <= 0) ? "-" : "," + (emptyBeads * 1.0f / beads).ToString("0.000");

            rtn += "," + components.ToString();
            rtn += "," + solidComponents.ToString();
            rtn += (components <= 0) ? "-" : "," + (solidComponents * 1.0f / beads).ToString("0.000");
            rtn += "," + largeComponents.ToString();
            rtn += (components <= 0) ? "-" : "," + (largeComponents * 1.0f / beads).ToString("0.000");

            return rtn;
        }
        
        public string simpleImageSummary(string[] frames, aggregateDetectorParams para)
        {
            string rtn = "";
            aggregateDetector tempDetector;
            int frameCnt = 0;
            int beads = 0;
            int solidBeads = 0;
            int emptyBeads = 0;
            int components = 0;
            int largeComponents = 0;
            int solidComponents = 0;

            foreach (string f in frames)
            {
                tempDetector = aggregateDetector.fromFile(f, para);
                ++frameCnt;
                beads += tempDetector.beads.Length;
                solidBeads += tempDetector.caWithSolidBead.Count;
            }


            rtn = beads.ToString();
            rtn += "," + solidBeads.ToString();
            return rtn;
        }
        private string[] frameFiles(string path)
        {
            List<string> rtn = new List<string>();
            foreach (string frame in this.frameIDs)
                if (File.Exists(path + "\\" + frame))
                    rtn.Add(path + "\\" + frame);
            return rtn.ToArray();
        }

        public void initialFrameImageID()
        {
            this.frameIDs = new string[]{
                "01_WL.jpg"
                ,"02_WL.jpg"
                ,"03_WL.jpg"
                ,"05_WL.jpg"
                ,"06_WL.jpg"
                ,"07_WL.jpg"
                ,"08_WL.jpg"
                ,"09_WL.jpg"
                ,"10_WL.jpg"
                ,"11_WL.jpg"
                ,"12_WL.jpg"
                ,"13_WL.jpg"
                ,"14_WL.jpg"
                ,"15_WL.jpg"
                ,"16_WL.jpg"
                ,"17_WL.jpg"
                ,"18_WL.jpg"
                ,"19_WL.jpg"
                ,"21_WL.jpg"
                ,"22_WL.jpg"
                ,"23_WL.jpg"
            };
        }
    }
}
