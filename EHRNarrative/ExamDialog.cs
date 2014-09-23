﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data.Common;
using Dapper;


namespace EHRNarrative
{
    public partial class ExamDialog : Form
    {
        Collection data;

        public ExamDialog(string dialog_name, string complaint_name)
        {
            InitializeComponent();
            try
            {
                data = LoadContent(dialog_name, complaint_name);
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Not a valid dialog name or complaint");
                data = null;
                return;
            }

            this.Text = data.dialog.Name;

            RenderElements(data);
        }
        public void Show() {
            if (data == null)
                return;
            else
                base.Show();
        }

        private Collection LoadContent(string dialog_name, string complaint_name)
        {
            var data = new Collection();
            using (var conn = new SQLiteConnection("Data Source=dialog_content.sqlite3;Version=3;"))
            {
                conn.Open();
                var sql = @"
                    SELECT * FROM dialog d WHERE d.name = @Dialog;
                    SELECT * FROM 'group';
                    SELECT * FROM subgroup;
                    SELECT * FROM element;
                    SELECT * FROM group_complaints;
                    SELECT * FROM group_complaint_groups;
                    SELECT * FROM element_complaints;
                    SELECT * FROM element_complaint_groups;
                    SELECT * FROM complaintGroup;
                    SELECT * FROM complaint c WHERE c.name = @Complaint";
                using (var multi = conn.QueryMultiple(sql, new { Dialog = dialog_name, Complaint = complaint_name }))
                {
                    try
                    {
                        data = new Collection
                        {
                            dialog = multi.Read<Dialog>().Single(),
                            groups = multi.Read<Group>().ToList(),
                            subgroups = multi.Read<Subgroup>().ToList(),
                            elements = multi.Read<Element>().ToList(),
                            group_complaints = multi.Read<Group_Complaints>().ToList(),
                            group_complaint_groups = multi.Read<Group_Complaint_Groups>().ToList(),
                            element_complaints = multi.Read<Element_Complaints>().ToList(),
                            element_complaint_groups = multi.Read<Element_Complaint_Groups>().ToList(),
                            complaintgroups = multi.Read<ComplaintGroup>().ToList(),
                            complaint = multi.Read<Complaint>().Single()
                        };
                    }
                    catch
                    {
                        throw new NotImplementedException("Not a valid dialog name or complaint");
                    }
                }
                conn.Close();
            }
            return data;
        }

        private void RenderElements(Collection data) {
            int columnWidth = 200;
            int columnGutter = 20;
            int itemHeight = 50;
            int columns = data.dialog.GroupsForComplaint(data).Count();

            int rows;
            if ((columns + 1) * (columnWidth + columnGutter) < System.Windows.Forms.Screen.GetWorkingArea(this).Width)
                rows = 1;
            else
                rows = 2;

            int columnsPerRow = columns / rows;

            this.Width = columnsPerRow * (columnWidth + columnGutter) + columnWidth;
            this.Height = 50 + data.dialog.GroupsForComplaint(data).Select(x => x.ItemCount(data)).Max() * itemHeight * rows;
            //this.CenterToScreen();

            foreach (var item in data.dialog.GroupsForComplaint(data).Select((group, i) => new { i, group }))
            {
                //draw headings
                var heading = new GroupLabel();
                heading.Text = item.group.Name;
                heading.Left = columnGutter + (item.i % columnsPerRow) * (columnWidth + columnGutter);
                heading.Top = 4 + this.Height / rows * (int)(item.i / columnsPerRow);
                this.Controls.Add(heading);

                //draw multiselects
                var listbox = new EHRListBox();
                listbox.AddElements(item.group.ElementsForComplaint(data));
                listbox.AddGroups(item.group.Subgroups(data));
                if (item.group.ElementsAdditional(data).Count() > 0)
                    listbox.Items.Add(new EHRListBoxGroup());
                listbox.Left = columnGutter + (item.i % columnsPerRow) * (columnWidth + columnGutter);
                listbox.Top = 4 + heading.Height + this.Height / rows * (int)(item.i / columnsPerRow);
                this.Controls.Add(listbox);

            }
            //draw extra group column:
            foreach (var item in data.dialog.GroupsAdditional(data).Select((group, i) => new { i, group }))
            {
                AdditionalGroupsList.Items.Add(item.group.Name);
            }
                
        }

        private void InsertEHRText()
        {
            foreach (IEnumerable<Element> keywordGroup in data.elements
                .Where(x => x.selected != null)
                .OrderBy(x => x.Is_present_normal)
                .GroupBy(x => x.EHR_keyword)
                )
            {
                var keyword = keywordGroup.First().EHR_keyword;
                var EHRString = String.Join("; ", keywordGroup.Select(x => x.EHRString).ToList());
            }
        }
        private void UpdateSLC()
        {
            //??
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            InsertEHRText();
            UpdateSLC();
            this.Close();
        }


    }

    public partial class GroupLabel : Label {
        public GroupLabel()
        {
            Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold, GraphicsUnit.Point);
            ForeColor = System.Drawing.Color.DarkSlateGray;
        }
    }

}
