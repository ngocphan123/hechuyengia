using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SbsSW.SwiPlCs;

namespace PrologConect
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        List<string> dauhieu = new List<string>();
        List<string> loimay = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }
        //khoi tao prolog
        private void PlInit(string file)
        {
            if (PlEngine.IsInitialized)//kiem tra neu upload file khác vào PlEngine
            {
                if (XtraMessageBox.Show(this, "Cần unload PlEngine trước khi load file khác?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                PlEngine.PlCleanup();
            }
            string[] args = { "-q", "-f", file };
            PlEngine.Initialize(args);//Khởi tạo SWI-Prolog va doc ghi 
            //status.Caption = "Load thành công: " + file;
            XtraMessageBox.Show(this, "Load thành công: " + file, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /// <summary>
        /// Hàm xử lý sau khi Prolog đc load thành công
        /// </summary>
        private void PlInitialized()
        {
            
            HashSet<string> hs = new HashSet<string>();
            List<string> benh = new List<string>();
            using (var query = new PlQuery("dinh_nghia_benh(Benh, DauHieu)."))
            {
                foreach (var sol in query.SolutionVariables)
                {
                    string s = sol["DauHieu"].ToString();
                    if (!hs.Contains(s))
                    {
                        dauhieu.Add(sol["DauHieu"].ToString());
                        benh.Add(sol["Benh"].ToString());
                        hs.Add(s);
                        
                        
                    }
                }
            }
            lstDauHieu.DataSource = dauhieu;
            lstBenh.DataSource = benh;
            /*string str = "dinh_nghia_benh(Benh,'khong khoi dong')";
           using (var q = new PlQuery(str))
           {
               foreach (var sol in q.SolutionVariables)
               {
                   string s = sol["Benh"].ToString();
                   benh.Add(s);
                   if (!hs.Contains(s))
                   {
                       hs.Add(s);
                   }
               }
              if(benh.Count>=2)
               {
                   foreach(var b in benh)
                   {
                       string strBenh = string.Format("dinh_nghia_benh('{0}',Y)", b);
                        using (var qBenh = new PlQuery(strBenh))
                        {
                            foreach (var sol in qBenh.SolutionVariables)
                            {
                                string s = sol["Y"].ToString();
                                DialogResult dialogResult = MessageBox.Show("May cua ban co bi "+s,"Question?", MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.Yes)
                                {
                                    //do something
                                }
                                else if (dialogResult == DialogResult.No)
                                {
                                    break;
                                }
                                 
                            }

                        }
                   }
               }
                
               //lstBenh.DataSource = benh;
           }*/
        }

        private void commandBarItem34_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Prolog file (*.pl)|*.pl" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                PlInit(dlg.FileName);
                PlInitialized();
            }
            catch (Exception exc)
            {
                XtraMessageBox.Show(this, exc.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Environment.SetEnvironmentVariable("SWI_HOME_DIR", @"C:\Program Files (x86)\swipl");
            Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path") + @";C:\Program Files (x86)\swipl\bin");

            //status.Caption = "Sẵn sàng";
        }

        private void btnKiemTra_Click(object sender, EventArgs e)
        {
            
            loimay.Clear();
            List<string> test = new List<string>();
            int ktr = 0;
            int ktr1 = 0;
            foreach (var d in dauhieu)
            {
                String b = "";
                try
                {
                    if ((b = d.Substring(0, d.IndexOf(txtBenh.Text.ToString()))) != "")
                    {
                        loimay.Add(d);
                        ktr++;
                      
                        string[] arrListStr = d.Split(',');
                        int i = 0;
                        string strDauHieu = "";
                        foreach(var str in arrListStr)
                        {
                             string strtest = str.Substring(8);
                            string[] s0=strtest.Split(')');
                             test.Add(s0[0]);
                            if (String.Compare(s0[0], txtBenh.Text.ToString(), true) == 0)
                            {
                                if (strDauHieu != "")
                                {
                                    strDauHieu = strDauHieu + "," + string.Format("dauhieu('{0}')", s0[0]);
                                }
                                else
                                {
                                    strDauHieu = string.Format("dauhieu('{0}')", s0[0]);
                                }
                                continue;
                            }
                            else
                            {
                                DialogResult dialogResult = MessageBox.Show(s0[0], "Question?", MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.Yes)
                                {
                                    i++;
                                    if (strDauHieu!="")
                                    {
                                        strDauHieu = strDauHieu+","+string.Format("dauhieu('{0}')", s0[0]);
                                    }
                                    else
                                    {
                                        strDauHieu = string.Format("dauhieu('{0}')", s0[0]);
                                    }
                                    
                                }
                                else if (dialogResult == DialogResult.No)
                                {
                                    break;
                                }
                            }

                        }
                        if (i == (arrListStr.Count()-1))
                        {
                            string str1 = string.Format("dinh_nghia_benh(X,({0}))", strDauHieu);
                            using (var q = new PlQuery(str1))
                            {
                                foreach (var sol in q.SolutionVariables)
                                {
                                    string s = sol["X"].ToString();
                                    XtraMessageBox.Show(this, "Máy của bạn bị lỗi sau: " + s, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                }
                                ktr1 = 1;
                            }
                            break;
                        }
                        else
                        {
                            //XtraMessageBox.Show(this, "Hệ thống không tìm ra bệnh", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        
                       
                        
                    }
                    else continue;
                }
                catch (Exception ex)
                {

                }

            }
            if(ktr==0||ktr1==0)
                XtraMessageBox.Show(this, "Hệ thống không tìm ra bệnh", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //lstBenh.DataSource = test;
        }
    }
}
