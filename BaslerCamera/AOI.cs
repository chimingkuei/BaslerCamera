using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaslerCamera
{
    class Algorithm
    {
        public Mat Binarization(Mat src, double threshold)
        {
            Mat grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);
            Mat binaryImg = new Mat();
            Cv2.Threshold(grayImg, binaryImg, threshold, 255, ThresholdTypes.Binary);
            return binaryImg;
        }

        public List<(int classId, float x1, float y1, float x2, float y2)> BoundingBox(Mat src, double threshold, int width, int length, int classId = 0)
        {
            Mat binaryImg = Binarization(src, threshold);
            Cv2.FindContours(binaryImg, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Mat dst = src.Clone();
            List<(int classId, float x1, float y1, float x2, float y2)> boxes = new List<(int classId, float x1, float y1, float x2, float y2)>();
            foreach (var contour in contours)
            {
                Rect boundingRect = Cv2.BoundingRect(contour);
                if (boundingRect.Width > width && boundingRect.Height > length)
                {
                    Cv2.Rectangle(dst, boundingRect, Scalar.Red, 2);
                    float x1 = boundingRect.Left;
                    float y1 = boundingRect.Top;
                    float x2 = boundingRect.Right;
                    float y2 = boundingRect.Bottom;
                    boxes.Add((classId, x1, y1, x2, y2));
                }
            }
            Cv2.ImWrite(@"E:\\DIP Temp\\Image Temp\result.bmp", dst);
            return boxes;
        }

        public void GenerateYoloAnnotation(string filePath, int imageWidth, int imageHeight, List<(int classId, float x1, float y1, float x2, float y2)> boxes)
        {
            List<string> annotations = new List<string>();
            foreach (var box in boxes)
            {
                int classId = box.classId;
                float x1 = box.x1;
                float y1 = box.y1;
                float x2 = box.x2;
                float y2 = box.y2;
                // 計算框的中心點、寬度和高度
                float xCenter = (x1 + x2) / 2;
                float yCenter = (y1 + y2) / 2;
                float width = x2 - x1;
                float height = y2 - y1;
                // 將中心點和尺寸歸一化
                float normXCenter = xCenter / imageWidth;
                float normYCenter = yCenter / imageHeight;
                float normWidth = width / imageWidth;
                float normHeight = height / imageHeight;
                // 格式化 YOLO 標註訊息
                string annotation = $"{classId} {normXCenter.ToString("F6", CultureInfo.InvariantCulture)} {normYCenter.ToString("F6", CultureInfo.InvariantCulture)} {normWidth.ToString("F6", CultureInfo.InvariantCulture)} {normHeight.ToString("F6", CultureInfo.InvariantCulture)}";
                annotations.Add(annotation);
            }
            File.WriteAllLines(filePath, annotations);
            Console.WriteLine("Generate annotation file：" + filePath);
        }

    }

}
