using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;


using Emgu.CV;
using Emgu.CV.Structure;

using System.Drawing;

namespace picodeAggregateDetector
{
    public class aggregateDetector
    {
        public string fileName { get; private set; }

        public Emgu.CV.Image<Gray, Byte> source;
        public Emgu.CV.Image<Gray, Byte> bindaryImage;
        public Emgu.CV.Image<Gray, Byte> dilateErodeImage1;
        public Emgu.CV.Image<Gray, Byte> dilateErodeImage2;

        public Mat labels;
        public int numLabels { get; private set; }
        public CircleF[] beads;
        public CircleF[] highCoveredBeads;
        public CircleF[] emptyBeads;

        public Dictionary<int,componentArea> ccAreas;
        public int ccCount
        {
            get
            {
                if (ccAreas == null)
                    return 0;
                int rtn = 0;
                foreach (KeyValuePair<int, componentArea> ca in ccAreas)
                    if (ca.Value.area >= paras.ccThresholdBead)
                        ++rtn;
                return rtn;
            }
        }
        public int ccHighCoveredCount
        {
            get
            {
                if (ccAreas == null)
                    return 0;
                int rtn = 0;
                foreach (KeyValuePair<int,componentArea> ca in ccAreas)
                    if (ca.Value.area >= paras.ccThresholdBead && ca.Value.coverage >= paras.ccThresholdCoverage)
                        ++rtn;
                return rtn;
            }
        }
        public int ccLargeAreaCount
        {
            get
            {
                if (ccAreas == null)
                    return 0;
                int rtn = 0;
                foreach (KeyValuePair<int, componentArea> ca in ccAreas)
                    if (ca.Value.area >= paras.ccThresholdArea)
                        ++rtn;
                return rtn;
            }
        }

        public Emgu.CV.Image<Gray, Byte> CComponentsImage;
        public Emgu.CV.Image<Gray, Byte> ResultImage;

        public aggregateDetectorParams paras;

        /*
        public Bitmap getBitmap()
        {
            return (this.source !=null)? this.source.ToBitmap(): null;
        }
        /**/

        private aggregateDetector(string name, aggregateDetectorParams para)
        {
            if (!File.Exists(name))
                throw new Exception("Image File is no exist");

            this.source = new Image<Gray, byte>(name);
            this.beads = null;
            this.highCoveredBeads = null;
            this.emptyBeads = null;
            this.doDetect(para);
        }

        public void doDetect(aggregateDetectorParams para)
        {
            if (para == null)
                throw new Exception("Parameter object is null");

            this.paras = para;

            this.bindaryImage = this.source.ThresholdAdaptive(
                this.paras.binaryMaxValue,
                this.paras.binaryAdaptiveType,
                this.paras.binaryThresholdType,
                this.paras.binaryBlockSize,
                this.paras.binaryPara1Value);

            //切換澎漲/侵蝕演算的順序 
            if (this.paras.edOrder == aggregateDetectorParams.erodeDilateOrder.dilateFirst)
            {
                this.dilateErodeImage1 = this.bindaryImage.Dilate(this.paras.DilateIteration); //澎漲
                this.dilateErodeImage2 = this.dilateErodeImage1.Erode(this.paras.ErodeIteration); //侵蝕
            }
            else
            {
                this.dilateErodeImage1 = this.bindaryImage.Erode(this.paras.DilateIteration); //澎漲
                this.dilateErodeImage2 = this.dilateErodeImage1.Dilate(this.paras.ErodeIteration); //侵蝕
            }


            this.labels = new Mat();
            this.numLabels = CvInvoke.ConnectedComponents(this.dilateErodeImage2, labels);

            this.beads = CvInvoke.HoughCircles(
                this.dilateErodeImage2,
                this.paras.circleHoughType,
                this.paras.circleDp,
                this.paras.circleMinDist,
                this.paras.circlePara1,
                this.paras.circlePara2,
                this.paras.circleMinRadius,
                this.paras.circleMaxRadius);
            List<CircleF> thcbs = new List<CircleF>();
            List<CircleF> empbs = new List<CircleF>();


            //Conected components detection
            this.CComponentsImage = labels.ToImage<Gray, byte>();
            this.ccAreas = this.ccCoverages(this.CComponentsImage);

            

            this.ResultImage = this.source.Clone();
            foreach (CircleF c in this.beads)
            {
                if (this.beadCoverRate(c) > this.paras.circleCoverThreshold)
                {
                    //Solid circle
                    this.ResultImage.Draw(c, new Gray(255), 4, Emgu.CV.CvEnum.LineType.AntiAlias);
                    thcbs.Add(c);
                }
                else if (this.beadCoverRate(c) <this.paras.circleEmptyThreshold)
                {
                    //Empty circle
                    this.ResultImage.Draw(
                        new CircleF(c.Center,c.Radius+2), new Gray(255), 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                    empbs.Add(c);
                }
                else
                    //Normal circle
                    this.ResultImage.Draw(
                        new CircleF(c.Center,8)
                        , new Gray(255), 1, Emgu.CV.CvEnum.LineType.AntiAlias);

            }
            this.highCoveredBeads = thcbs.ToArray();
            this.emptyBeads = empbs.ToArray();
        }

        public double ccCoverageForTest(Image<Gray, byte> mask, Image<Gray, byte> Subject)
        {
            if (mask == null || Subject == null || mask.Width != Subject.Width || mask.Height != Subject.Height)
                throw new Exception("Invalid image or image size is not equal");

            int maskCnt = 0;
            int coverCnt = 0;

            for(int x=0; x<mask.Width;++x)
                for(int y=0; y<mask.Height; ++y )
                {
                   if(0<(int) mask[y,x].Intensity)
                    {
                        maskCnt++;
                        if (0 < (int)Subject[y, x].Intensity)
                            coverCnt++;
                    }
                }

            return coverCnt * 1.0 / maskCnt;
        }

        /// <summary>
        /// mask is image of conected components with ID
        /// Subject is original labels
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="Subject"></param>
        /// <returns></returns>
        public Dictionary<int, componentArea> ccCoverages(Image<Gray, byte> ccImg)
        {
            Dictionary<int, componentArea> rtn = new Dictionary<int, componentArea>();
            if (this.labels == null || this.CComponentsImage == null)
                return null;

            Image<Gray,byte> ccImg2=ccImg.Dilate(10).Erode(20).Dilate(10);

            for (int x=0; x<this.labels.Width; ++x)
                for(int y=0; y<labels.Height; ++y)
                {
                    //to gat component id of the pixel
                    int ccLabel = (int)ccImg2[y, x].Intensity;
                    if(ccLabel>0)
                    {
                        if(!rtn.ContainsKey(ccLabel))
                        {
                            rtn.Add(ccLabel, new componentArea());
                            rtn[ccLabel].id = ccLabel;
                            rtn[ccLabel].area = 1;
                        }
                        else
                            rtn[ccLabel].area++;

                        if (ccImg[y, x].Intensity > 0)
                            rtn[ccLabel].covered++;
                            
                    }
                }

            return rtn;
        }

        public static aggregateDetector fromFile(string file, aggregateDetectorParams para=null)
        {
            aggregateDetectorParams tempPara = (para == null) ?
                new aggregateDetectorParams()
                :
                para;
            
                
            try
            {
                return new aggregateDetector(file,tempPara);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /*/
        public int[] highCoversComponents(double threshold, out int coverPixels)
        {
            coverPixels = 0;
            if (this.labels == null)
                return null;

            Dictionary<int,int> cList = new Dictionary<int,int>();
            for(int x=0; x<this.labels.Size.Width; ++x)
                for(int y=0; y<this.labels.Size.Height; ++y)
                {
                    int l = (int)this.CComponentsImage[y, x].Intensity;
                    if (l>0)
                    {
                        if (cList.ContainsKey(l))
                        {
                            cList[l] += 1;
                        }
                        else
                            cList[l] = 1;

                        coverPixels += 1;
                    }
                }

            return cList.Keys.ToArray();
        }
        /**/

        public double beadCoverRate(CircleF c, float reducing= 1.0f)
        {
            Image<Gray, byte> img = this.CComponentsImage;
            int pixelCnt = 0;
            int coverCnt = 0;
            //range of x
            int xStart = (int) Math.Floor(c.Center.X-c.Radius-reducing);
            if (xStart < 0)
                xStart = 0;
            int xEnd = (int)Math.Ceiling(c.Center.X + c.Radius - reducing);
            if (xEnd >= img.Width) 
                xEnd = img.Width - 1;
            //range of y
            int yStart = (int)Math.Floor(c.Center.Y - c.Radius - reducing);
            if (yStart < 0)
                yStart = 0;
            int yEnd = (int)Math.Ceiling(c.Center.Y + c.Radius - reducing);
            if (yEnd >= img.Height)
                yEnd = img.Height - 1;

            double r2 =Math.Pow(c.Radius - reducing, 2);
            for(int x=xStart; x<=xEnd; ++x)
                for(int y=yStart; y<yEnd; ++y)
                    if((Math.Pow(x-c.Center.X,2)+Math.Pow(y-c.Center.Y,2))<r2)
                    {
                        ++pixelCnt;
                        if (0 < (int)img[y,x].Intensity)
                            ++coverCnt;
                    }
            return coverCnt*1.0 / pixelCnt;
        }
    }

    public class aggregateDetectorParams
    {
        public Gray binaryMaxValue;
        public Gray binaryPara1Value;
        public Emgu.CV.CvEnum.AdaptiveThresholdType binaryAdaptiveType;
        public Emgu.CV.CvEnum.ThresholdType binaryThresholdType;
        public int binaryBlockSize;

        /// <summary>
        /// Erode iteration , <= 0 : not to do erode.
        /// </summary>
        public int ErodeIteration;
        /// <summary>
        /// Dilate iteration, <= 0 : not to do dilate
        /// </summary>
        public int DilateIteration;
        /// <summary>
        /// Do erode before dilate or not
        /// </summary>
        public erodeDilateOrder edOrder;

        
        public enum erodeDilateOrder
        {
            erodeFirst,
            dilateFirst
        }

        public Emgu.CV.CvEnum.HoughType circleHoughType;
        public double circleDp;
        public double circleMinDist;
        public double circlePara1;
        public double circlePara2;
        public int circleMinRadius;
        public int circleMaxRadius;
        public double circleCoverThreshold;
        public double circleEmptyThreshold;

        //connected components
        public double ccThresholdCoverage;
        public double ccThresholdArea;
        public int ccThresholdBead;

        //default
        public aggregateDetectorParams()
        {
            this.binaryMaxValue = new Gray(255);
            this.binaryPara1Value = new Gray(5);
            this.binaryAdaptiveType = Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC;
            this.binaryThresholdType = Emgu.CV.CvEnum.ThresholdType.BinaryInv;
            this.binaryBlockSize = 11;
            this.edOrder = erodeDilateOrder.dilateFirst;
            this.ErodeIteration = 0;
            this.DilateIteration = 0;

            this.circleHoughType = Emgu.CV.CvEnum.HoughType.Gradient;
            this.circleDp = 1;
            this.circleMinDist = 5;
            this.circlePara1 = 60;
            this.circlePara2 = 20;
            this.circleMinRadius = 18;
            this.circleMaxRadius = 25;

            this.circleCoverThreshold = 0.8;
            this.circleEmptyThreshold = 0.2;


            this.ccThresholdArea = 1800;
            this.ccThresholdCoverage = 0.8;
            this.ccThresholdBead = 800;
        }
    }

    public class componentArea
    {
        public int id = 0;
        public int area = 1;
        public int covered = 0;
        public double coverage
        {
            get
            {
                return covered * 1.0 / area;
            }
        }
    }
}
