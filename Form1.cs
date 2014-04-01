using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EditCLUI
{
    public partial class Form1 : Form
    {
        const string PATH = @"E:\Veggie\clui.dll";
        private Dictionary<int, string> strings;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var stream = File.OpenRead(PATH);

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            char[] unicode = new char[buffer.Length / sizeof(char)];
            Buffer.BlockCopy(buffer, 0, unicode, 0, buffer.Length);

            strings = unicode.SplitWhere((char)0).ToDictionary(p => p.Key, p => new string(p.Value));

            listBox1.DataSource = strings.ToList();
            listBox1.DisplayMember = "Value";
            listBox1.ValueMember = "Key";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.DataSource = strings.Where(p => p.Value.Contains(textBox2.Text)).ToList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedValue is int)
            {
                string val = strings[(int)listBox1.SelectedValue];
                textBox1.MaxLength = val.Length;
                textBox1.Text = val;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedValue is int)
            {
                string val = textBox1.Text.PadRight(textBox1.MaxLength, ' ');
                int i = (int)listBox1.SelectedValue;
                strings[i] = val;

                listBox1.DataSource = strings.Where(p => p.Value.Contains(textBox2.Text)).ToList();
                listBox1.DisplayMember = "Value";
                listBox1.ValueMember = "Key";

                byte[] array = new byte[val.Length * sizeof(char)];
                Buffer.BlockCopy(val.ToCharArray(), 0, array, 0, array.Length);
                var stream = File.OpenWrite(PATH);
                stream.Seek((long)i * sizeof(char), SeekOrigin.Begin);
                stream.Write(array, 0, array.Length);
                stream.Close();
            }
        }
    }

    static class Extensions
    {
        public static IEnumerable<KeyValuePair<int, T[]>> SplitWhere<T>(this T[] array, T separator)
        {
            int j = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(separator))
                {
                    if (i > j)
                    {
                        var sub = new T[i - j];
                        Array.Copy(array, j, sub, 0, i - j);
                        yield return new KeyValuePair<int, T[]>(j, sub);
                    }

                    j = i + 1;
                }
            }
        }
    }
}
