﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EHRNarrative
{
    public partial class SubmenuPopover : UserControl
    {
        private EHRListBox _listBox;
        private EHRListBoxGroup _parentListItem;
        public SubmenuPopover(EHRListBoxGroup parent, Subgroup subgroup, Collection data)
        {
            InitializeComponent();
            this._parentListItem = parent;
            this._listBox = new EHRListBox();
            this._listBox.AddElements(subgroup.Elements(data));

            initialize();
        }
        public SubmenuPopover(EHRListBoxGroup parent, IEnumerable<Element> elements)
        {
            InitializeComponent();
            this._parentListItem = parent;
            this._listBox = new EHRListBox();
            this._listBox.AddElements(elements);

            initialize();
        }
        private void initialize()
        {
            this.timer1.Stop();

            this._listBox.Height = Math.Min(600, this._listBox.Items.Count * this._listBox.ItemHeight);
            this._listBox.Width = 190;
            this._listBox.Top = 5;
            this._listBox.Left = 5;

            this.Controls.Add(this._listBox);

            this.Width = 200;
            this.Height = this._listBox.Height + 10;
            this.Visible = false;
            this.BorderStyle = BorderStyle.None;
            this.BackColor = Color.LightGray;

            this.Enabled = true;
        }

        new public void Hide()
        {
            this.timer1.Start();
        }

        private void HideIfShould()
        {
            if (!ClientRectangle.Contains(PointToClient(Control.MousePosition)) && !this._parentListItem.HasMouse)
            {
                base.Hide();

                this._parentListItem.Parent.SubmenuClosed(this._parentListItem);
            }
        }

        private void SubmenuPopover_MouseLeave(object sender, EventArgs e)
        {
            this.timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.timer1.Stop();
            this.HideIfShould();
        }
    }
}
