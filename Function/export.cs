using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace drawing_lane.Function
{
    public class export
    {
        public void ToImage(Canvas canvas, string path, string fileName, double hor_offset, double ver_offset)
        {            
            canvas.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvas, new Vector());
            path = path + @"\temp\" + fileName;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.DesiredSize.Width, (int)canvas.DesiredSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);
            DrawingVisual dvInk = new DrawingVisual();
            DrawingContext dcInk = dvInk.RenderOpen();
            dcInk.DrawRectangle(canvas.Background, null, new Rect(0d, 0d, canvas.Width, canvas.Height));
            dcInk.Close();

            FileStream fs = File.Open(path, FileMode.OpenOrCreate);//save bitmap to file
            JpegBitmapEncoder encoder1 = new JpegBitmapEncoder();
            encoder1.Frames.Add(BitmapFrame.Create(rtb));
            encoder1.Save(fs);
            fs.Close();
            Vector old_offset = new Vector(hor_offset, ver_offset);
            canvas.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvas, old_offset);
        }

        public void DrawImage(Canvas canvas1, string path, string fileName, int iii)
        {
            canvas1.Children.RemoveAt(iii);
            /*
            List<drawingData> drawingDatas = new List<drawingData>();            
            using (StreamReader input = new StreamReader(path + "/temp/" + fileName + ".txt"))
            {
                string str;
                int id = 0;
                while ((str = input.ReadLine()) != null)
                {
                    string[] ss = str.Split(' ');
                    if (ss[0] != "[" && ss[0] != "(" && ss[0] != "*")
                    {
                        drawingDatas.Add(new drawingData()
                        {
                            damageType = ss[0],
                            damageLevel = ss[1],
                            drawType = Convert.ToInt32(ss[2]),
                        });
                        for (int i = 6; i < ss.Length - 1; i += 2)
                        {
                            drawingDatas[id].points.Add(new Point()
                            {
                                X = Convert.ToDouble(ss[i]),
                                Y = Convert.ToDouble(ss[i + 1])
                            });
                            int c = drawingDatas[id].points.Count;

                            SolidColorBrush brush = new SolidColorBrush();
                            brush.Color = Colors.Red;
                            Ellipse ellipse = new Ellipse();
                            ellipse.Margin = new Thickness(drawingDatas[id].points[c - 1].X - 5, drawingDatas[id].points[c - 1].Y - 5, 0, 0);
                            ellipse.Width = 10;
                            ellipse.Height = 10;
                            ellipse.Stroke = brush;
                            ellipse.StrokeThickness = 5;
                            canvas1.Children.Add(ellipse);
                        }
                        if (drawingDatas[id].drawType == 0 && drawingDatas[id].points.Count == 2)
                        {
                            SolidColorBrush brush = new SolidColorBrush();
                            brush.Color = Colors.Blue;
                            Line line = new Line();
                            line.X1 = drawingDatas[id].points[0].X;
                            line.Y1 = drawingDatas[id].points[0].Y;
                            line.X2 = drawingDatas[id].points[1].X;
                            line.Y2 = drawingDatas[id].points[1].Y;
                            line.Stroke = brush;
                            line.StrokeThickness = 2;
                            canvas1.Children.Add(line);
                        }
                        else if (drawingDatas[id].drawType == 1 && drawingDatas[id].points.Count >= 2)
                        {
                            SolidColorBrush brush = new SolidColorBrush();
                            brush.Color = Colors.Yellow;
                            Polyline polyline = new Polyline();
                            PointCollection pointCollection = new PointCollection();
                            foreach (Point point in drawingDatas[id].points)
                            {
                                pointCollection.Add(point);
                            }
                            polyline.Points = pointCollection;
                            polyline.Stroke = brush;
                            polyline.StrokeThickness = 2;
                            canvas1.Children.Add(polyline);
                        }
                        else if (drawingDatas[id].drawType == 2 && drawingDatas[id].points.Count >= 3)
                        {
                            SolidColorBrush brush = new SolidColorBrush();
                            brush.Color = Colors.White;
                            Polygon polygon = new Polygon();
                            PointCollection pointCollection = new PointCollection();
                            foreach (Point point in drawingDatas[id].points)
                            {
                                pointCollection.Add(point);
                            }
                            polygon.Points = pointCollection;
                            polygon.Stroke = brush;
                            polygon.StrokeThickness = 2;
                            canvas1.Children.Add(polygon);
                        }
                        id++;
                    }
                }
            }
            */
            canvas1.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvas1, new Vector());            
            path = path + @"\temp\" + fileName;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas1.Width, (int)canvas1.Height, 96d, 96d, PixelFormats.Default);
			rtb.Render(canvas1);
			DrawingVisual dvInk = new DrawingVisual();
			DrawingContext dcInk = dvInk.RenderOpen();
			dcInk.DrawRectangle(canvas1.Background, null, new Rect(0d, 0d, canvas1.Width, canvas1.Height));
			dcInk.Close();

			FileStream fs = File.Open(path, FileMode.OpenOrCreate);//save bitmap to file
			PngBitmapEncoder encoder1 = new PngBitmapEncoder();
			encoder1.Frames.Add(BitmapFrame.Create(rtb));
			encoder1.Save(fs);
			fs.Close();
		}
	}
}
