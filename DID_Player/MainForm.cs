using Newtonsoft.Json;
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
        private Random random = new Random(); // Random 객체를 클래스 필드로 이동하여 재사용

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
            this.FormClosing += MainForm_FormClosing; // 폼 종료 시 리소스 정리
        }

        // 폼 종료 시 모든 이미지 리소스 해제
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeAllImages();
       
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
}

     // 모든 이미지 리소스를 해제하는 헬퍼 메서드
        private void DisposeAllImages()
        {
          foreach (var kvp in images)
         {
      if (kvp.Value != null)
        {
        kvp.Value.Dispose();
      }
            }
            images.Clear();
        }

        // 풀스크린에서의 종료 메뉴 (마우스 우클)
        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
       closeFullScreen();
        }

    private void Timer1_Tick(object sender, EventArgs e)
        {
            try
    {
        // 리스트가 비어있으면 타이머 중지
             if (listBox1.Items.Count == 0)
       {
          timer1.Stop();
          return;
        }

           // 순차적
    if (comboBox1.SelectedIndex == 0)
             {
         showImage();
             }
             // 랜덤
    else
            {
        currentIndex = random.Next(0, listBox1.Items.Count);
       showImage();
}
            }
      catch (Exception ex)
  {
       MessageBox.Show("이미지 전환 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
         timer1.Stop();
            }
        }

        // 리스트박스 인덱스가 바뀔때 미리보기를 할 수 있다.
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
 {
            if (listBox1.SelectedItem != null)
    {
         string imagePath = listBox1.SelectedItem.ToString();
         
     // 이미지가 딕셔너리에 존재하는지 확인
        if (images.ContainsKey(imagePath))
 {
         pictureBox1.Image = images[imagePath];
   }
  else
     {
            pictureBox1.Image = null;
           }
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
     return; // 리스트가 비어있으면 리턴
            }
       
            // 기존 풀스크린이 있으면 정리
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

            this.Hide();
       setFullScreen();
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
        MessageBox.Show("저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
           catch (Exception ex)
    {
         MessageBox.Show("저장 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
   try
      {
        // 기존 이미지들을 먼저 Dispose
   if (listBox1.Items.Count != 0)
   {
          DisposeAllImages();
           listBox1.Items.Clear();
  }
       
 string json = File.ReadAllText(openFileDialog.FileName);
               List<string> files = JsonConvert.DeserializeObject<List<string>>(json);

      if (files != null && files.Count > 0)
          {
    addImages(files.ToArray());
            }
     }
      catch (Exception ex)
              {
      MessageBox.Show("불러오기 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
   }
            }
  }

// 초기화
        private void Button7_Click(object sender, EventArgs e)
        {
 if (listBox1.Items.Count != 0)
    {
          // 모든 이미지 리소스를 해제
 DisposeAllImages();
          listBox1.Items.Clear();
       pictureBox1.Image = null;
            }
  setListboxCount();
     }

        // 풀스크린 실행 메서드
        private void setFullScreen()
      {
            try
 {
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

     string firstImagePath = listBox1.Items[currentIndex].ToString();
 
        // 파일 존재 여부 확인
    if (!File.Exists(firstImagePath) || !images.ContainsKey(firstImagePath))
  {
             MessageBox.Show("이미지 파일을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        fullScreen.Dispose();
  fullScreen = null;
         Show();
    return;
                }

   pictureBox = new PictureBox
    {
    Dock = DockStyle.Fill,
       SizeMode = PictureBoxSizeMode.StretchImage,
Image = images[firstImagePath]
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
    catch (Exception ex)
            {
         MessageBox.Show("슬라이드쇼 시작 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
   if (fullScreen != null)
 {
           fullScreen.Dispose();
         fullScreen = null;
  }
          Show();
     }
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

         // 파일 존재 여부 및 딕셔너리 확인
     if (!File.Exists(imagePath) || !images.ContainsKey(imagePath))
          {
       // 파일이 없으면 목록에서 제거하고 다음 이미지로
        RemoveInvalidImage(imagePath);
         return;
   }

   pictureBox.Image = images[imagePath];
 currentIndex++;
        }

        // 유효하지 않은 이미지를 목록에서 제거하는 메서드
        private void RemoveInvalidImage(string imagePath)
        {
    if (images.ContainsKey(imagePath))
            {
       if (images[imagePath] != null)
        {
           images[imagePath].Dispose();
     }
                images.Remove(imagePath);
            }
            
        if (listBox1.Items.Contains(imagePath))
       {
  listBox1.Items.Remove(imagePath);
       }
      
            setListboxCount();
}

// 추가된 이미지를 리스트박스와 딕셔너리에 추가하는 메서드
      private void addImages(string[] files)
   {
       // 파일 배열이 null이 아니고 최소한 하나의 파일을 포함하고 있는지 확인
            if (files != null && files.Length > 0)
       {
  int successCount = 0;
      int failCount = 0;

  foreach (string file in files)
          {
    // 파일 존재 여부 확인
      if (!File.Exists(file))
    {
     failCount++;
      continue;
      }

         // 파일이 이미 딕셔너리에 없는지 확인
           if (!images.ContainsKey(file))
 {
      try
  {
     Image img = Image.FromFile(file);      // 파일로부터 이미지를 로드
     images.Add(file, img);    // 이미지를 파일 경로를 키로하여 딕셔너리에 추가
      listBox1.Items.Add(file);           // 파일 경로를 리스트 박스에 추가
        successCount++;
      }
 catch (OutOfMemoryException)
           {
  MessageBox.Show("메모리 부족으로 이미지를 추가할 수 없습니다: " + Path.GetFileName(file), "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
failCount++;
   }
               catch (Exception ex)
        {
              MessageBox.Show("이미지 로드 오류 (" + Path.GetFileName(file) + "): " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
      failCount++;
             }
 }
              }

 // 결과 표시
         if (successCount > 0)
 {
           if (listBox1.Items.Count > 0)
          {
     listBox1.SelectedIndex = 0; // 이미지 추가후 미리보기를 위해 리스트박스의 0번 인덱스 셀렉트
         }
         setListboxCount();
         }

  if (failCount > 0)
     {
              MessageBox.Show(failCount + "개의 파일을 추가하지 못했습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
            }
     }

        // 리스트 제외를 위한 메서드
        private void removeImage()
     {
          if (listBox1.SelectedItem != null)
     {
   string itemPath = listBox1.SelectedItem.ToString();
   
       // 이미지 리소스 해제
   if (images.ContainsKey(itemPath))
             {
            if (images[itemPath] != null)
               {
          images[itemPath].Dispose();
          }
       images.Remove(itemPath);
         }
    
                listBox1.Items.Remove(itemPath);
      
      // 미리보기 이미지 정리
    if (pictureBox1.Image != null && !images.ContainsValue(pictureBox1.Image))
           {
     pictureBox1.Image = null;
       }
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
  label4.Text = "Total : " + totalCount;
          }
       else
 {
     label4.Text = "Total : 0";
 }
        }
  }
}
