using System.Text.RegularExpressions;

namespace WinFormTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Ҫ��֤�� URL
            string url = "http://www.example.com/path/to/resource";

            // ��֤ URL ��������ʽ
            string pattern = @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})(:[0-9]{1,5})?([\/\w \.-]*)*\/?$";

            // ����һ�� Regex ����
            Regex regex = new Regex(pattern);

            // ��� URL �Ƿ���ϸ�ʽ
            if (regex.IsMatch(url))
            {
                MessageBox.Show("URL ��ʽ��ȷ");
            }
            else
            {
                MessageBox.Show("URL ��ʽ����ȷ");
            }
        }
    }
}