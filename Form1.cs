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
        const string PATH = @"C:\Corey Derochie\clui.dll";
        private Dictionary<int, string> strings;
        private string loadPath;

        public Form1()
        {
            InitializeComponent();
            loadPath = PATH;
        }

        public Form1(string path)
        {
            InitializeComponent();
            loadPath = path;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var stream = File.OpenRead(loadPath);

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            char[] unicode = new char[buffer.Length / sizeof(char)];
            Buffer.BlockCopy(buffer, 0, unicode, 0, buffer.Length);

            strings = unicode.SplitWhere((char)0).ToDictionary(p => p.Key, p => new string(p.Value));

            listBox1.DataSource = format(strings).ToList();
            listBox1.DisplayMember = "Value";
            listBox1.ValueMember = "Key";

            Text = Text + " - " + loadPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.DataSource = format(strings.Where(p => p.Value.Contains(searchText.Text) || p.Key.ToString().Contains(searchText.Text))).ToList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedValue is int)
            {
                string val = strings[(int)listBox1.SelectedValue];
                stringEditor.MaxLength = val.Length;
                stringEditor.Text = val;

                origDisplay.Text = val;

                stringEditor.ReadOnly = false;

            }
            else
            {
                stringEditor.Text = origDisplay.Text = "";
                stringEditor.ReadOnly = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedValue is int)
            {
                string val = stringEditor.Text.PadRight(stringEditor.MaxLength, ' ');
                int i = (int)listBox1.SelectedValue;
                strings[i] = val;

                listBox1.DataSource = format(strings.Where(p => p.Value.Contains(searchText.Text))).ToList();
                listBox1.DisplayMember = "Value";
                listBox1.ValueMember = "Key";

                byte[] array = new byte[val.Length * sizeof(char)];
                Buffer.BlockCopy(val.ToCharArray(), 0, array, 0, array.Length);
                var stream = File.OpenWrite(loadPath);
                stream.Seek((long)i * sizeof(char), SeekOrigin.Begin);
                stream.Write(array, 0, array.Length);
                stream.Close();
            }
        }

        private Dictionary<int, string> format(IEnumerable<KeyValuePair<int, string>> arg)
        {
            return arg.ToDictionary(p => p.Key, p => string.Format("{0}\t{1}", p.Key, p.Value));
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
