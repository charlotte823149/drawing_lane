using drawing_lane.Data;
using drawing_lane.Function;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace drawing_lane.Control
{

    public partial class ImageViewBox : UserControl
    {
		#region parameter setting
		public string path = "", folder = "", fileName = "";
		public int[] human_count = new int[6] { 0, 0, 0, 0, 0, 0 };
		public int laneNum = 0;
		public int total_laneNum = 0;
		bool newData_flag = false;
		double ratio = 1;
		jsonData json = new jsonData();
		List<Point> tempLane = new List<Point>();

		Polyline polyline0 = new Polyline();
		Polyline polyline1 = new Polyline();
		Polyline polyline2 = new Polyline();
		Polyline polyline3 = new Polyline();
		public ImageViewBox()
		{
			InitializeComponent();
		}
		#endregion

		#region functions for called
		public void setImage()
		{
			reset();
			BitmapImage image = new BitmapImage(new Uri(path + "/" + folder + fileName, UriKind.Absolute));
			ImageBrush imageBrush = new ImageBrush(image);
			canvas.Background = imageBrush;
			canvas.Width = 1280;
			canvas.Height = 720;
			redraw();
			//ratio = 0.4;
			//ZoomOut();
		}

		public int return_laneNum()
        {
			string js = fileName;
			if (folder == "/objectImage/")
            {
				js = fileName.Replace("detection.jpg", "pave.json");
            }
            else
            {
				js = fileName.Replace(".jpg", ".json");
			}
			if (File.Exists(path + "/lane/" + js))
            {
				using (StreamReader read = new StreamReader(path + "/lane/" + js))
				{
					string data = read.ReadToEnd();

					json = JsonConvert.DeserializeObject<jsonData>(data);
					return json.lane.Count;
				}
			}
			return 0;			
		}

		private void resetLaneList()
        {
			if (tempLane.Count > 0)
            {
				tempLane.Reverse();
				double x = 0;
				List<double[]> new_lane = new List<double[]>();
				foreach (Point point in tempLane)
				{
					if (point.X != x)
					{
						x = point.X;
						double[] p = { point.X, point.Y };
						new_lane.Add(p);
					}
				}
				if (json.lane.Count - 1 < laneNum)
                {
					json.lane.Add(new_lane);
				}
                else
                {
					json.lane[laneNum] = new_lane;
				}
				
				tempLane.Clear();
			}
		}

		public void ZoomIn()
        {
			ScaleTransform st = canvas.LayoutTransform as ScaleTransform;
			st = new ScaleTransform(ratio, ratio);
			if (ratio < 2)
			{
				ratio = ratio + 0.1;
			}
			st = new ScaleTransform(ratio, ratio);
            canvas.LayoutTransform = st;
        }

		public void ZoomOut()
        {
			ScaleTransform st = canvas.LayoutTransform as ScaleTransform;
			st = new ScaleTransform(ratio, ratio);
			if (ratio > 0.2)
			{
				ratio = ratio - 0.1;
			}
			st = new ScaleTransform(ratio, ratio);
			canvas.LayoutTransform = st;
		}
		#endregion

		#region drawing
		public void drawCenter(Point LeftPoint, Point RightPoint)
		{
			SolidColorBrush brush = new SolidColorBrush();
			brush.Color = Colors.White;
			Ellipse ellipse = new Ellipse();
			ellipse.Margin = new Thickness((LeftPoint.X + RightPoint.X) / 2 * 0.32 - 5, (LeftPoint.Y + RightPoint.Y) / 2 * 0.4 - 5, 0, 0);
			ellipse.Width = 10;
			ellipse.Height = 10;
			ellipse.Stroke = brush;
			ellipse.StrokeThickness = 10;

			canvas.Children.Add(ellipse);
		}

		private void drawPoint(double x, double y)
		{
			SolidColorBrush brush = new SolidColorBrush();
			brush.Color = Colors.Red;
			Ellipse ellipse = new Ellipse();
			ellipse.Margin = new Thickness(x - 5, y - 5, 0, 0);
			ellipse.Width = 10;
			ellipse.Height = 10;
			ellipse.Stroke = brush;
			ellipse.StrokeThickness = 2;

			canvas.Children.Add(ellipse);
		}

		private void drawPolyline (PointCollection pointCollection, int color)
		{			
            switch (color)
            {
				case 0:
					SolidColorBrush brush0 = new SolidColorBrush();
					brush0.Color = Color.FromArgb(255, 0, 0, 255);
					polyline0.Points = pointCollection;
					polyline0.Stroke = brush0;
					polyline0.StrokeThickness = 5;
					canvas.Children.Add(polyline0);
					break;
				case 1:
					SolidColorBrush brush1 = new SolidColorBrush();
					brush1.Color = Color.FromArgb(255, 18, 164, 31);
					polyline1.Points = pointCollection;
					polyline1.Stroke = brush1;
					polyline1.StrokeThickness = 5;
					canvas.Children.Add(polyline1);
					break;
				case 2:
					SolidColorBrush brush2 = new SolidColorBrush();
					brush2.Color = Color.FromArgb(255, 255, 0, 0);
					polyline2.Points = pointCollection;
					polyline2.Stroke = brush2;
					polyline2.StrokeThickness = 5;
					canvas.Children.Add(polyline2);
					break;
				case 3:
					SolidColorBrush brush3 = new SolidColorBrush();
					brush3.Color = Color.FromArgb(255, 125, 0, 125);
					polyline3.Points = pointCollection;
					polyline3.Stroke = brush3;
					polyline3.StrokeThickness = 5;
					canvas.Children.Add(polyline3);
					break;
			}		
		}		
		#endregion

		#region data add, save, read, delete
		private void drawingDataAdd(Point point)
		{
			tempLane.Add(point);
		}

		public void saveTemp()
		{
			resetLaneList();
			try
            {
				string js = fileName;
				if (folder == "/objectImage/")
				{
					js = fileName.Replace("detection.jpg", "pave.json");
				}
				else
				{
					js = fileName.Replace(".jpg", ".json");
				}
				using (StreamWriter output = new StreamWriter(path + "/lane/" + js))
				{                    
                    string strJson = JsonConvert.SerializeObject(json, Formatting.Indented);
                    output.Write(strJson);
                    output.Close();
                    output.Dispose();
                }
				//canvas.Children.Remove(polyline3);
				//exports.ToImage(canvas, path, fileName, -scrollViewer.HorizontalOffset, -scrollViewer.VerticalOffset);
			}
            catch
            {
				MessageBox.Show("無法儲存json檔");
			}
		}

		private void redraw()
		{
			string js = fileName;
			if (folder == "/objectImage/")
			{
				js = fileName.Replace("detection.jpg", "pave.json");
			}
			else
			{
				js = fileName.Replace(".jpg", ".json");
			}
			if (File.Exists(path + "/lane/" + js))
            {
				try
				{
					reset();
					using (StreamReader read = new StreamReader(path + "/lane/" + js))
					{
						string data = read.ReadToEnd();

						json = JsonConvert.DeserializeObject<jsonData>(data);
						if (json.lane.Count > 0)
						{
							//resetArray(json.lane);
							for (int i = 0; i < json.lane.Count; i++)
							{
								PointCollection pointCollection = new PointCollection();
								for (int j = 0; j < json.lane[i].Count; j++)
								{

									pointCollection.Add(new Point { X = json.lane[i][j][0], Y = json.lane[i][j][1] });
								}
								drawPolyline(pointCollection, i);
							}
						}
					}
				}
				catch
				{
					MessageBox.Show("無法讀取json檔");
				}
			}            
		}

		public void deleteLane()
		{
			if (json.lane.Count - 1 >= laneNum)
			{
				json.lane.RemoveAt(laneNum);
			}
			saveTemp();
			redraw();
		}
        #endregion

        #region Event
        private void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			newData_flag = !newData_flag;
			if (tempLane.Count > 0)
            {
				saveTemp();
				redraw();
			}
		}

		private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if(newData_flag == true)
            {
				Point current_p = e.GetPosition(canvas);
				if (current_p.X < canvas.Width && current_p.Y < canvas.Height)
				{
					drawPoint(current_p.X, current_p.Y);
					drawingDataAdd(current_p);
				}
			}			
		}
		#endregion

		#region resetting, cal lane
		private void reset()
		{
			canvas.Children.Clear();
		}
		#endregion		
    }
}
