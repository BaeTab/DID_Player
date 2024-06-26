﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DID_Player
{
    public partial class MainForm : Form
    {
        private Form fullScreen = null;
        private PictureBox pictureBox = null;
        private int currentIndex = 0;

        Dictionary<string, Image> images = new Dictionary<string, Image>();

        public MainForm()
        {
            InitializeComponent();
            regEvent();
            setListboxCount();
            comboBox1.SelectedIndex = 0;
        }

        private void regEvent()
        {
            button1.Click += Button1_Click;   // 파일 추가
            button2.Click += Button2_Click;   // 선택 삭제
            button3.Click += Button3_Click;   // 재생
            button4.Click += Button4_Click;   // 종료
            button5.Click += Button5_Click;   // 저장
            button6.Click += Button6_Click;   // 불러오기
            button7.Click += Button7_Click;   // 초기화

            listBox1.DragEnter += listBox1_DragEnter;
            listBox1.DragDrop += listBox1_DragDrop;
            listBox1.KeyDown += ListBox1_KeyDown;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;

            timer1.Tick += Timer1_Tick;
        }

        // 풀스크린에서의 종료 메뉴 (마우스 우클)
        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            closeFullScreen();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            // 순차적
            if (comboBox1.SelectedIndex == 0)
            {
                showImage();
            }
            // 랜덤
            else
            {
                Random random = new Random();
                currentIndex = random.Next(0, listBox1.Items.Count);
                showImage();
            }
        }

        // 리스트박스 인덱스가 바뀔때 미리보기를 할 수 있다.
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string imagePath = listBox1.SelectedItem.ToString();
                pictureBox1.Image = images[imagePath];
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp")
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None; // 이미지 파일이 아닌 경우 드롭x
                        return;
                    }
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                addImages(files);
            }
        }

        // Del 키로 리스트 제외
        private void ListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                removeImage();
            }
            setListboxCount();
        }

        // 추가
        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image Files | *.jpg;*png;*.jpeg;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                addImages(openFileDialog.FileNames);
            }
        }

        // 리스트 제외
        private void Button2_Click(object sender, EventArgs e)
        {
            removeImage();
            setListboxCount();
        }

        // 슬라이드쇼 시작
        private void Button3_Click(object sender, EventArgs e)
        {
            currentIndex = 0;
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("먼저 이미지를 리스트에 추가 해 주세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (fullScreen == null && pictureBox == null)
            {
                this.Hide();
                setFullScreen();
            }
            else
            {
                fullScreen = null;
                pictureBox = null;
                this.Hide();
                setFullScreen();
            }
        }

        // 종료
        private void Button4_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("프로그램을 종료 하시겠습니까?", "알림", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                Close();
            }
        }

        // 저장
        private void Button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON |*.json"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(images.Keys.ToList());
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"오류: {ex.Message}");
                }
            }
        }

        // 불러오기
        private void Button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON |*.json"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (listBox1.Items.Count != 0)
                {
                    listBox1.Items.Clear();
                    images.Clear();
                }
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    List<string> files = JsonConvert.DeserializeObject<List<string>>(json);

                    addImages(files.ToArray());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"오류: {ex.Message}");
                }
            }
        }

        // 초기화
        private void Button7_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0)
            {
                listBox1.Items.Clear();
                images.Clear();
                pictureBox1.Image = null;
            }
            setListboxCount();
        }

        // 풀스크린 실행 메서드
        private void setFullScreen()
        {
            if (fullScreen != null)
            {
                fullScreen = null;
            }
            if (pictureBox != null)
            {
                pictureBox = null;
            }
            fullScreen = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
#if DEBUG
                WindowState = FormWindowState.Normal,
#else
                WindowState = FormWindowState.Maximized,
#endif                
                TopMost = true,
                KeyPreview = true,
            };

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = images[listBox1.Items[currentIndex].ToString()]
            };
            currentIndex++;

            fullScreen.Controls.Add(pictureBox);
            fullScreen.Show();
            fullScreen.KeyDown += FullScreen_KeyDown;

            pictureBox.MouseClick += PictureBox_LeftClick;

            timer1.Interval = (int)numericUpDown1.Value * 1000;
            timer1.Start();
            Cursor.Hide();
        }

        // ESC 키 슬라이드쇼 종료 이벤트
        private void FullScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                closeFullScreen();
            }
        }

        // 좌클릭시 슬라이드쇼 종료 이벤트
        private void PictureBox_LeftClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                closeFullScreen();
            }
        }

        // 풀스크린 종료 메서드
        private void closeFullScreen()
        {
            timer1.Stop();
            Cursor.Show();
            if (DialogResult.Yes == MessageBox.Show("슬라이드쇼를 종료 하시겠습니까?", "알림", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                if (currentIndex != 0)
                {
                    currentIndex = 0;
                }
                if (fullScreen != null)
                {
                    fullScreen.Dispose();
                    fullScreen = null;
                }
                if (pictureBox != null)
                {
                    pictureBox.Dispose();
                    pictureBox = null;
                }
                Show();
            }
            else
            {
                Cursor.Hide();
                timer1.Start();
            }
        }

        // 인덱스에 따른 이미지 표시를 위한 메서드
        private void showImage()
        {
            if (currentIndex >= listBox1.Items.Count)
            {
                currentIndex = 0;
            }
            string imagePath = listBox1.Items[currentIndex].ToString();

            pictureBox.Image = images[imagePath];
            currentIndex++;
        }

        // 추가된 이미지를 리스트박스와 딕셔너리에 추가하는 메서드
        private void addImages(string[] files)
        {
            // 파일 배열이 null이 아니고 최소한 하나의 파일을 포함하고 있는지 확인
            if (files != null && files.Length > 0)
            {
                foreach (string file in files)
                {
                    // 파일이 이미 딕셔너리에 없는지 확인
                    if (!images.ContainsKey(file))
                    {
                        try
                        {
                            Image img = Image.FromFile(file);           // 파일로부터 이미지를 로드
                            images.Add(file, img);                           // 이미지를 파일 경로를 키로하여 딕셔너리에 추가
                            listBox1.Items.Add(file);                         // 파일 경로를 리스트 박스에 추가
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                listBox1.SelectedIndex = 0;         // 이미지 추가후 미리보기를 위해 리스트박스의 0번 인덱스 셀렉트
                setListboxCount();
            }
        }

        // 리스트 제외를 위한 메서드
        private void removeImage()
        {
            if (listBox1.SelectedItem != null)
            {
                string itemPath = listBox1.SelectedItem.ToString();
                listBox1.Items.Remove(itemPath);
                images[itemPath].Dispose();
                images.Remove(itemPath);
            }
            else
            {
                MessageBox.Show("먼저 제외할 아이템을 선택해 주세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 리스트 박스에 추가된 이미지 갯수 카운팅을 위한 메서드
        private void setListboxCount()
        {
            if (listBox1.Items.Count != 0)
            {
                string totalCount = listBox1.Items.Count.ToString();
                label4.Text = $"Total : {totalCount}";
            }
            else
            {
                label4.Text = $"Total : 0";
            }
        }
    }
}
