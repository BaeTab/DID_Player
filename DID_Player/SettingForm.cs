using System;
using System.Windows.Forms;

namespace DID_Player
{
    public partial class SettingForm : Form
    {
        private MainForm mainForm = null;

        public SettingForm(MainForm mainForm)
        {
            InitializeComponent();
            regEvent();
            comboBox1.SelectedIndex = 0;
            this.mainForm = mainForm;
        }

        private void regEvent()
        {
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if(numericUpDown1.Value > 0  && comboBox1.SelectedIndex != -1)
            {
                Close();
                mainForm.UpdateValues(numericUpDown1.Value , comboBox1.SelectedIndex);
            }
            else
            {
                MessageBox.Show("설정을 적용할 수 없습니다", "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
