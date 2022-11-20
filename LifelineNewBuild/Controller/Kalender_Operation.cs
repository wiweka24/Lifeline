﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Bunifu.UI.WinForms;

namespace LifelineNewBuild
{
    public partial class Kalender : Form
    {
        private List<BunifuPanel> listFLDays = new List<BunifuPanel>();
        private List<BunifuPanel> listFLActs = new List<BunifuPanel>();
        private List<LinkLabel> listLabel = new List<LinkLabel>();
        private DateTime currentDate = DateTime.Today;

        private NpgsqlDataReader dr;
        private DataTable activity;

        private void GetActivityTable()
        {
            try
            {
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = string.Format("SELECT * FROM activity WHERE act_user_id = '{0}' ORDER BY act_date", currentUser);

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    activity = new DataTable();
                    activity.Load(dr);
                }
                con.Dispose();
                con.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show("Error: " + err.Message, "FAIL!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetFirstDayOfWeekOfCurrentDate()
        {
            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            return (int)(firstDayOfMonth.DayOfWeek + 1);
        }

        private int GetTotalDaysOfCurrentDate()
        {
            DateTime firstDayOfCurrentDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            return firstDayOfCurrentDate.AddMonths(1).AddDays(-1).Day;
        }

        private void DisplayCurrentDate()
        {
            InitializeConnection();
            GetActivityTable();

            lblMonthAndYear.Text = currentDate.ToString("MMMM yyyy");
            int firstDayAtFlNumber = GetFirstDayOfWeekOfCurrentDate();
            int totalDay = GetTotalDaysOfCurrentDate();

            AddLabelDay(firstDayAtFlNumber, totalDay);
            AddAppointmentAndToday(firstDayAtFlNumber);
            if (activity != null)
            {
                AddUpcomingAct();
            }
        }

        private void AddAppointmentAndToday(int startDayAtFlNumber)
        {
            DateTime startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            int DayInterval = 1;

            while (startDate <= endDate)
            {
                if (activity != null)
                {
                    AddAppointmentToFlDay(startDate, startDayAtFlNumber);
                }

                if (startDate == DateTime.Today)
                {
                    listFLDays[(startDate.Day - 1) + (startDayAtFlNumber - 1)].BackColor = Color.FromArgb(59, 130, 246);
                }

                startDate = startDate.AddDays(DayInterval);
            }
        }

        private void LinkOpenAct(object sender)
        {
            LinkLabel link = sender as LinkLabel;
            link.LinkVisited = true;
            string actName = link.Text;

            DataRow[] query = activity.Select("act_name = '" + actName + "'");

            foreach (DataRow item in query)
            {
                ActForm aktivitas = new ActForm(item["act_name"].ToString(), item["act_type"].ToString(), item["act_date"].ToString(), item["act_desc"].ToString(), item["act_id"].ToString(), currentUser);
                aktivitas.ShowDialog();
            }
        }

        private void AddAppointmentToFlDay(DateTime startDate, int startDayAtFlNumber)
        {
            string date = startDate.ToString("dd-MM-yyyy");
            DataRow[] query = activity.Select("act_date = '" + date + "'");
            foreach (DataRow item in query)
            {
                DateTime appDay = startDate;
                LinkLabel link = new LinkLabel();
                link.Text = item["act_name"].ToString();
                link.Click += new EventHandler(link_Clicked);
                link.LinkColor = Color.Black;
                link.Font = new Font("Segoe UI Semibold", 10);
                link.TextAlign = ContentAlignment.MiddleCenter;
                listLabel.Add(link);
                listFLDays[(appDay.Day - 1) + (startDayAtFlNumber - 1)].Controls.Add(link);
            }
        }

        private void GenerateDayPanel(int totalDays)
        {
            for (int i = 1; i <= totalDays; i++)
            {
                BunifuPanel fl = new BunifuPanel();
                fl.Name = $"flDay{i}";
                fl.Size = new Size(136, 136);
                fl.Margin = new Padding(5, 5, 5, 5);
                fl.Padding = new Padding(5, 5, 5, 5);
                flDays.Controls.Add(fl);
                listFLDays.Add(fl);
            }
        }

        private void AddLabelDay(int StartDay, int totalDayInMonth)
        {
            foreach (BunifuPanel fl in listFLDays)
            {
                fl.Controls.Clear();
                fl.BackColor = Color.FromArgb(248, 250, 252);
            }

            for (int i = 1; i <= totalDayInMonth; i++)
            {
                Label lbl = new Label();
                lbl.Name = $"lblDay{i}";
                lbl.AutoSize = false;
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Size = new Size(136, 20);
                lbl.Text = i.ToString();
                lbl.Font = new Font("Segoe UI Semibold", 10);

                listFLDays[(i - 1) + (StartDay - 1)].Controls.Add(lbl);
                listFLDays[(i - 1) + (StartDay - 1)].BackColor = Color.FromArgb(209, 220, 250);
            }
        }

        private void GenerateUpcomingActPanel(int totalAct)
        {
            for (int i = 1; i <= totalAct; i++)
            {
                BunifuPanel fl = new BunifuPanel();
                fl.Name = $"flDay{i}";
                fl.Size = new Size(200, 75);
                fl.Margin = new Padding(5, 5, 5, 5);
                flUpcomingAct.Controls.Add(fl);
                listFLActs.Add(fl);
            }
        }

        private void AddUpcomingAct()
        {
            DataTable item = new DataTable();
            int k = 0;

            foreach (BunifuPanel fl in listFLActs)
            {
                fl.Controls.Clear();
                fl.BackColor = Color.Transparent;
            }

            foreach (DataRow row in activity.Rows)
            {
                string[] date = row["act_date"].ToString().Split('-');
                int[] date_int = Array.ConvertAll(date, int.Parse);

                DateTime date_date = new DateTime(date_int[2], date_int[1], date_int[0]);
                if (date_date >= currentDate && k < 4)
                {
                    LinkLabel link = new LinkLabel();
                    link.Text = row["act_name"].ToString();
                    link.Click += new EventHandler(link_Clicked);
                    link.LinkColor = Color.Black;
                    listLabel.Add(link);

                    Label txtbx_date = new Label();
                    txtbx_date.Text = row["act_date"].ToString();

                    listFLActs[k].BackColor = Color.White;
                    listFLActs[k].Controls.Add(link);
                    listFLActs[k].Controls.Add(txtbx_date);

                    k++;
                }
            }
        }
    }
}
