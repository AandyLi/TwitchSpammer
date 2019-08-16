﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;



// Todo / Feature list

/*
 
    Custom message:
    Send custom message - Choose @ or not - done
    Multiply custom message x times - done
    Send messages y amount with z interval - done

    Emote behaviour:
    Fast emote - clicked emote is instantly sent to chat - done
    Click emote to add to custom text - done
    Differentiate if user wants fast emote or add to text - done
    Emote spam - choose emote and modify its spam behaviour


    Spam behaviour:
    Specify 1 or more emotes to spam (maybe show in text box?)
    Select how many messages will be sent - done
    Select how many emotes each message contains - done
    Make each message unique - change / remove one emote - done


    Global:
    Select @
    Save custom emotes
    Save custom messages 
    Load custom messages on startup



    Future:
    Emote pictures - done
    Emote pictures embedded in text
    Server sided emote updates

     */




    // Test



namespace TwitchSpammer
{

    public partial class Form1 : Form
    {

        int chatBoxMouseX;
        int chatBoxMouseY;

        int multiplyDecrease = 0;

        public Form1()
        {
            InitializeComponent();


            getImages();

            getSavedText();

            GetDebugSettings();
        }

        private void GetDebugSettings()
        {
            if (checkBox5.Checked)
            {
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
            }
        }

        private void getImages()
        {

            // Load images into imagelist
            foreach (String path in Directory.GetFiles("../../Images"))
            {
                Console.WriteLine(path);

                imageList1.Images.Add(Path.GetFileNameWithoutExtension(path), Image.FromFile(path));
            }


            // Populate listview with imagelist
            for (int i = 0; i < 5; i++)
            {

                listView1.LargeImageList = imageList1;

                foreach (var key in imageList1.Images.Keys)
                {

                    listView1.Items.Add(key, key);
                }
            }
        }

        private void getSavedText()
        {
            string path = "../../SavedText.txt";

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    listView2.Items.Add(line);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer2.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point p = System.Windows.Forms.Control.MousePosition;

            label1.Text = p.X.ToString();
            label2.Text = p.Y.ToString();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Point p = System.Windows.Forms.Control.MousePosition;

            label3.Text = p.X.ToString() + ", ";
            label3.Text += p.Y.ToString();

            timer2.Stop();


            // Temp
            chatBoxMouseX = p.X;
            chatBoxMouseY = p.Y;

        }

        private void button2_Click(object sender, EventArgs e)
        {


            if (numericUpDown3.Value > 0)
            {
                timer3.Interval = (int)numericUpDown2.Value * 1000;
                timer3.Start();
            }
            else
            {
                string msg = compileMessage();
                sendMessage(msg);
            }
        }

        private string compileMessage()
        {
            string msg = "";
            string atStreamer = "@" + streamerName.Text + " ";
            string textBoxText = richTextBox1.Text + " ";

            if (checkBox1.Checked)
            {
                msg += atStreamer;
            }

            // Check if message should be repeated
            if (numericUpDown1.Value > 0)
            {
                // Repeat for every n - 1 messages, also decrease if spam is enabled
                for (int i = 0; i < (numericUpDown1.Value - 1 - multiplyDecrease); i++)
                {
                    msg += textBoxText;

                    // Check if "repeat streamer name" is checked
                    if (checkBox1.Checked && checkBox3.Checked)
                    {
                        msg += atStreamer;
                    }
                }

                // Send nth message
                msg += textBoxText;
            }
            else
            {
                msg += textBoxText;
            }

            return msg;
        }

        private void listView1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                sendMessage(listView1.FocusedItem.ImageKey);
                Console.WriteLine("Right button clicked " + listView1.FocusedItem.ToString());
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (checkBox4.Checked && richTextBox1.Text != "")
                {
                    richTextBox1.Text += " ";
                }
                richTextBox1.Text += listView1.FocusedItem.ImageKey;
            }
        }

        private void sendMessage(string msg)
        {
            Point currentMousePos = Cursor.Position;

            Cursor.Position = new Point(chatBoxMouseX, chatBoxMouseY);

            MouseHandler mh = new MouseHandler();

            mh.MouseClick(chatBoxMouseX, chatBoxMouseY);

            Clipboard.SetText(msg);

            SendKeys.Send("{END}");

            SendKeys.Send("^{v}");

            // Send ENTER

            Cursor.Position = currentMousePos;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.Visible == true)
            {
                listView1.Visible = false;
                button3.Text = "Default";
                listView2.Visible = true;
            }
            else if (listView2.Visible == true)
            {
                listView2.Visible = false;
                button3.Text = "Custom";
                listView1.Visible = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listView2.Items.Add(richTextBox1.Text);

            saveCustomTextToFile(richTextBox1.Text);
        }

        private void saveCustomTextToFile(string text)
        {

            string path = "../../SavedText.txt";

            File.AppendAllText(path, text + "\n");
          
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (panel2.Visible == true)
            {
                panel2.Visible = false;
            }
            else
            {
                panel2.Visible = true; 
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            string msg = compileMessage();
            sendMessage(msg);

            numericUpDown3.Value --;
            if (checkBox2.Checked)
            {
                multiplyDecrease++;
            }

            if (numericUpDown3.Value == 0)
            {
                timer3.Stop();
                multiplyDecrease = 0;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(imageList1.Images[imageList1.Images.IndexOfKey("Kappa")]);
            richTextBox1.Paste();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DownloadEmoteUpdateFile();



            //WebClient wc = new WebClient();

            //wc.DownloadFileCompleted += new AsyncCompletedEventHandler(complete);
            //wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgressBar);
            //wc.Headers.Add("User-Agent: Other");
            //wc.DownloadFileAsync(new Uri("https://image.shutterstock.com/image-photo/bright-spring-view-cameo-island-260nw-1048185397.jpg"), @"c:\test\file2.jpg");
        }


        private void DownloadEmoteUpdateFile()
        {
            WebClient wc = new WebClient();

            string emoteFile = @"emoteFile.txt";
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(EmoteUpdateFileComplete);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgressBar);
            wc.Headers.Add("User-Agent: Other");
            wc.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/AandyLi/TwitchSpammer/master/emotelinks.txt"), emoteFile);

            

        }

        private void ReadEmoteFile(string emoteFile)
        {

            DownloadInfo DI = new DownloadInfo(0, 0);

            // Read file
            List<string> currentEmotes = new List<string>();

            foreach (String path in Directory.GetFiles("../../Images"))
            {
                Console.WriteLine(path);

                currentEmotes.Add(Path.GetFileNameWithoutExtension(path));
            }

            string[] lines = File.ReadAllLines(emoteFile);

            foreach (string line in lines)
            {
                string[] splitLine = line.Split(' ');

                bool emoteFound = false;

                // Look for emote 
                foreach (string emote in currentEmotes)
                {
                    if (splitLine[0] == emote)
                    {
                        emoteFound = true;
                    }
                }

                if (!emoteFound)
                {
                    DI.TotalDownloads++;

                    UpdateDownloadEmoteText(DI);

                    // download emote
                    DownloadEmote(splitLine[0], splitLine[1], DI);
                    
                }
            }

            if (File.Exists(emoteFile))
            {
                File.Delete(emoteFile);
            }

        }

        private void UpdateDownloadEmoteText(DownloadInfo DI)
        {
            label11.Text = "Downloading emote " + DI.CurrentDownload + " of " + DI.TotalDownloads.ToString();
        }

        private void DownloadEmote(string emote, string link, DownloadInfo DI)
        {
            DI.CurrentDownload++;

            string path = @"../../Images/";

            WebClient wc = new WebClient();
            
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(complete);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgressBar);
            wc.Headers.Add("User-Agent: Other");
            wc.DownloadFileAsync(new Uri(link), path + emote + ".png");

            UpdateDownloadEmoteText(DI);
        }

        private void EmoteUpdateFileComplete(object sender, AsyncCompletedEventArgs e)
        {

            string emoteFile = @"emoteFile.txt";
            ReadEmoteFile(emoteFile);
        }

        private void complete(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download complete");
        }

        private void updateProgressBar(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                sendMessage(listView2.FocusedItem.Text);
                Console.WriteLine("Right button clicked " + listView2.FocusedItem.Text);
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (checkBox4.Checked && richTextBox1.Text != "")
                {
                    richTextBox1.Text += " ";
                }
                richTextBox1.Text += listView2.FocusedItem.Text;
            }
            
        }

        private void listView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                Point mousePoint = Cursor.Position;

                Point clientPoint = listView2.PointToClient(mousePoint);

                ListViewItem item = listView2.GetItemAt(clientPoint.X, clientPoint.Y);

                listView2.Items.Remove(item);

                // remove item from save file
                RemoveFromSaveFile(item);
            }
        }

        private void RemoveFromSaveFile(ListViewItem item)
        {
            string path = "../../SavedText.txt";

            string textToRemove = item.Text;

            string[] allLines = File.ReadAllLines(path);

            string[] newText = new string[allLines.Length - 1];

            int j = 0;
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i] != textToRemove)
                {
                    newText[j] = allLines[i];
                    j++;
                }
            }
            File.Delete(path);
            File.AppendAllLines(path, newText);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
            }

            if (!checkBox5.Checked)
            {
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
            }
        }
    }
}
