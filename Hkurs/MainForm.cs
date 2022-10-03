using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hkurs
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 AB1 = new AboutBox1();
            AB1.ShowDialog();
        }

        private void текстовыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextMode txtM = new TextMode();
            txtM.ShowDialog();
        }

        private void файловыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileMode flM = new FileMode();
            flM.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            if (checkBox1.Checked) Global.upper_case = true;
            else Global.upper_case = false;

            if (checkBox2.Checked) Global.digit = true;
            else Global.digit = false;

            if (checkBox3.Checked) Global.symbol = true;
            else Global.symbol = false;

            Global.max_length = (int)MaxLength.Value;
            Global.min_length = (int)MinLength.Value;

            MessageBox.Show("Requirements are saved");
        }
    }

    public static class Global
    {
        public static bool upper_case = false;
        public static bool symbol = false;
        public static bool digit = false;
        public static int max_length = 100;
        public static int min_length = 0;

        public static string signature = "Victoria";

        public static bool Check_KeyPhrase(string key)
        {
            if (key.Length > max_length | key.Length < min_length)
                return false;

            bool fU = false;
            bool fS = false;
            bool fD = false;

            for (int i = 0; i < key.Length; i++)
            {
                if (Char.IsUpper(key[i])) fU = true;
                if (Char.IsDigit(key[i])) fD = true;
                if (Char.IsSymbol(key[i]) | Char.IsPunctuation(key[i])) fS = true;
            }

            if (!(!upper_case | fU)) return false;
            if (!(!symbol | fS)) return false;
            if (!(!digit | fD)) return false;

            return true;
        }

        public static string Decrypt_Final(byte[] text, ref bool errorstate)
        {
            errorstate = false;
            byte[][] encNew = Global.Divide_Blocks_B(text);
            byte[][] dec = Global.Decrypt_All(encNew);
            string str = Global.Decrypt_ToString(dec);
            for (int i = 0; i < 3; i++) //вырезаем добавленные пробелы
            {
                if (str.Substring(str.Length - 1, 1) == " ")
                    str = str.Substring(0, str.Length - 1);
            }
            if (str.Substring(0, Global.signature.Length) == Global.signature)
            {
                str = str.Substring(Global.signature.Length);
                return str;
            }
            else
            {
                errorstate = true;
                return "";
            }
        }

        public static byte[][] Encrypt_Final(string text)
        {
            text = Global.signature + text;
            string textspace = Global.Add_Spaces(text);
            byte[][] blocks = Global.Divide_Blocks(textspace);
            byte[][] enc = Global.Encrypt_All(blocks);
            return enc;
        }

        public static string Add_Spaces(string str)
        {
            if ((str.Length % 4) != 0)
            {
                while ((str.Length % 4) != 0)
                    str = str + " ";
            }
            return str;
        }

        public static byte[][] Divide_Blocks_B(byte[] str)
        {
            byte[][] blocks = new byte[str.Length / 8][];
            for (int i = 0; i < blocks.Length; i++)
            {
               blocks[i] = new byte[8];
               Buffer.BlockCopy(str, i*8, blocks[i], 0, 8);
            }
            return blocks;
        }

        public static byte[][] Divide_Blocks(string str)
        {
            byte[][] blocks = new byte[str.Length / 4][];
            for (int i = 0; i < blocks.Length; i++)
                blocks[i] = Encoding.Unicode.GetBytes(str.Substring(i * 4, 4));
            return blocks;
        }

        public static byte[][] Encrypt_All(byte[][] blocks)
        {
            byte[][] blocks_out = new byte[blocks.Length][];
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks_out[i] = new byte[8];
                GOST_Magma_Encrypt(blocks[i], ref blocks_out[i]);
            }

            return blocks_out;
        }

        public static byte[][] Decrypt_All(byte[][] blocks)
        {
            byte[][] blocks_out = new byte[blocks.Length][];
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks_out[i] = new byte[8];
                GOST_Magma_Decrypt(blocks[i], ref blocks_out[i]);
            }

            return blocks_out;
        }

        public static string Decrypt_ToString(byte[][] blocks)
        {
            string str = string.Empty;
            for (int i = 0; i < blocks.Length; i++)
            {
                str = str + Encoding.Unicode.GetString(blocks[i]);
            }
            return str;
        }

        // Размер блока 4 байта (или 32 бита)
        public const int BLOCK_SIZE = 4;

        //таблица перестановок
        public static byte[][] Pi = new byte[][] //[8][16]
        {
          new byte[] {1,7,14,13,0,5,8,3,4,15,10,6,9,12,11,2},
          new byte[] {8,14,2,5,6,9,1,12,15,4,11,0,13,10,3,7},
          new byte[] {5,13,15,6,9,2,12,10,11,7,8,1,4,3,14,0},
          new byte[] {7,15,5,10,8,1,6,13,0,9,3,14,11,4,2,12},
          new byte[] {12,8,2,1,13,4,15,6,7,0,10,5,3,14,9,11},
          new byte[] {11,3,5,8,2,15,10,13,14,1,7,4,12,9,6,0},
          new byte[] {6,8,2,3,9,10,5,12,1,14,4,7,11,13,0,15},
          new byte[] {12,4,6,2,10,5,11,9,14,8,13,7,0,3,15,1}
        };



        //Сложение двух двоичных векторов по модулю 2
        public static void GOST_Magma_Add(byte[] a, byte[] b, ref byte[] c)
        {
            for (int i = 0; i < BLOCK_SIZE; i++)
                c[i] = (byte)(a[i] ^ b[i]);
        }

        //Сложение двух двоичных векторов по модулю 32        
        public static void GOST_Magma_Add_32(byte[] a, byte[] b, ref byte[] c)
        {
            uint internal1 = 0;
            for (int i = 3; i >= 0; i--)
            {
                internal1 = (uint)(a[i] + b[i] + (internal1 >> 8));
                c[i] = (byte)(internal1 & 0xff);
            }
        }

        //Нелинейное биективное преобразование (преобразование T)
        public static void GOST_Magma_T(byte[] in_data, ref byte[] out_data)
        {
            byte first_part_byte, sec_part_byte;
            for (int i = 0; i < 4; i++)
            {
                // Извлекаем первую 4-битную часть байта
                first_part_byte = (byte)((in_data[i] & 0xf0) >> 4);
                // Извлекаем вторую 4-битную часть байта
                sec_part_byte = (byte)(in_data[i] & 0x0f);
                // Выполняем замену в соответствии с таблицей подстановок
                first_part_byte = Pi[i * 2][first_part_byte];
                sec_part_byte = Pi[i * 2 + 1][sec_part_byte];
                // «Склеиваем» обе 4-битные части обратно в байт
                out_data[i] = (byte)((first_part_byte << 4) | sec_part_byte);
            }
        }

        public static byte[][] iter_key = new byte[32][]; // Итерационные ключи шифрования

        //Развертывание ключей
        public static void GOST_Magma_Expand_Key(byte[] key)
        {
            // Формируем первых 24 32-битных подключа в порядке следования с первого по 24
            for (int i = 0; i < 8; i++)
            {
                iter_key[i] = new byte[4];
                Buffer.BlockCopy(key, i * 4, iter_key[i], 0, 4);
            }

            for (int i = 0, k = 8; i < 8; i++, k++)
            {
                iter_key[k] = new byte[4];
                Buffer.BlockCopy(key, i * 4, iter_key[k], 0, 4);
            }

            for (int i = 0, k = 16; i < 8; i++, k++)
            {
                iter_key[k] = new byte[4];
                Buffer.BlockCopy(key, i * 4, iter_key[k], 0, 4);
            }
            // Формируем восемь 32-битных подключей в порядке следования с восьмого по первый
            for (int i = 24, k = 0; i < 32; i++, k++)
            {
                iter_key[i] = new byte[4];
                Buffer.BlockCopy(key, 28 - k * 4, iter_key[i], 0, 4);
            }
        }


        //Преобразование g
        public static void GOST_Magma_g(byte[] k, byte[] a, ref byte[] out_data)
        {
            byte[] internal1 = new byte[4];
            uint out_data_32;

            // Складываем по модулю 32 правую половину блока с итерационным ключом
            GOST_Magma_Add_32(a, k, ref internal1);

            // Производим нелинейное биективное преобразование результата
            GOST_Magma_T(internal1, ref internal1);

            // Преобразовываем четырехбайтный вектор в одно 32-битное число
            out_data_32 = internal1[0];
            out_data_32 = (out_data_32 << 8) + internal1[1];
            out_data_32 = (out_data_32 << 8) + internal1[2];
            out_data_32 = (out_data_32 << 8) + internal1[3];

            // Циклически сдвигаем все влево на 11 разрядов
            out_data_32 = (out_data_32 << 11) | (out_data_32 >> 21);
            // Преобразовываем 32-битный результат сдвига обратно в 4-байтовый вектор
            out_data[3] = (byte)(out_data_32);
            out_data[2] = (byte)(out_data_32 >> 8);
            out_data[1] = (byte)(out_data_32 >> 16);
            out_data[0] = (byte)(out_data_32 >> 24);
        }

        //Преобразование G
        public static void GOST_Magma_G(byte[] k, byte[] a, ref byte[] out_data)
        {
            byte[] a_0 = new byte[4]; // Правая половина блока
            byte[] a_1 = new byte[4]; // Левая половина блока
            byte[] G = new byte[4];

            int i;
            // Делим 64-битный исходный блок на две части
            for (i = 0; i < 4; i++)
            {
                a_0[i] = a[4 + i];
                a_1[i] = a[i];
            }

            // Производим преобразование g
            GOST_Magma_g(k, a_0, ref G);
            // xor результат преобразования g с левой половиной блока
            GOST_Magma_Add(a_1, G, ref G);

            for (i = 0; i < 4; i++)
            {
                // Пишем в левую половину значение из правой
                a_1[i] = a_0[i];
                // Пишем результат GOST_Magma_Add в правую половину блока
                a_0[i] = G[i];
            }

            // Сводим правую и левую части блока в одно целое
            for (i = 0; i < 4; i++)
            {
                out_data[i] = a_1[i];
                out_data[4 + i] = a_0[i];
            }
        }

        //Финальное преобразование G
        public static void GOST_Magma_G_Fin(byte[] k, byte[] a, ref byte[] out_data)
        {
            byte[] a_0 = new byte[4]; // Правая половина блока
            byte[] a_1 = new byte[4]; // Левая половина блока
            byte[] G = new byte[4];

            int i;
            // Делим 64-битный исходный блок на две части
            for (i = 0; i < 4; i++)
            {
                a_0[i] = a[4 + i];
                a_1[i] = a[i];
            }

            // Производим преобразование g
            GOST_Magma_g(k, a_0, ref G);
            // xor результат преобразования g с левой половиной блока
            GOST_Magma_Add(a_1, G, ref G);
            // Пишем результат GOST_Magma_Add в левую половину блока
            for (i = 0; i < 4; i++)
                a_1[i] = G[i];

            // Сводим правую и левую части блока в одно целое
            for (i = 0; i < 4; i++)
            {
                out_data[i] = a_1[i];
                out_data[4 + i] = a_0[i];
            }
        }

        //Шифрование Магма
        public static void GOST_Magma_Encrypt(byte[] blk, ref byte[] out_blk)
        {
            int i;
            // Первое преобразование G
            GOST_Magma_G(iter_key[0], blk, ref out_blk);
            // Последующие (со второго по тридцать первое) преобразования G
            for (i = 1; i < 31; i++)
                GOST_Magma_G(iter_key[i], out_blk, ref out_blk);
            // Последнее (тридцать второе) преобразование G
            GOST_Magma_G_Fin(iter_key[31], out_blk, ref out_blk);
        }

        //Расшифровывание Магма
        public static void GOST_Magma_Decrypt(byte[] blk, ref byte[] out_blk)
        {
            int i;
            // Первое преобразование G с использованием
            // тридцать второго итерационного ключа
            GOST_Magma_G(iter_key[31], blk, ref out_blk);
            // Последующие (со второго по тридцать первое) преобразования G
            // (итерационные ключи идут в обратном порядке)
            for (i = 30; i > 0; i--)
                GOST_Magma_G(iter_key[i], out_blk, ref out_blk);
            // Последнее (тридцать второе) преобразование G
            // с использованием первого итерационного ключа
            GOST_Magma_G_Fin(iter_key[0], out_blk, ref out_blk);
        }
    }
}
