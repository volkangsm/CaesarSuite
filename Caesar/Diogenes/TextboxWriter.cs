﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Diogenes
{
    public class TextboxWriter : TextWriter
    {
        private TextBox InputTextbox;
        private Timer timer;
        private StringBuilder sb = new StringBuilder();
        private bool dirty = false;
        private readonly object WriteLock = new object();

        public TextboxWriter(TextBox inputTextbox)
        {
            InputTextbox = inputTextbox;
            timer = new Timer();
            timer.Interval = 30;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // using a timer since invoking an event on every character write gets expensive quickly
            if (dirty || (InputTextbox.Text.Length != sb.Length))
            {
                InputTextbox.Text = sb.ToString();
                dirty = false;
                InputTextbox.SelectionStart = InputTextbox.TextLength;
                InputTextbox.ScrollToCaret();
            }
        }

        // fix append for crossthread : https://stackoverflow.com/questions/12645351/stringbuilder-tostring-throw-an-index-out-of-range-exception
        public override void Write(char value)
        {
            lock (WriteLock)
            {
                sb.Append(value);
                dirty = true;
            }
        }

        public override void Write(string value)
        {
            lock (WriteLock)
            {
                sb.Append(value);
                dirty = true;
            }
        }

        public void Clear()
        {
            sb = new StringBuilder();
            dirty = true;
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }
    }
}
