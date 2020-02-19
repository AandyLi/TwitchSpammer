using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TwitchSpammer
{
    public partial class Form2 : Form
    {
        List<string> filteredEmotes = new List<string>();

        public Form2()
        {
            InitializeComponent();
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public CheckedListBox checkedListBox_public
        {
            get { return checkedListBox1; }
            set { checkedListBox1 = value; }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var emoteName in filteredEmotes)
            {
                Properties.Settings.Default.FilteredEmotes += emoteName + ",";
            }
            //Console.WriteLine("Filtered emotes on close: " + Properties.Settings.Default.FilteredEmotes);
            Properties.Settings.Default.Save();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string emoteName = checkedListBox1.Items[e.Index].ToString();


            if (e.NewValue == CheckState.Unchecked)
            {
                //Console.WriteLine("CheckState.Unchecked begin " + emoteName + ". Filtered emotes before: " + Properties.Settings.Default.FilteredEmotes);
                filteredEmotes.Add(emoteName);
                //Console.WriteLine("Filtered " + emoteName);
            }
            else if(e.NewValue == CheckState.Checked)
            {
                //Console.WriteLine("Readded " + emoteName + ". Filtered emotes before: " + Properties.Settings.Default.FilteredEmotes);
                filteredEmotes.Remove(emoteName);
                RemoveFromSettingsFile(emoteName);
                //Console.WriteLine("Filtered emotes after: " + Properties.Settings.Default.FilteredEmotes);
            }

        }

        private void RemoveFromSettingsFile(string emoteName)
        {
            string emoteFilterStr = Properties.Settings.Default.FilteredEmotes;
            string emoteFilterStrCopy = emoteFilterStr;
            try
            {
                // Use regex to replace emote in string
                emoteFilterStr = StrReplaceRegex(@"\b" + emoteName + "," + @"\b", "");

                // Workaround for strange edge case with semicolon
                if (emoteFilterStr == emoteFilterStrCopy)
                {
                    emoteFilterStr = StrReplace(emoteName + ",", "");
                }

            }
            catch (Exception e)
            {
                // When regex fails, use regular replace
                emoteFilterStr = StrReplace(emoteName + ",", "");
                Console.WriteLine("Exception: " + e.Message);
            }

            // Save settings
            Properties.Settings.Default.FilteredEmotes = emoteFilterStr;
        }

        private string StrReplaceRegex(string oldStr, string newStr)
        {
            string str;

            str = Regex.Replace(Properties.Settings.Default.FilteredEmotes, oldStr, newStr);

            return str;
        }

        private string StrReplace(string oldStr, string newStr)
        {
            string str;

            str = Properties.Settings.Default.FilteredEmotes.Replace(oldStr, newStr);

            return str;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            // Register item check event after form has loaded
            checkedListBox1.ItemCheck += new ItemCheckEventHandler(checkedListBox1_ItemCheck);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }
    }
}
