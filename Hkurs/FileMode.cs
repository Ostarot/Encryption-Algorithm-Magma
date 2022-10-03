using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace Hkurs
{
    public partial class FileMode : Form
    {
        public FileMode()
        {
            InitializeComponent();
        }

        private void FileMode_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            if (d.ShowDialog() == DialogResult.OK) { textBox3.Text = d.FileName; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            if (d.ShowDialog() == DialogResult.OK) { textBox4.Text = d.FileName; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == textBox4.Text)
            {
                MessageBox.Show("Input and output files can not be the same");
                return;

            }
            if (File.Exists(textBox3.Text) & textBox4.Text != "") // & File.Exists(textBox4.Text)
            {
                if (textBox1.Text != textBox2.Text)
                {
                    MessageBox.Show("Key phrases shoul be the same");
                    return;
                }

                //проверяем ключ на соответствие ограничениям
                if (!Global.Check_KeyPhrase(textBox1.Text))
                {
                    MessageBox.Show("Key phrase does not meet the requirements");
                    return;
                }

                //генерируем итерационные ключи
                byte[] key;
                using (SHA256 mySHA256 = SHA256.Create())
                {
                    key = mySHA256.ComputeHash(Encoding.GetEncoding(1251).GetBytes(textBox1.Text));
                }

                Global.GOST_Magma_Expand_Key(key);

                string text = File.ReadAllText(textBox3.Text, Encoding.GetEncoding(1251)); //без Encoding.GetEncoding(1251) не читает текст на рус

                //шифруем
                byte[][] enc = Global.Encrypt_Final(text);
 
                //записываем в файл
                FileStream fstream = File.Open(textBox4.Text, System.IO.FileMode.OpenOrCreate);

                for (int i = 0; i < enc.Length; i++)
                {
                    fstream.Write(enc[i], 0, 8);
                }

                fstream.Close();
                MessageBox.Show("File has been encrypted");
            }
            else
            {
                MessageBox.Show("Input/output file does not exist");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == textBox4.Text)
            {
                MessageBox.Show("Input and output files can not be the same");
                return;

            }
            if (File.Exists(textBox3.Text) & textBox4.Text != "")
            {
                //проверяем ключ на соответствие ограничениям
                if (!Global.Check_KeyPhrase(textBox1.Text))
                {
                    MessageBox.Show("Key phrase does not meet the requirements");
                    return;
                }

                //генерируем итерационные ключи
                byte[] key;
                using (SHA256 mySHA256 = SHA256.Create())
                {
                    key = mySHA256.ComputeHash(Encoding.GetEncoding(1251).GetBytes(textBox1.Text));
                }

                Global.GOST_Magma_Expand_Key(key); 

                byte[] encFile = File.ReadAllBytes(textBox3.Text);

                bool errorstate = false;
                string s = Global.Decrypt_Final(encFile, ref errorstate);
                if (!errorstate)
                {
                    File.WriteAllText(textBox4.Text, s, Encoding.GetEncoding(1251));
                    MessageBox.Show("File has been decrypted");
                }
                else
                {
                    MessageBox.Show("Key phrase was entered wrong, file has not been decrypted", "Error");
                }
            }
            else
            {
                MessageBox.Show("Input/output file does not exist");
            }
        }
    }
}
