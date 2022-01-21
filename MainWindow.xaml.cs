using System;
using System.Windows;
using drawing_lane.Function;
using System.Windows.Input;
using System.Windows.Controls;
using System.IO;
using System.Collections.Generic;
using drawing_lane.Data;
using Newtonsoft.Json;

namespace drawing_lane
{
    public partial class MainWindow : Window
    {     
        private static void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        #region parameter setting
        string path = "";
        string folder_name = "";
        int oldfilenum = -1;
        List<aiData> aiDataList = new List<aiData>();
        basicData basicDatas = new basicData();
        cal_fuction cal = new cal_fuction();
        List<humanCountData> human = new List<humanCountData>();

        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region menu event       
        private void readFolder_Click(object sender, RoutedEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            #region resetting
            path = "";
            folder_name = "";
            human.Clear();
            humanCount_listView.Items.Clear();
            oldfilenum = -1;
            aiDataList.Clear();
            basicDatas = new basicData();
            fileName_Combo.Items.Clear();
            fileName_Combo.SelectedIndex = -1;
            oldfilenum = -1;
            #endregion

            //open choose file window
            getData getdata = new getData();
            path = getdata.SelectedPath();            

            string[] str = path.Split('\\');
            folder_name = str[str.Length - 1];

            #region check files and folders exist
            if (!Directory.Exists(path + "/lane/"))
            {
                MessageBox.Show("沒有lane資料夾");
                return;
            }            
            if (!Directory.Exists(path + "/image/"))
            {
                MessageBox.Show("沒有image資料夾");
                return;
            }
            if (!Directory.Exists(path + "/objectImage/"))
            {
                MessageBox.Show("沒有objectImage資料夾");
                return;
            }            
            #endregion

            //read image data names
            string[] unObject_list = Directory.GetFiles(path + "/image/");
            foreach (string item in unObject_list)
            {
                if (item.Contains("desktop.ini"))
                {
                    continue;
                }
                string[] path_name = item.Split('/');
                aiDataList.Add(new aiData
                {
                    OriginalName = path_name[2]                    
                });
            }

            //getting damage list
            string[] damage_list = Directory.GetFiles(path + "/JSON/");
            foreach (string item in damage_list)
            {
                string fileExt = Path.GetExtension(item);
                if (fileExt == ".json")
                {
                    using (StreamReader read = new StreamReader(item))
                    {
                        string data = read.ReadToEnd();
                        if (data != "")
                        {
                            aiData j = new aiData();
                            j = JsonConvert.DeserializeObject<aiData>(data);

                            int index = aiDataList.FindIndex(x => x.OriginalName == j.OriginalName);
                            if (index != -1) //damage on same image
                            {
                                aiDataList[index] = j;
                                aiDataList[index].PCI = new List<double>();
                                if (j.Shape.Count != 0)
                                {
                                    foreach (damageData d in aiDataList[index].Shape)
                                    {
                                        d.Category = cal.ToChineseCate(d.Category);
                                        d.Level = cal.ToChineseLevel(d.Level);
                                    }
                                }
                                else
                                {
                                    aiDataList[index].AfterName = null;
                                }
                            }
                        }
                    }
                }                
            }

            //using Date to sort
            aiDataList.Sort((x, y) => x.Date.CompareTo(y.Date));

            #region basic setting
            foreach (aiData aiData in aiDataList)
            {
                fileName_Combo.Items.Add(aiData.OriginalName);
            }
            image_show.path = path;
            if (aiDataList.Count > 0)
            {
                EnabledItem();
            }
            fileName_Combo.SelectedIndex = 0;
            laneType_Combo.SelectedIndex = 0;

            human.Add(new humanCountData
            {
                Category = "人手孔蓋未平順",
                Number = 0
            });
            human.Add(new humanCountData
            {
                Category = "鱷魚狀裂縫",
                Number = 0
            });
            human.Add(new humanCountData
            {
                Category = "線狀裂縫",
                Number = 0
            });
            human.Add(new humanCountData
            {
                Category = "補綻",
                Number = 0
            });
            human.Add(new humanCountData
            {
                Category = "坑洞",
                Number = 0
            });
            human.Add(new humanCountData
            {
                Category = "剝脫",
                Number = 0
            });
            addingHuman();
            #endregion
        }

        public void saveProject_Click(object sender, RoutedEventArgs e)
        {
            if (fileName_Combo.SelectedIndex != -1)
            {
                image_show.saveTemp();
                CalLane(fileName_Combo.SelectedIndex);
            }
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            if (roadType_Combo.SelectedIndex == -1)
            {
                MessageBox.Show("請選擇道路類型");
                return;
            }

            try
            {
                //check basic data setting
                basicDatas.Stake = Convert.ToInt32(stake_TextBox.Text.ToString());
                basicDatas.Space = Convert.ToInt32(space_TextBox.Text.ToString());
                basicDatas.LaneWidth = Convert.ToDouble(laneWide_TextBox.Text.ToString());
                basicDatas.LaneLongth = Convert.ToDouble(laneLongth_TextBox.Text.ToString());
                if (roadType_Combo.SelectedIndex == 0)
                {
                    basicDatas.RoadName = city_TextBox.Text.ToString() + cityRoad_TextBox.Text.ToString();
                    basicDatas.Start = cityStart_TextBox.Text.ToString();
                    basicDatas.End = cityEnd_TextBox.Text.ToString();
                }
                else if (roadType_Combo.SelectedIndex > 0)
                {
                    basicDatas.RoadName = country_TextBox.Text.ToString();
                    basicDatas.Start = countryStart_TextBox.Text.ToString();
                    basicDatas.End = countryEnd_TextBox.Text.ToString();
                }                
            }
            catch
            {
                MessageBox.Show("基本資訊輸入不完整");
                return;
            }

            for (int i = 0; i < aiDataList.Count; i++)
            {
                if (aiDataList[i].Shape != null)
                {
                    //sort damages by lane
                    aiDataList[i].Shape.Sort((x, y) => x.Lane.CompareTo(y.Lane));
                    aiDataList[i] = cal.CalLanePlace(path, aiDataList[i]);
                    aiDataList[i].PCI.Clear();
                    for (int j = 0; j <= aiDataList[i].LaneNumber; j++)
                    {
                        aiDataList[i].PCI.Add(cal.PCI(j, aiDataList[i].Shape, Convert.ToDouble(laneWide_TextBox.Text), Convert.ToDouble(laneLongth_TextBox.Text)));
                    }
                }
            }

            string[] temp = path.Split(new[] { "\\" }, StringSplitOptions.None);
            string fileName = temp[temp.Length - 1];
            string savepath = path + "\\" + fileName + ".xlsx";            
            bool IsExists = File.Exists(savepath);
            if (IsExists)
            {
                try
                {
                    Stream s = File.Open(savepath, FileMode.Open, FileAccess.Read, FileShare.None);
                    s.Close();
                    File.Delete(savepath);
                }
                catch (Exception)
                {
                    MessageBox.Show("檔案被佔用！請關閉檔案再試一次");
                    return;
                }
            }
            ProgressWindow window = new ProgressWindow(path, fileName, aiDataList, basicDatas);
            window.Show();
            window.Exporting();
        }

        private void explanation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("1. 資料夾配置\n" +
                "image = 所有照片的原始檔\n" +
                "objectImage: 有被辨識到破壞的辨識後圖片\n" +
                "lane: 車道辨識後用以儲存車道線資訊的資料夾\n" +
                "JSON: 重新調整過的破壞json檔資料夾\n" +
                "DetectInfo_yyyy_mm_dd.txt: 裡面會包含拍攝時間、車號、辨識前照片名、辨識後照片名、破壞長度、破壞寬度、破壞面積、程度、經度、緯度、破壞種類、[左上x座標, 左上y座標, 右上x座標, 右上y座標]及標案號碼\n\n" +
                "2.軟體功能簡介\n" +
                "A.上方資訊區\n" +
                "路徑來源: 顯示目前顯示圖片的電腦路徑位置\n" +
                "圖名: 下拉式選單，可以改變目前顯示的圖片\n" +
                "道路類型: 有市區道路和省縣鄉道、隧道、橋樑兩種，選市區道路就可以填寫市區道路格子內的資訊，反之亦然\n" +
                "!!!出報告時必須要將舊的excel表格關閉，並且需選取道路類型並填寫格子內的資訊，不可留空\n" +
                "車道線數量: 可選擇車道線數量\n" +
                "破壞資訊表: 表格內會顯示此張照片被辨識到的資訊，依序是破壞類型、破壞程度、線段長、線段寬、面積和所在的車道，最底為計算出的各車道的PCI值\n" +
                "(車道會在畫了車道線，並且按下儲存後會再次計算並改變)\n" +
                "車道的表示方式: 0 (車道線1) 1 (車道線2) 2 (車道線3) 3 (車道線4) 4\n" +
                "樁號: 可自行設定起始樁號和間隔\n" +
                "車道線: 車道線數量改變後即可多畫幾條車道線\n" +
                "車道寬度 / 長度: 每張照片的車道寬和長度，用來計算PCI值\n\n" +
                "B.下方顯示區\n" +
                "圖片: 圖片會顯示出已辨識後的照片或是沒有偵測到破壞的照片原始檔，可以點選車道來畫車道線\n" +
                "右方欄位為人工計數區，在項目上點左鍵增加數字，點右鍵減少數字\n" +
                "!!!人工計數區內的數字不會被記錄，重讀資料夾後就會遺失\n" +
                "畫圖: 在圖片內點下滑鼠左鍵開始畫線、點下滑鼠右鍵停止畫線\n\n" +
                "3.快捷鍵\n" +
                "F1~F4: 切換車道線\n" +
                "ctrl + S: 儲存當前圖片的畫出的車道線到json資料夾中\n" +
                "ctrl + C: 按下後即可使用方向鍵[左]、[右]來切換上一張或下一張圖片\n" +
                "ctrl + Z: 刪除目前選取的車道線\n" +
                "[ + ]: 放大圖片\n" +
                "[ - ]: 縮小圖片");
        }

        #endregion

        #region shortcut setting
        public void CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        public void ChooseFile(object sender, RoutedEventArgs e)
        {
            fileName_Combo.Focus();
        }

        public void DeleteLane(object sender, RoutedEventArgs e)
        {
            image_show.deleteLane();
            CalLane(fileName_Combo.SelectedIndex);
        }

        #region zoom
        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            image_show.ZoomIn();
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            image_show.ZoomOut();
        }
        #endregion

        #region lane
        private void F1(object sender, RoutedEventArgs e)
        {
            if (lane0.IsEnabled)
            {
                lane0.IsChecked = true;
                image_show.laneNum = 0;
            }
        }

        private void F2(object sender, RoutedEventArgs e)
        {
            if (lane1.IsEnabled)
            {
                lane1.IsChecked = true;
                image_show.laneNum = 1;
            }
        }

        private void F3(object sender, RoutedEventArgs e)
        {
            if (lane2.IsEnabled)
            {
                lane2.IsChecked = true;
                image_show.laneNum = 2;
            }
        }

        private void F4(object sender, RoutedEventArgs e)
        {
            if (lane3.IsEnabled)
            {
                lane3.IsChecked = true;
                image_show.laneNum = 3;
            }
        }
        #endregion

        #endregion

        #region Selected Changed
        private void fileName_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fileName_Combo.SelectedItem != null)
            {
                int index = fileName_Combo.SelectedIndex;
                if (oldfilenum != -1)
                {
                    listView.Items.Clear();
                    image_show.saveTemp();
                }

                if (aiDataList[index].AfterName == null)
                {
                    image_show.folder = "/image/";
                    image_show.fileName = aiDataList[index].OriginalName;
                }
                else
                {
                    image_show.folder = "/objectImage/";
                    image_show.fileName = aiDataList[index].AfterName;
                    CalLane(index);
                }
                try
                {
                    image_show.setImage();
                    oldfilenum = Convert.ToInt32(fileName_Combo.SelectedIndex);
                    path_Textbox.Text = path + @"\" + fileName_Combo.SelectedItem.ToString();
                }
                catch
                {
                    MessageBox.Show("無圖片檔");
                    fileName_Combo.SelectedIndex = -1;
                }
            }            
        }
        
        private void roadType_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (roadType_Combo.SelectedIndex == 0)
            {
                if (city_Group != null && countryEnd_TextBox != null)
                {
                    city_Group.IsEnabled = true;
                    country_Group.IsEnabled = false;
                }                
            }
            else if (roadType_Combo.SelectedIndex > 0)
            {
                city_Group.IsEnabled = false;
                country_Group.IsEnabled = true;
            }
        }

        private void laneType_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            basicDatas.LaneType = laneType_Combo.SelectedIndex;
            switch(laneType_Combo.SelectedIndex)
            {               
                case 0:
                    lane0.IsEnabled = true;
                    lane1.IsEnabled = false;
                    lane2.IsEnabled = false;
                    lane3.IsEnabled = false;
                    lane0.IsChecked = true;
                    image_show.laneNum = 0;
                    break;
                case 1:
                    lane0.IsEnabled = true;
                    lane1.IsEnabled = true;
                    lane2.IsEnabled = false;
                    lane3.IsEnabled = false;
                    lane0.IsChecked = true;
                    image_show.laneNum = 0;
                    break;
                case 2:
                    lane0.IsEnabled = true;
                    lane1.IsEnabled = true;
                    lane2.IsEnabled = true;
                    lane3.IsEnabled = false;
                    lane0.IsChecked = true;
                    image_show.laneNum = 0;
                    break;
                case 3:
                    lane0.IsEnabled = true;
                    lane1.IsEnabled = true;
                    lane2.IsEnabled = true;
                    lane3.IsEnabled = true;
                    lane0.IsChecked = true;
                    image_show.laneNum = 0;
                    break;
            }            
        }

        private void lane_CheckedChanged(object sender, RoutedEventArgs e)
        {
            RadioButton ra = (RadioButton)sender;
            image_show.laneNum = Convert.ToInt32(ra.Content.ToString().Substring(2, 1)) - 1;
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] temp = Convert.ToString(datePicker.SelectedDate).Split(' ');
            basicDatas.Date = temp[0];
        }
        
        private void humanCount_listView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (humanCount_listView.SelectedIndex >= 0)
            {
                int index = humanCount_listView.SelectedIndex;
                if (e.ChangedButton == MouseButton.Left)
                {
                    human[index].Number++;
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    human[index].Number--;
                }

                humanCount_listView.Items.Clear();
                addingHuman();
            }
        }
        #endregion

        #region functions for called
        private void EnabledItem()
        {
            //menu
            saveProject.IsEnabled = true;
            export_menu.IsEnabled = true;
            //on grid1
            fileName_Combo.IsEnabled = true;
            //on grid2
            image_show.IsEnabled = true;
        }

        private void DisabledItem()
        {
            //menu
            saveProject.IsEnabled = false;
            export_menu.IsEnabled = false;
            //on grid1
            fileName_Combo.IsEnabled = false;
            //on grid2
            image_show.IsEnabled = false;
        }

        private void CalLane(int index)
        {
            if (aiDataList[index].Shape != null)
            {
                listView.Items.Clear();
                aiDataList[index] = cal.CalLanePlace(path, aiDataList[index]);
                foreach (damageData damage in aiDataList[index].Shape)
                {
                    listView.Items.Add(damage);
                }                

                aiDataList[index].PCI.Clear();
                for (int i = 0; i <= image_show.return_laneNum(); i++)
                {
                    double PCI = cal.PCI(i, aiDataList[index].Shape, Convert.ToDouble(laneWide_TextBox.Text), Convert.ToDouble(laneLongth_TextBox.Text));
                    aiDataList[index].PCI.Add(PCI);
                    if (PCI != 100)
                    {
                        listView.Items.Add(new damageData
                        {
                            Category = "PCI",
                            Level = "車道" + Convert.ToString(i),
                            Area = PCI,
                            Lane = i
                        });
                    }                    
                }                
            }            
        }

        private void addingHuman()
        {
            foreach(humanCountData h in human)
            {
                humanCount_listView.Items.Add(h);
            }
        }
        #endregion

        
    }
}
