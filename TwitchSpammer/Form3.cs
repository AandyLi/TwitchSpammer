using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchSpammer
{
    public partial class Form3 : Form
    {

        int centerOffsetToOrigoX = 0;
        int centerOffsetToOrigoY = 0;

        Point chatBoxPos;

        public Point getMousePos
        {
            get { return chatBoxPos; }
            set { chatBoxPos = value; }
        }

        public Form3()
        {
            InitializeComponent();
            centerOffsetToOrigoX = button1.Location.X + (button1.Size.Width / 2);
            centerOffsetToOrigoY = button1.Location.Y + (button1.Size.Height / 2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chatBoxPos = MousePosition;
            this.Close();
        }

        private void Form3_Activated(object sender, EventArgs e)
        {
            this.Location = new Point(MousePosition.X - centerOffsetToOrigoX, MousePosition.Y - centerOffsetToOrigoY);
        }

        private void Form3_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            this.Location = new Point(MousePosition.X - centerOffsetToOrigoX, MousePosition.Y - centerOffsetToOrigoY);
        }
    }
}
