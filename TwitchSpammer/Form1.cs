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
using System.Net;
using System.Text.RegularExpressions;


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
    Server sided emote updates - done


    Add in app info
    Clean up code
    Add exceptions

     */




// Test



namespace TwitchSpammer
{

    public partial class Form1 : Form
    {

        int chatBoxMouseX;
        int chatBoxMouseY;

        int msgPeriodAppend = 0;
        Theme theme;


        public Form1()
        {
            //Properties.Settings.Default.Reset();
            InitializeComponent();

            getImages();

            getSavedText();

            GetDebugSettings();

            PaddAndMoveMouseLabel();

            ApplySavedSettings();

            theme = new Theme(this);


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = 
                     SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

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

        private void ApplySavedSettings()
        {
            if (Properties.Settings.Default.UserSkipUpdate)
            {
                checkBox7.Checked = false;
            }

            if (Properties.Settings.Default.UseMs)
            {
                checkBox6.Checked = true;
                label7.Text = "Interval (ms)";
            }
            else
            {
                checkBox6.Checked = false;
                label7.Text = "Interval (s)";
            }


            if (Properties.Settings.Default.AutoAddSpace)
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }

            if (Properties.Settings.Default.RepeatStreamerName)
            {
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Checked = false;
            }
        }

        private void getImages()
        {

            string imgPath = GetImagesFolderPath();

            // Load images into imagelist
            try
            {
                // Create directory, is ignored if folder already exists
                Directory.CreateDirectory(imgPath);

                foreach (String path in Directory.GetFiles(imgPath))
                {
                    //Console.WriteLine(path);
                    //Console.WriteLine("Trying to add image " + Path.GetFileNameWithoutExtension(path));

                    string emoteName = Path.GetFileNameWithoutExtension(path);

                    try
                    {
                        imageList1.Images.Add(ConvertEmoteName(emoteName), Image.FromFile(path));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Failed to add emote " + emoteName + " Error: " + e.Message);
                    }

                }

                listView1.LargeImageList = imageList1;
            }
            catch (Exception e)
            {
                MessageBox.Show("No image folder found. Error: " + e.Message);
                
            }

            // Get the filtered emotes
            var filteredEmotes = Properties.Settings.Default.FilteredEmotes.Split(',');

            // Convert to list for easier use
            var emoteList = imageList1.Images.Keys.Cast<string>().ToList();

            // Add each emote except the filtered ones
            foreach (string emote in emoteList.Except(filteredEmotes))
            {
                listView1.Items.Add(emote, emote);
            }
   
        }

        private string ConvertEmoteName(string oldName)
        {
            string input = oldName;

            if (input[0] == '-' && input[1] != '-')
            {
                input = "<" + input.Substring(1);
            }
            else if(input[0] == '-' && input[1] == '-')
            {
                input = ">" + input.Substring(2);
            }


            return input;
        }

        private void ClearImagesList()
        {

            imageList1.Images.Clear();
            
        }

        private void getSavedText()
        {
            string path = GetSaveTextFolderPath();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    listView2.Items.Add(line);
                }
            }
        }

        /// <summary>
        /// Label that covers everything when locating chat box
        /// </summary>
        private void PaddAndMoveMouseLabel() 
        {
            int heightPad = (this.Height / 2) - (label12.Height / 2);
            int widthPad = (this.Width / 2) - (label12.Width / 2);

            label12.Padding = new Padding(widthPad, heightPad, widthPad, heightPad);
            label12.Location = new Point(0, -25);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Debug
            timer2.Start();

            label12.Visible = true;
            
            // Create invisible form
            Form3 form3 = new Form3();

            form3.ShowDialog();
            // Get chat box location
            Point p = form3.getMousePos;

            label12.Visible = false;

            chatBoxMouseX = p.X;
            chatBoxMouseY = p.Y;
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (numericUpDown3.Value > 0)
            {
                timer3.Interval = checkBox6.Checked ? (int)numericUpDown2.Value : (int)numericUpDown2.Value * 1000;
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
                for (int i = 0; i < (numericUpDown1.Value - 1); i++)
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

                // Add period(s) at end
                for (int i = 0; i < msgPeriodAppend; i++)
                {
                    msg += ".";
                }
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

            SendKeys.Send("{ENTER}");

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
                button3.Text = "Saved";
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

            string path = GetSaveTextFolderPath();

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

                comboBox1.Text = Properties.Settings.Default.AppliedTheme;

                if (comboBox1.Items.Count <= 0)
                {
                    List<string> ThemeNames = theme.GetAllThemes();
                    foreach (string theme in ThemeNames)
                    {
                        comboBox1.Items.Add(theme);
                    }
                }

            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            string msg = compileMessage();
            sendMessage(msg);

            numericUpDown3.Value --;
            if (checkBox2.Checked)
            {
                msgPeriodAppend++;
            }

            if (numericUpDown3.Value == 0)
            {
                timer3.Stop();
                msgPeriodAppend = 0;
            }
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


            string emoteString = wc.DownloadString(new Uri("https://raw.githubusercontent.com/AandyLi/TwitchSpammer/master/emotelinks.txt"));

            File.WriteAllText(@"emoteFile.txt", emoteString);

            ReadEmoteFile(@"emoteFile.txt");

            //string emoteFile = @"emoteFile.txt";
            //wc.DownloadFileCompleted += new AsyncCompletedEventHandler(EmoteUpdateFileComplete);
            //wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgressBar);
            //wc.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
            //wc.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/AandyLi/TwitchSpammer/master/emotelinks.txt"), emoteFile);  

        }

        private async Task ReadEmoteFile(string emoteFile)
        {

            DownloadInfo DI = new DownloadInfo(0, 0);

            bool addedNewEmotes = false;

            // Read file
            List<string> currentEmotes = new List<string>();

            foreach (String path in Directory.GetFiles(GetImagesFolderPath()))
            {
                Console.WriteLine(path);

                currentEmotes.Add(Path.GetFileNameWithoutExtension(path));
            }

            string[] lines = File.ReadAllLines(emoteFile);

            List<string> emoteNames = new List<string>();
            List<string> emoteLinks = new List<string>();


            foreach (string line in lines)
            {
                string[] splitLine = line.Split(' ');

                bool emoteFound = false;

                // Look for emote 
                foreach (string emote in currentEmotes)
                {
                    splitLine[0] = CheckSpecialCaseEmotes(splitLine[0]);
                    if (splitLine[0] == emote || splitLine[0].Contains(":"))
                    {
                        emoteFound = true;
                    }

                }

                if (!emoteFound)
                {
                    DI.TotalDownloads++;
                    addedNewEmotes = true;
                    UpdateDownloadEmoteText(DI);

                    splitLine[0] = CheckSpecialCaseEmotes(splitLine[0]);

                    // add to list
                    emoteNames.Add(splitLine[0]);
                    emoteLinks.Add(splitLine[1]);
                }
            }

            for (int i = 0; i < emoteNames.Count; i++)
            {
                // download emote
                Task<int> downloadTask = DownloadEmote(emoteNames[i], emoteLinks[i], DI);
                int result = await downloadTask;
            }

            if (addedNewEmotes)
            {
                MessageBox.Show(DI.TotalDownloads.ToString() + " new emotes were added!");
                ReloadEmotes();
            }
            else
            {
                MessageBox.Show("No new emotes were added");
            }

            if (File.Exists(emoteFile))
            {
                File.Delete(emoteFile);
            }

        }
        private string CheckSpecialCaseEmotes(string emote)
        {
            string input = emote;

            var regexItem = new Regex(@"[\/:*?<>]+");

            if (regexItem.IsMatch(input))
            {
                //Console.WriteLine("Match.. " + input);
                if (input[0] == '<')
                {
                    input = "-" + input.Substring(1);
                }
                else if(input[0] == '>')
                {
                    input = "--" + input.Substring(1);
                }
            }


            return input;
        }
        private void ReloadEmotes()
        {
            foreach (ListViewItem item in listView1.Items)
            {

                listView1.Items.Remove(item);
            }

            ClearImagesList();
            getImages();
        }
        private void UpdateDownloadEmoteText(DownloadInfo DI)
        {
            label11.Text = "Downloading emote " + DI.CurrentDownload + " of " + DI.TotalDownloads.ToString();

            float value = (float)DI.CurrentDownload / DI.TotalDownloads;
            float percentage = value * 100;

            progressBar1.Value = (int)percentage;

        }

        private async Task<int> DownloadEmote(string emote, string link, DownloadInfo DI)
        {

            string path = GetImagesFolderPath();

            WebClient wc = new WebClient();
            
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(complete);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgressBar);
            wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36");
            wc.DownloadFileAsync(new Uri(link), path + emote + ".png", DI);

            Console.WriteLine(link + " " + path + emote);

            await Task.Delay(50);
            return 1;
            
        }

        private void EmoteUpdateFileComplete(object sender, AsyncCompletedEventArgs e)
        {

            string emoteFile = @"emoteFile.txt";
            ReadEmoteFile(emoteFile);
        }

        private void complete(object sender, AsyncCompletedEventArgs e)
        {
            //MessageBox.Show("Download complete");
            DownloadInfo DI = (DownloadInfo) e.UserState;
            DI.CurrentDownload++;
            UpdateDownloadEmoteText(DI);
        }

        private void updateProgressBar(object sender, DownloadProgressChangedEventArgs e)
        {
            //progressBar1.Value = e.ProgressPercentage;
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
            string path = GetSaveTextFolderPath();

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

        private string GetImagesFolderPath()
        {
#if DEBUG

            string imgPath = "../../Images/";

            return imgPath;
#else

            string imgPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/TwitchSpammer/Images/";
            return imgPath;
#endif
        }

        private string GetSaveTextFolderPath()
        {
#if DEBUG

            string imgPath = "../../SavedText.txt";

            return imgPath;
#else
            string imgPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/TwitchSpammer/SavedText.txt";

            return imgPath;
#endif
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Initialize a new form
            Form2 form2 = new Form2();

            // Access form2's checkbox list
            CheckedListBox form2CheckedListBox = form2.checkedListBox_public;

            // Get the filtered emotes
            var filteredEmotes = Properties.Settings.Default.FilteredEmotes.Split(',');

            // Get the emote list
            var emoteList = imageList1.Images.Keys.Cast<string>().ToList();

            // Add all emotes into the checkbox list and determine check status by emote filter
            foreach (var emote in emoteList)
            {
                bool isFiltered = filteredEmotes.Contains(emote);
                form2CheckedListBox.Items.Add(emote, !isFiltered);
                
            }

            // Show form2. This will also wait until it's closed
            var dialogResult = form2.ShowDialog();

            // After form2 is closed the emotes will be reloaded
            ReloadEmotes();

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                label7.Text = "Interval (ms)";
                Properties.Settings.Default.UseMs = true;
            }
            else
            {
                label7.Text = "Interval (s)";
                Properties.Settings.Default.UseMs = false;
            }
            Properties.Settings.Default.Save();
        }


        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            theme.SetTheme(comboBox1.SelectedItem.ToString());

            Properties.Settings.Default.AppliedTheme = comboBox1.SelectedItem.ToString();

            Properties.Settings.Default.Save();
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                Properties.Settings.Default.UserSkipUpdate = false;
            }
            else
            {
                Properties.Settings.Default.UserSkipUpdate = true;
            }

            Properties.Settings.Default.Save();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                Properties.Settings.Default.AutoAddSpace = true;
            }
            else
            {
                Properties.Settings.Default.AutoAddSpace = false;
            }

            Properties.Settings.Default.Save();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                Properties.Settings.Default.RepeatStreamerName = true;
            }
            else
            {
                Properties.Settings.Default.RepeatStreamerName = false;
            }

            Properties.Settings.Default.Save();
        }
    }
}