using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace Hkurs
{
    public partial class TextMode : Form
    {
        public TextMode()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != textBox5.Text)
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

            string text = textBox2.Text; 

            //шифруем
            byte[][] enc = Global.Encrypt_Final(text);

            byte[] lenc = new byte[enc.Length * 8];
            for (int i = 0; i < enc.Length; i++)
            {
                Buffer.BlockCopy(enc[i], 0, lenc, i * 8, 8);
            }

            textBox3.Text = Convert.ToBase64String(lenc);

        }

        private void button6_Click(object sender, EventArgs e)
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


            byte[] encFile = Convert.FromBase64String(textBox2.Text);

            bool errorstate = false;
            string fin_str = Global.Decrypt_Final(encFile, ref errorstate);

            if (!errorstate)
            {
                textBox3.Text = fin_str;
            }
            else
            {
                MessageBox.Show("Key phrase was entered wrong, text has not been decrypted", "Error");
                textBox3.Text = "";
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            if (d.ShowDialog() == DialogResult.OK) { textBox4.Text = d.FileName; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                if (textBox1.Text != textBox5.Text)
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

                string text = textBox2.Text;

                //шифруем
                byte[][] enc = Global.Encrypt_Final(text);

                //записываем в файл
                FileStream fstream = File.Open(textBox4.Text, System.IO.FileMode.OpenOrCreate);

                for (int i = 0; i < enc.Length; i++)
                {
                    fstream.Write(enc[i], 0, 8);
                }

                fstream.Close();

                MessageBox.Show("Text has been encrypted and saved to file " + textBox4.Text);
            }
        }
    }
}
