using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Net;


/*
 
    Host themes online
    
    First time
    1. Download themes string
    2. Save local copy
    3. Parse string
    4. Apply theme
    5. Save download time

    Next time..
    1. Check if last download time > value, ex 10 days (can be done one startup). Allow user to toggle off auto theme version check
    2. If not, look for local themes file, parse and apply
    3. If last download time > value && user allows, get version number
    4. If version number is same, apply theme, else download and apply new theme
    5. Save new theme to local copy
    6. Save download time


     */


namespace TwitchSpammer
{
    class Themes
    {
        public int VersionNr { get; set; }
        public List<ThemeData> ThemesList { get; set; }
    }

    class ThemeData
    {
        public string ThemeName { get; set; }
        public List<ControlProperties> Ctrl { get; set; }
    }

    struct ControlProperties
    {
        public string ControlName { get; set; }
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
        public Font Font { get; set; }
        public FlatStyle FlatStyle { get; set; }
    }

    class Theme
    {

        private Themes allThemes;
        private Form clientForm;
        private string currentTheme;

        public Theme(Form form)
        {
            clientForm = form;

            GetThemes();

            if (Properties.Settings.Default.AppliedTheme != "")
            {
                SetTheme(Properties.Settings.Default.AppliedTheme);
            }
        }

        private void GetThemes()
        {
            // Determine wheter to download or use local themes.json
            DateTime lastDownload = Properties.Settings.Default.LastThemesDownloadDate;
            bool userSkipUpdate = Properties.Settings.Default.UserSkipUpdate;
            // Compare dates and if user wants to skip updates
            if ((DateTime.Now - lastDownload.Date).TotalDays > 10 && !userSkipUpdate )
            {
                // Download and compare versions
                DownloadThemes();
                //if (allThemes.VersionNr < GetLocalVersionNr())
                //{
                //    LoadThemesFromFile();
                //}
            }
            else
            
            { // Else, parse local file
                try
                {
                    LoadThemesFromFile();

                }
                catch (Exception)
                {
                    DialogResult dialogResult = MessageBox.Show("Could not find themes file. Do you want to download it?", "Themes not found", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        DownloadThemes();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        
                    }

                }
            }

        }

        private int GetLocalVersionNr()
        {
            string fileName = @"Themes.json";

            StreamReader sr = new StreamReader(fileName);

            string content = sr.ReadToEnd();

            Themes td = JsonConvert.DeserializeObject<Themes>(content);

            return td.VersionNr;
        }

        private void LoadThemesFromFile()
        {
            string fileName = @"Themes.json";

            StreamReader sr = new StreamReader(fileName);

            string content = sr.ReadToEnd();

            ParseThemes(content);
        }

        private void DownloadThemes()
        {
            MessageBox.Show("Downloading themes");

            WebClient wc = new WebClient();

            string themesString = wc.DownloadString(new Uri("https://raw.githubusercontent.com/AandyLi/TwitchSpammer/master/Themes.json"));

            File.WriteAllText(@"Themes.json", themesString);

            Properties.Settings.Default.LastThemesDownloadDate = DateTime.Now;

            Properties.Settings.Default.Save();

            ParseThemes(themesString);
        }

        private void ParseThemes(string content)
        {
            Themes td = JsonConvert.DeserializeObject<Themes>(content);

            allThemes = td;
        }




        public void SetTheme(string themeName)
        {
            currentTheme = themeName;

            ThemeData td = allThemes.ThemesList.Single(w => w.ThemeName == currentTheme);

            ModifyControls(td);

        }


        private void ModifyControls(ThemeData td)
        {
            foreach (var item in td.Ctrl)
            {
                if (item.ControlName == "Form")
                {
                    clientForm.BackColor = item.BackColor;
                    continue;
                }
                string fullControlName = "System.Windows.Forms" + "." + item.ControlName + ", System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

                Type controlType = Type.GetType(fullControlName);

                List<PropertyInfo> propertyList = item.GetType().GetProperties().ToList();

                foreach (Control currentControl in clientForm.Controls)
                {
                    #region childControls
                    if (currentControl.HasChildren)
                    {
                        foreach (Control groupBoxControl in currentControl.Controls)
                        {
                            if (groupBoxControl.Tag != null)
                            {
                                continue;
                            }
                            if (groupBoxControl.GetType() == controlType)
                            {
                                foreach (PropertyInfo property in propertyList.Skip(1))
                                {
                                    PropertyInfo PI = controlType.GetProperty(property.Name);

                                    try
                                    {
                                        // Should check if value can be set. Using try for now
                                        PI.SetValue(groupBoxControl, property.GetValue(item));

                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                            }
                        }
                    }

                    if (currentControl.Tag != null)
                    {
                        continue;
                    }
                    #endregion
                    if (currentControl.GetType() == controlType)
                    {
                        foreach (PropertyInfo property in propertyList.Skip(1))
                        {
                            PropertyInfo PI = controlType.GetProperty(property.Name);

                            try
                            {
                                // Should check if value can be set. Using try for now
                                PI.SetValue(currentControl, property.GetValue(item));

                            }
                            catch (Exception e)
                            {
                                
                            }
                        }
                    }
                }
            }


        }



        public List<string> GetAllThemes()
        {
            List<string> ThemeList = new List<string>();

            foreach (string item in allThemes.ThemesList.Select(w => w.ThemeName))
            {
                ThemeList.Add(item);
            }

            return ThemeList;
        }
    }
}
