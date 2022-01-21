using System.Collections.Generic;
using Window = System.Windows.Window;
using Color = System.Drawing.Color;
using drawing_lane.Data;
using System.IO;
using System;
using Action = System.Action;
using System.Threading.Tasks;
using System.Windows;
using drawing_lane.Function;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml.Drawing.Chart;

namespace drawing_lane
{
    public partial class ProgressWindow : Window
    {
        string path = "", excelName = "";
        List<aiData> aiDatas = new List<aiData>();
        basicData basicData = new basicData();
        List<string> record = new List<string>();
        cal_fuction cal = new cal_fuction();

        public ProgressWindow(string path, string excelName, List<aiData> aiDatas, basicData basicData)
        {
            InitializeComponent();
            this.path = path;
            this.aiDatas = aiDatas;
            this.excelName = excelName;
            this.basicData = basicData;
        }

        public async void Exporting()
        {
            int imgSize = 95; //pixel
            int cellHeight = 84;
            Action<double> bindProgress = value => bar.Value = value;

            IProgress<double> progress = new Progress<double>(bindProgress);

            Action growProgress =
                () =>
                {
                    //fondation setting
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 關閉新許可模式通知
                    //save and release resource
                    string savepath = path + "\\" + excelName + ".xlsx";
                    var file = new FileInfo(savepath);
                    using (var excel = new ExcelPackage())
                    {
                        #region setting
                        int start = 0, damage_start = 0;
                        List<double> pci = new List<double>();
                        #endregion

                        #region datasheet
                        #region basic setting
                        var data_ws = excel.Workbook.Worksheets.Add("資料");                        

                        data_ws.Cells[1, 1].Value = "日期";
                        data_ws.Cells[2, 1].Value = basicData.Date;
                        data_ws.Cells[1, 2].Value = "省鄉道/市區道路名稱";
                        data_ws.Cells[2, 2].Value = basicData.RoadName;
                        data_ws.Cells[1, 3].Value = "起始點、終止點";
                        data_ws.Cells[2, 3].Value = basicData.Start + "、" + basicData.End;
                        data_ws.Cells[1, 4].Value = "車行方向";
                        data_ws.Cells[2, 4].Value = "順向";
                        data_ws.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid; //一定要加這行..不然會報錯
                        data_ws.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 217, 196));

                        data_ws.Cells[3, 1].Value = "里程(公里)";
                        data_ws.Cells[3, 2].Value = "圖片名";
                        data_ws.Cells[3, 3].Value = "原始圖片";
                        data_ws.Cells[3, 4].Value = "點圖";
                        data_ws.Cells[3, 5].Value = "經緯度";
                        data_ws.Cells[3, 6].Value = "樁號";
                        data_ws.Cells[3, 7].Value = "車道";
                        data_ws.Cells[3, 8].Value = "PCI";
                        data_ws.Cells[3, 9].Value = "破壞類型";
                        data_ws.Cells[3, 10].Value = "破壞程度";
                        data_ws.Cells[3, 11].Value = "破壞長度";
                        data_ws.Cells[3, 12].Value = "破壞寬度";
                        data_ws.Cells[3, 13].Value = "破壞面積";
                        data_ws.Cells[3, 1, 3, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        data_ws.Cells[3, 1, 3, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(238, 236, 225));
                        data_ws.Cells["A1:M3"].AutoFitColumns();
                        data_ws.Column(2).Width = 54;
                        data_ws.Column(4).Width = 18;
                        data_ws.Column(5).Width = 32;
                        progress.Report(1);
                        #endregion

                        #region export data

                        damage_start = 4;
                        for (int i = 0; i < aiDatas.Count; i++)
                        {
                            start = damage_start;
                            //only export aiData have damages
                            if (aiDatas[i].Shape.Count != 0 && aiDatas[i].Shape != null)
                            {
                                //adding aiData
                                double stake = basicData.Stake + basicData.Space * i * 0.001;
                                data_ws.Cells[start, 1].Value = cal.StakeString(stake);
                                data_ws.Cells[start, 2].Value = aiDatas[i].OriginalName + "_" + Convert.ToString(aiDatas[i].LaneNumber);
                                data_ws.Cells[start, 5].Value = aiDatas[i].Longtitude + " - " + aiDatas[i].Latitude;
                                data_ws.Cells[start, 6].Value = stake;
                                double height = cellHeight / aiDatas[i].Shape.Count;

                                //go through every lane
                                for (int k = 0; k <= aiDatas[i].LaneNumber; k++)
                                {
                                    List<damageData> a = aiDatas[i].Shape.FindAll(x => x.Lane.Equals(k));

                                    if (a.Count != 0)
                                    {                                        
                                        //adding PCI
                                        data_ws.Cells[damage_start, 7].Value = k;
                                        double pp = aiDatas[i].PCI[k];
                                        data_ws.Cells[damage_start, 8].Value = pp;

                                        //adding damages
                                        foreach (damageData dama in a)
                                        {                                            
                                            //set cell height
                                            data_ws.Row(damage_start).Height = height;

                                            //write damage data
                                            data_ws.Cells[damage_start, 9].Value = dama.Category;
                                            data_ws.Cells[damage_start, 10].Value = dama.Level;
                                            data_ws.Cells[damage_start, 11].Value = dama.Length;
                                            data_ws.Cells[damage_start, 12].Value = dama.Width;
                                            data_ws.Cells[damage_start, 13].Value = dama.Area;
                                            damage_start += 1;
                                        }

                                        if (a.Count > 1)
                                        {
                                            //mergeing cells if this lane have more than one damages
                                            data_ws.Cells[start, 7, damage_start - 1, 7].Merge = true;
                                            data_ws.Cells[start, 8, damage_start - 1, 8].Merge = true;
                                        }
                                    }
                                }

                                //add photos
                                Image img = Image.FromFile(path + @"\image\" + aiDatas[i].OriginalName);
                                var origin_img = data_ws.Drawings.AddPicture(path + @"\image\" + aiDatas[i].OriginalName, img);
                                origin_img.SetSize(imgSize, imgSize);
                                origin_img.SetPosition(start - 1, 0, 2, 0);

                                img = Image.FromFile(path + @"\objectImage\" + aiDatas[i].AfterName);
                                var after_img = data_ws.Drawings.AddPicture(path + @"\objectImage\" + aiDatas[i].AfterName, img);
                                after_img.SetSize(imgSize, imgSize);
                                after_img.SetPosition(start - 1, 0, 3, 0);

                                //mergeing cells
                                data_ws.Cells[start, 1, damage_start - 1, 1].Merge = true;
                                data_ws.Cells[start, 2, damage_start - 1, 2].Merge = true;
                                data_ws.Cells[start, 3, damage_start - 1, 3].Merge = true;
                                data_ws.Cells[start, 4, damage_start - 1, 4].Merge = true;
                                data_ws.Cells[start, 5, damage_start - 1, 5].Merge = true;
                                data_ws.Cells[start, 6, damage_start - 1, 6].Merge = true;
                            }

                            if (i > 1 && i < 98)
                            {
                                progress.Report(i);
                            }
                        }
                        #endregion
                        data_ws.Cells[1, 1, start, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        data_ws.Cells[3, 6, start, 13].AutoFitColumns();
                        data_ws.Cells.Style.Font.Name = "微軟正黑體";
                        data_ws.Cells.Style.Font.Size = 12;
                        #endregion

                        #region IRIPCI worksheet
                        var IRIPCI_ws = excel.Workbook.Worksheets.Add("PCI統計");
                        IRIPCI_ws.Cells[1, 1].Value = "PCI統計";
                        IRIPCI_ws.Cells["A1:A2"].Merge = true;
                        IRIPCI_ws.Cells[1, 2].Value = "100~90";
                        IRIPCI_ws.Cells[2, 2].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=100\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=90\")";
                        IRIPCI_ws.Cells[1, 3].Value = "90~80";
                        IRIPCI_ws.Cells[2, 3].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=90\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=80\")";
                        IRIPCI_ws.Cells[1, 4].Value = "80~70";
                        IRIPCI_ws.Cells[2, 4].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=80\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=70\")";
                        IRIPCI_ws.Cells[1, 5].Value = "70~60";
                        IRIPCI_ws.Cells[2, 5].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=70\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=60\")";
                        IRIPCI_ws.Cells[1, 6].Value = "60~50";
                        IRIPCI_ws.Cells[2, 6].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=60\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=50\")";
                        IRIPCI_ws.Cells[1, 7].Value = "50~40";
                        IRIPCI_ws.Cells[2, 7].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=50\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=40\")";
                        IRIPCI_ws.Cells[1, 8].Value = "40~30";
                        IRIPCI_ws.Cells[2, 8].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=40\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=30\")";
                        IRIPCI_ws.Cells[1, 9].Value = "30~20";
                        IRIPCI_ws.Cells[2, 9].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=30\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=20\")";
                        IRIPCI_ws.Cells[1, 10].Value = "20~10";
                        IRIPCI_ws.Cells[2, 10].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=20\")-COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=10\")";
                        IRIPCI_ws.Cells[1, 11].Value = "10以下";
                        IRIPCI_ws.Cells[2, 11].Formula = "COUNTIF(資料!H4:H" + (damage_start - 1).ToString() + ",\"<=10\")";
                        IRIPCI_ws.Cells[1, 12].Value = "平均";
                        IRIPCI_ws.Cells[2, 12].Formula = "AVERAGE(資料!H4:H" + (damage_start - 1).ToString() + ")";
                        IRIPCI_ws.Cells["A1:L2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        IRIPCI_ws.Cells["A1:L2"].AutoFitColumns();

                        var bar = (ExcelBarChart)IRIPCI_ws.Drawings.AddChart("BarChart", eChartType.ColumnClustered);
                        bar.SetSize(300, 300);
                        bar.SetPosition(10, 10);
                        bar.Title.Text = "PCI統計";
                        bar.Legend.Remove();
                        bar.Series.Add(ExcelRange.GetAddress(2, 2, 2, 11), ExcelRange.GetAddress(1, 2, 1, 11));
                        bar.SetSize(600, 300);
                        bar.SetPosition(50, 10);

                        IRIPCI_ws.Cells.Style.Font.Name = "微軟正黑體";
                        IRIPCI_ws.Cells.Style.Font.Size = 12;
                        progress.Report(98);
                        #endregion

                        #region category worksheet
                        var type_ws = excel.Workbook.Worksheets.Add("破壞統計");
                        type_ws.Cells[1, 1].Value = "破壞統計";
                        type_ws.Cells["A1:A2"].Merge = true;
                        type_ws.Cells[1, 2].Value = "人手孔蓋未平順";
                        type_ws.Cells[2, 2].Formula = "COUNTIFS(資料!I4:I" + (damage_start - 1).ToString() + ", B1)";
                        type_ws.Cells[1, 3].Value = "鱷魚狀裂縫";
                        type_ws.Cells[2, 3].Formula = "COUNTIFS(資料!I4:I" + (damage_start - 1).ToString() + ", C1)";
                        type_ws.Cells[1, 4].Value = "線狀裂縫";
                        type_ws.Cells[2, 4].Formula = "COUNTIFS(資料!I4:I" + (damage_start - 1).ToString() + ", D1)";
                        type_ws.Cells[1, 5].Value = "補綻";
                        type_ws.Cells[2, 5].Formula = "COUNTIFS(資料!I4:I" + (damage_start - 1).ToString() + ", E1)";
                        type_ws.Cells[1, 6].Value = "坑洞";
                        type_ws.Cells[2, 6].Formula = "COUNTIFS(資料!I4:I" + (damage_start - 1).ToString() + ", F1)";
                        type_ws.Cells[1, 7].Value = "剝脫";
                        type_ws.Cells[2, 7].Formula = "COUNTIFS(資料!I4:I" + (damage_start - 1).ToString() + ", G1)";
                        type_ws.Cells["A1:G2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        type_ws.Cells["A1:G2"].AutoFitColumns();
                        type_ws.Column(1).Width = 11;

                        ExcelPieChart pie = type_ws.Drawings.AddChart("PieChart", eChartType.Pie) as ExcelPieChart;
                        pie.Title.Text = "破壞統計";
                        pie.Series.Add(ExcelRange.GetAddress(2, 2, 2, 7), ExcelRange.GetAddress(1, 2, 1, 7));
                        pie.SetSize(450, 300);
                        pie.SetPosition(50, 10);

                        type_ws.Cells.Style.Font.Name = "微軟正黑體";
                        type_ws.Cells.Style.Font.Size = 12;
                        progress.Report(99);
                        #endregion
                        excel.SaveAs(file);
                        progress.Report(100);
                    }                    
                    MessageBox.Show("匯出成功");
                };
            await Task.Run(growProgress);            
            this.Close();
        }
    }
}
