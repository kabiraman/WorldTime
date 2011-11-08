/* 
 * Name: FormMain.cs
 * Programmed by: Karthik Abiraman (kmabiraman@gmail.com)
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace WorldTime
{
    /// <summary>
    /// This form is the startup form for WorldTime.  This form is the GUI 
    /// for the WorldTime application.  This is the form through which the 
    /// user interacts with the application.
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// The connection to the WorldTime database.  This is maintained 
        /// until the application is closed.
        /// </summary>
        private SQLiteConnection m_dbConnection;

        /// <summary>
        /// Variable used throughout the app to create SQL commands.
        /// </summary>
        private SQLiteCommand m_dbCommand;
        
        /// <summary>
        /// Variable used throughout the app to execute queries.
        /// </summary>
        private SQLiteDataReader m_dr;

        /*
         * Arrays declared below to store ids so that a query can be run 
         * against IDs and not names.
         */

        /// <summary>
        /// Stores IDs of cities.
        /// </summary>
        private int[] m_intCities;
        
        /// <summary>
        /// Stores IDs of 'My Cities'.
        /// </summary>
        private int[] m_intMyCities;

        /// <summary>
        /// Stores IDs of source cities in Meeting Planner.
        /// </summary>
        private int[] m_intSCity;
        
        /// <summary>
        /// Stores IDs of destination cities in Meeting Planner.
        /// </summary>
        private int[] m_intDCities;

        /// <summary>
        /// The selected city in 'All Cities'.
        /// </summary>
        private SelectedCity m_objSelectedCity = new SelectedCity();

        /// <summary>
        /// The selected city in 'My Cities'.
        /// </summary>
        private SelectedCity m_objMySelectedCity = new SelectedCity();

        /// <summary>
        /// The application ToolTip.
        /// </summary>
        private ToolTip m_toolTip;

        /// <summary>
        /// Whether the balloon tip is currently being shown.
        /// </summary>
        private bool m_bIsBalloonTipBeingShown = false;

        /// <summary>
        /// Whether the user has specified the System Tray Icon should be 
        /// shown.
        /// </summary>
        private bool m_bShowSystemTrayIcon;

        /// <summary>
        /// Whether the user has specified the app should minimze to the 
        /// System Tray.
        /// </summary>
        private bool m_bMinimizeToTray;

        /// <summary>
        /// The Date Format the app should use as specified by the user.
        /// </summary>
        private string m_strDateFormat;

        /// <summary>
        /// The Time Format the app should use as specified by the user.
        /// </summary>
        private string m_strTimeFormat;

        #region Form Initialization/Uninitialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens connection to the database, initializes objects, populates 
        /// all cities and all 'My Cities'.
        /// </summary>
        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                ReadSettings();

                m_toolTip = new ToolTip();

                m_toolTip.IsBalloon = true;
                m_toolTip.SetToolTip(this.m_txtSearchCity,
                    "Press Enter to search for a city");

                if (System.IO.File.Exists(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData) +
                    System.IO.Path.DirectorySeparatorChar + "WorldTime\\WorldTime.db"))
                {
                    m_dbConnection = new SQLiteConnection("Data Source=" +
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                        System.IO.Path.DirectorySeparatorChar + "WorldTime\\WorldTime.db");
                }
                else
                {
                    m_dbConnection = new SQLiteConnection("Data Source=" +
                        System.AppDomain.CurrentDomain.BaseDirectory +
                        "\\WorldTime.db");
                }
                
                m_dbConnection.Open();

                m_txtVersion.Text = "0.9.11";

                bool bDoMyCitiesExist = PopulateMyCities();
                PopulateAllCities();
                PopulateMeetingPlanner();

                // select appropriate tab
                if (bDoMyCitiesExist)
                {
                    m_tabControl.SelectedIndex = 0;
                }
                else
                {
                    m_tabControl.SelectedIndex = 1;
                }

                DateTime dt = DateTime.Now;

                // the various Date formats the app supports
                m_rbWorldTime.Text = dt.DayOfWeek + ", " +
                    WorldTime.FormatMonth(dt.Month) + " " +
                    dt.Day + ", " + dt.Year.ToString();
                m_rbSystemCultureShort.Text = dt.ToShortDateString();
                m_rbSystemCultureLong.Text = dt.ToLongDateString();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        private void FormMain_Activated(object sender, System.EventArgs e)
        {
            this.ShowInTaskbar = true;            
        }

        private void FormMain_Deactivate(object sender, System.EventArgs e)
        {
            if (m_bMinimizeToTray)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.ShowInTaskbar = false;
                }
                else
                {
                    this.ShowInTaskbar = true;
                }
            }
        }        

        private void FormMain_FormClosing(object sender, EventArgs e)
        {
            m_dbConnection.Close();
            m_dbConnection.Dispose();
        }

        private void ReadSettings()
        {
            m_bShowSystemTrayIcon = m_chkShowSystemTrayIcon.Checked =
                Properties.Settings.Default.ShowSystemTrayIcon;
            m_bMinimizeToTray = m_chkMinimzeToTray.Checked = 
                Properties.Settings.Default.MinimizeToSystemTray;

            if (m_bShowSystemTrayIcon)
            {
                m_notifyIcon.Visible = true;                
            }
            else
            {
                m_bMinimizeToTray = m_chkMinimzeToTray.Enabled = false;
            }

            if (Properties.Settings.Default.UseStandardTimeFormat)
            {
                m_rbStandardTimeFormat.Checked = true;
                m_strTimeFormat = "en-CA";
            }
            else
            {
                m_rbMilitaryTimeFormat.Checked = true;
                m_strTimeFormat = "es-ES";
            }

            m_strDateFormat = Properties.Settings.Default.DateFormat;

            if (m_strDateFormat == "WorldTime")
            {
                m_rbWorldTime.Checked = true;
            }
            else if (m_strDateFormat ==
                "SystemCultureLong")
            {
                m_rbSystemCultureLong.Checked = true;
            }
            else
            {
                m_rbSystemCultureShort.Checked = true;
            }
        }

        /// <summary>
        /// Gets all of the user's cities and populates the Listview.
        /// </summary>
        /// <returns>True, if there are any 'My Cities'.</returns>
        private bool PopulateMyCities()
        {
            try
            {
                int i = 0;

                m_lvMyCities.Items.Clear();

                // get the count of my cities
                m_dbCommand = new SQLiteCommand(Queries.GetMyCitiesCount(), 
                    m_dbConnection);

                // declare the size of the array to the count of 'My Cities'
                m_intMyCities = new int[int.Parse
                    (m_dbCommand.ExecuteScalar().ToString())];

                m_dbCommand.Dispose();

                // get all 'My Cities'
                m_dbCommand = new SQLiteCommand(
                    Queries.GetMyCities(), m_dbConnection);

                m_dr = m_dbCommand.ExecuteReader();

                if (m_dr.HasRows)
                {
                    while (m_dr.Read())
                    {
                        ListViewItem item;

                        // populate the listview with the city name
                        if (m_dr[2].ToString() == "System")
                        {
                            item = new ListViewItem(
                                m_dr[1].ToString() + ", " +
                                m_dr[3].ToString());                            
                        }
                        else
                        {
                            item = new ListViewItem(
                                m_dr[1].ToString() + ", " +
                                m_dr[2].ToString() + ", " +
                                m_dr[3].ToString());
                        }

                        m_lvMyCities.Items.Add(item);

                        // store the city id in the array
                        m_intMyCities[i] = int.Parse(m_dr[0].ToString());

                        int intHasDST = int.Parse(m_dr[6].ToString());
                        item.SubItems.Add(
                            WorldTime.GetTime(m_dr[5].ToString(), intHasDST,
                        m_dr[7].ToString(), m_dr[8].ToString(), DateTime.Now,
                        TimeZone.CurrentTimeZone.GetUtcOffset
                        (DateTime.Now).TotalHours.ToString(),
                        m_strDateFormat, m_strTimeFormat));


                        i += 1;
                    }

                    m_lvMyCities.Items[0].Selected = true;
                }
                else
                {
                    m_btnRemove.Enabled = false;
                }

                m_dr.Close();
                m_dr.Dispose();

                m_dbCommand.Dispose();

                UpdateMyCitiesCount();

                return m_btnRemove.Enabled;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }

            return false;
        }

        /// <summary>
        /// Gets all cities and populates the listbox in tab 'All Cities' and 
        /// the ListBox and ComboBox in 'Meeting Planner'.
        /// </summary>
        private void PopulateAllCities()
        {
            try
            {
                int i = 0;

                m_lstCities.Items.Clear();

                // get the city count
                m_dbCommand = new SQLiteCommand(Queries.GetAllCitiesCount(), 
                    m_dbConnection);

                // declare the array to size of city count
                m_intCities = new int[int.Parse(
                    m_dbCommand.ExecuteScalar().ToString())];

                // declare the array to size of city count
                m_intSCity = new int[int.Parse(
                    m_dbCommand.ExecuteScalar().ToString())];
                
                // declare the array to the size of city count
                m_intDCities = new int[int.Parse(
                    m_dbCommand.ExecuteScalar().ToString())];

                m_dbCommand.Dispose();

                // get all cities
                m_dbCommand = new SQLiteCommand(Queries.GetAllCities(), 
                    m_dbConnection);

                m_dr = m_dbCommand.ExecuteReader();

                if (m_dr.HasRows)
                {
                    while (m_dr.Read())
                    {
                        m_lstCities.Items.Add(m_dr[1].ToString());

                        // add cities to meeting planner
                        if (m_dr[2].ToString() == "System")
                        {
                            cboSCity.Items.Add(m_dr[1].ToString() + ", " + 
                                m_dr[3].ToString());
                            
                            m_lstDCities.Items.Add(m_dr[1].ToString() + ", " +
                                m_dr[3].ToString());
                        }
                        else
                        {
                            cboSCity.Items.Add(m_dr[1].ToString() + ", " +
                            m_dr[2].ToString() + ", " + m_dr[3].ToString());
                            
                            m_lstDCities.Items.Add(m_dr[1].ToString() + ", " +
                                m_dr[2].ToString() + ", " + m_dr[3].ToString());
                        }

                        cboSCity.SelectedIndex = 0;

                        // store the city id
                        m_intCities[i] = int.Parse(m_dr[0].ToString());
                        m_intSCity[i] = int.Parse(m_dr[0].ToString());
                        m_intDCities[i] = int.Parse(m_dr[0].ToString());

                        i += 1;
                    }
                }

                m_dr.Close();
                m_dr.Dispose();

                m_dbCommand.Dispose();

                // update the city count in tab 'All Cities'
                UpdateCityCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Initializes all controls in the Meeting Planner tab.
        /// </summary>
        private void PopulateMeetingPlanner()
        {
            int i = 1;

            for (i = 1; i < 32; i++)
            {
                m_cboSDay.Items.Add(i.ToString());
            }

            m_cboSDay.SelectedIndex = DateTime.Now.Day - 1;

            for (i = 1; i < 13; i++)
            {
                m_cboSMonth.Items.Add(WorldTime.FormatMonth(i));
            }

            m_cboSMonth.SelectedIndex = DateTime.Now.Month - 1;

            int intYearLimit = DateTime.Now.Year + 11;

            for (i = DateTime.Now.Year; i < intYearLimit; i++)
            {
                m_cboSYear.Items.Add(i.ToString());
            }

            m_cboSYear.SelectedIndex = 0;

            for (i = 0; i < 24; i++)
            {
                m_cboSourceCityHour.Items.Add(i.ToString());
            }

            m_cboSourceCityHour.SelectedIndex = DateTime.Now.Hour;            

            for (i = 0; i < 60; i++)
            {
                m_cboSMin.Items.Add(i.ToString());
            }

            m_cboSMin.SelectedIndex = DateTime.Now.Minute;
        }

        #endregion

        /// <summary>
        /// Once a city is selected, call GetCityDetails().
        /// </summary>
        private void CitiesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // make sure a city is selected
            if (m_lstCities.Items.Count > 0 && (m_lstCities.SelectedIndex != -1))
            {
                // get the city's time-related info
                GetCityDetails("", 
                    m_intCities[m_lstCities.SelectedIndex]);

                m_tmrWT.Enabled = true;
            }
        }

        /// <summary>
        /// Gets all city details and calculates time and other time-related 
        /// info.
        /// </summary>
        private void GetCityDetails(string strForWhat, int intCityID)
        {
            try
            {
                if (intCityID == -1 && strForWhat == "My Cities")
                {
                    m_grbMyDateTime.Text = "Date and Time";
                    this.Text = "WorldTime";

                    m_txtMyTime.Text = txtMySTimezone.Text = m_txtMyCTimezone.Text =
                        m_objMySelectedCity.Offset = 
                        m_objMySelectedCity.DSTStart =
                        m_objMySelectedCity.DSTEnd = m_txtMyDSTStart.Text =
                        m_txtMyDSTEnd.Text = m_txtMyTimezoneName.Text = m_txtMyPhoneCode.Text =
                        m_txtMyCapital.Text = "";

                    return;
                }

                // get city's details
                m_dbCommand = new SQLiteCommand(Queries.GetCityDetails
                    (intCityID), m_dbConnection);

                m_dr = m_dbCommand.ExecuteReader();

                if (m_dr.HasRows)
                {
                    while (m_dr.Read())
                    {
                        StringBuilder strState;
                        
                        // proceed with 'My Cities'
                        if (strForWhat == "My Cities")
                        {
                            if (m_dr[10].ToString() == "System")
                            {
                                strState = new StringBuilder("");
                            }
                            else
                            {
                                strState = new StringBuilder(
                                    m_dr[10].ToString() +
                                    ", ");
                            }

                            m_objMySelectedCity.HasDST = 
                                int.Parse(m_dr[2].ToString());

                            // city, state, country, continent
                            m_grbMyDateTime.Text = m_dr[5].ToString() + ", " + 
                                strState + m_dr[11].ToString() + ", " + 
                                m_dr[12].ToString();

                            txtMySTimezone.Text = "GMT" + 
                                WorldTime.FormatOffset(m_dr[1].ToString());

                            m_txtMyCTimezone.Text = "GMT" + 
                                WorldTime.GetCurrentOffset(m_dr[1].ToString(),
                                m_objMySelectedCity.HasDST,
                                m_dr[3].ToString(), m_dr[4].ToString(), true);

                            m_objMySelectedCity.Offset = m_dr[1].ToString();
                            m_objMySelectedCity.DSTStart = m_dr[3].ToString();
                            m_objMySelectedCity.DSTEnd = m_dr[4].ToString();

                            GetTime(strForWhat);

                            // if the city has DST, calculate it
                            if (m_objMySelectedCity.HasDST == 1)
                            {
                                m_txtMyDSTStart.Text = WorldTime.FormatDST(
                                    m_dr[3].ToString(),
                                    m_dr[1].ToString(), true);

                                if (m_dr[4].ToString() == "")
                                {
                                    m_txtMyDSTEnd.Text = "No Daylight Saving Time";
                                }
                                else
                                {
                                    m_txtMyDSTEnd.Text = WorldTime.FormatDST(
                                        m_dr[4].ToString(),
                                        m_dr[1].ToString(), false);
                                }
                            }
                            else
                            {
                                m_txtMyDSTStart.Text = m_txtMyDSTEnd.Text =
                                    "No Daylight Saving Time";
                            }

                            if (m_dr[0].ToString() == "System")
                            {
                                m_txtMyTimezoneName.Text = "N/A";
                                m_toolTip.SetToolTip(m_txtMyTimezoneName, String.Empty);
                            }
                            else
                            {
                                m_txtMyTimezoneName.Text = m_dr[0].ToString();
                                m_toolTip.SetToolTip(m_txtMyTimezoneName, m_dr[0].ToString());
                            }
                            
                            m_txtMyPhoneCode.Text = "+" + m_dr[7].ToString();

                            // if there is an area code, display it
                            if (m_dr[8].ToString().Length > 0)
                            {
                                m_txtMyPhoneCode.Text += " (" + 
                                    m_dr[8].ToString() + ")";
                            }

                            // if the city is a capital
                            if (m_dr[6].ToString() == "1")
                            {
                                m_txtMyCapital.Text = m_dr[5].ToString() + 
                                    " is the capital of " +
                                    m_dr[11].ToString();
                            }
                            else
                            {
                                m_txtMyCapital.Text = "";
                            }
                        }
                        else
                        {
                            // if there is no state for this city
                            if (m_dr[10].ToString() == "System")
                            {
                                strState = new StringBuilder("");
                            }
                            else
                            {
                                strState = new StringBuilder(
                                    m_dr[10].ToString() +
                                    ", ");
                            }

                            m_objSelectedCity.HasDST = int.Parse(m_dr[2].ToString());
                            
                            m_grbDateTime.Text = m_dr[5].ToString() + ", " +
                                strState + m_dr[11].ToString() + ", " +
                                m_dr[12].ToString();

                            m_txtSTimezone.Text = "GMT" + WorldTime.FormatOffset(
                                m_dr[1].ToString());

                            m_txtCTimezone.Text = "GMT" + WorldTime.GetCurrentOffset(
                                m_dr[1].ToString(),
                                m_objSelectedCity.HasDST,
                                m_dr[3].ToString(), m_dr[4].ToString(), true);

                            m_objSelectedCity.Offset = m_dr[1].ToString();
                            m_objSelectedCity.DSTStart = m_dr[3].ToString();
                            m_objSelectedCity.DSTEnd = m_dr[4].ToString();

                            GetTime(strForWhat);

                            // if the city has DST, calculate it
                            if (m_objSelectedCity.HasDST == 1)
                            {
                                m_txtDSTStart.Text = WorldTime.FormatDST(
                                    m_dr[3].ToString(), m_dr[1].ToString(), true);

                                if (m_dr[4].ToString() == "")
                                {
                                    m_txtDSTEnd.Text = "No Daylight Saving Time";
                                }
                                else
                                {
                                    m_txtDSTEnd.Text = WorldTime.FormatDST(m_dr[4].ToString(),
                                        m_dr[1].ToString(), false);
                                }
                            }
                            else
                            {
                                m_txtDSTStart.Text = m_txtDSTEnd.Text = "No Daylight Saving Time";
                            }

                            if (m_dr[0].ToString() == "System")
                            {
                                m_txtTimezoneName.Text = "N/A";
                                m_toolTip.SetToolTip(m_txtTimezoneName, String.Empty);
                            }
                            else
                            {
                                m_txtTimezoneName.Text = m_dr[0].ToString();
                                m_toolTip.SetToolTip(m_txtTimezoneName, m_dr[0].ToString());
                            }

                            m_txtPhoneCode.Text = "+" + m_dr[7].ToString();

                            // if there is an area code, display it
                            if (m_dr[8].ToString().Length > 0)
                            {
                                m_txtPhoneCode.Text += " (" + m_dr[8].ToString() + ")";
                            }

                            // if the city is a capital
                            if (m_dr[6].ToString() == "1")
                            {
                                m_txtCapital.Text = m_dr[5].ToString() +
                                    " is the capital of " + m_dr[11].ToString();
                            }
                            else
                            {
                                m_txtCapital.Text = "";
                            }
                        }
                    }
                }

                m_dr.Close();
                m_dr.Dispose();

                m_dbCommand.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        #region Timers

        /// <summary>
        /// Update the time based on which tab is selected.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (m_tabControl.SelectedIndex == 0)
            {
                if (m_lvMyCities.Items.Count > 0)
                {
                    if (m_lvMyCities.SelectedIndices.Count > 0)
                    {
                        GetTime("My Cities");
                    }
               }                
            }
            else
            {
                if (m_lstCities.Items.Count > 0 && 
                    (m_lstCities.SelectedIndex != -1))
                {
                    GetTime("");
                }
            }
        }

        private void MyCitiesTimer_Tick(object sender, EventArgs e)
        {
            if (m_tabControl.SelectedIndex == 0 &&
                m_lvMyCities.Items.Count > 0)
            {
                int i = 0;

                try
                {
                    m_lvMyCities.SuspendLayout();

                    foreach (int intCityID in m_intMyCities)
                    {
                        // get city's details
                        m_dbCommand = new SQLiteCommand(Queries.GetCityDetails(
                            intCityID), m_dbConnection);

                        m_dr = m_dbCommand.ExecuteReader();

                        if (m_dr.HasRows)
                        {
                            while (m_dr.Read())
                            {
                                int intHasDST = int.Parse(m_dr[2].ToString());

                                m_lvMyCities.Items[i].SubItems[1].Text =
                                    WorldTime.GetTime(m_dr[1].ToString(), intHasDST,
                                    m_dr[3].ToString(), m_dr[4].ToString(), DateTime.Now,
                                    TimeZone.CurrentTimeZone.GetUtcOffset
                                    (DateTime.Now).TotalHours.ToString(),
                                    m_strDateFormat,
                                    m_strTimeFormat);

                            }
                        }

                        i += 1;
                    }
                }
                finally
                {
                    m_lvMyCities.ResumeLayout();

                    m_dr.Close();
                    m_dr.Dispose();

                    m_dbCommand.Dispose();
                }
            }
        }

        #endregion

        /// <summary>
        /// When the user selects a city, displays its details.
        /// </summary>
        private void MyCitiesList_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            if (m_lvMyCities.Items.Count > 0 &&
                (m_lvMyCities.SelectedIndices.Count > 0))
            {
                m_btnRemove.Enabled = true;

                GetCityDetails("My Cities",
                    m_intMyCities[m_lvMyCities.SelectedIndices[0]]);

                m_tmrWT.Enabled = true;
            }
            else
            {
                m_btnRemove.Enabled = false;
            }
        }

        /// <summary>
        /// Adds a city to 'My Cities'.
        /// </summary>
        private void AddButton_Click(object sender, EventArgs e)
        {
            if ((m_lstCities.Items.Count > 0) &&
                (m_lstCities.SelectedItems.Count > 0))
            {
                try
                {
                    // check if the city already exists in the database table
                    m_dbCommand = new SQLiteCommand(
                        Queries.CheckIfCityExists(
                    m_intCities[m_lstCities.SelectedIndex]), m_dbConnection);

                    if (m_dbCommand.ExecuteScalar().ToString() == "0")
                    {
                        m_dbCommand.Dispose();

                        // add the city to 'My Cities'
                        m_dbCommand = new SQLiteCommand(Queries.AddToMyCities
                        (m_intCities[m_lstCities.SelectedIndex]), m_dbConnection);

                        m_dbCommand.ExecuteNonQuery();

                        m_dbCommand.Dispose();

                        PopulateMyCities();
                    }
                    else
                    {
                        m_dbCommand.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "WorldTime",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }
        
        /// <summary>
        /// Remove a city from 'My Cities'.
        /// </summary>
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (m_lvMyCities.Items.Count > 0 &&
                (m_lvMyCities.SelectedIndices.Count > 0))
            {
                try
                {
                    // delete the city from 'My Cities'
                    m_dbCommand = new SQLiteCommand(Queries.DeleteFromMyCities
                    (m_intMyCities[m_lvMyCities.SelectedIndices[0]]), m_dbConnection);

                    m_dbCommand.ExecuteNonQuery();

                    m_dbCommand.Dispose();

                    // clear fields
                    GetCityDetails("My Cities", -1);

                    // refresh the listview
                    PopulateMyCities();                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        private void UpdateCityCount()
        {
            m_lblAllCities.Text = "City List: (" + 
                m_lstCities.Items.Count.ToString() + ")";
        }

        /// <summary>
        /// Updates the count of 'My Cities' displayed in the 'My Cities' tab.
        /// </summary>
        private void UpdateMyCitiesCount()
        {
            m_tabMyCities.Text = "My Cities (" + 
                m_lvMyCities.Items.Count + ")";
        }

        /// <summary>
        /// When the user presses ENTER, search for the city in the database 
        /// and display it.
        /// </summary>
        private void SearchCityTextBox_KeyDown(
            object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) && (m_txtSearchCity.Text.Length > 3))
            {
                try
                {
                    m_objSelectedCity.CityID = 0;

                    // check to see if city exists
                    m_dbCommand = new SQLiteCommand(Queries.FindCity(
                        m_txtSearchCity.Text), m_dbConnection);

                    m_dr = m_dbCommand.ExecuteReader();

                    if (m_dr.HasRows)
                    {
                        while (m_dr.Read())
                        {
                            // store the city id, is 0 if city doesn't exist
                            m_objSelectedCity.CityID = 
                                int.Parse(m_dr[0].ToString());
                        }
                    }

                    m_dr.Close();
                    m_dr.Dispose();

                    m_dbCommand.Dispose();

                    if (m_objSelectedCity.CityID != 0)
                    {
                        m_lstCities.SelectedIndex = -1;

                        GetCityDetails("", m_objSelectedCity.CityID);

                        // refresh the listbox
                        PopulateAllCities();

                        int intCount = m_lstCities.Items.Count;

                        for (int i = 0; i < intCount; i++)
                        {
                            if (m_lstCities.Items[i].ToString().ToLower() ==
                                m_txtSearchCity.Text.ToLower())
                            {
                                m_lstCities.SelectedIndex = i;
                            }
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Is called by the timer event and updates the time based on the 
        /// tab selected by the user.
        /// </summary>
        /// <param name="strForWhat">
        /// Specifies whether to update time for 'My Cities' or for 
        /// 'All Cities'.
        /// </param>
        private void GetTime(string strForWhat)
        {
            if (strForWhat == "My Cities")
            {
                m_txtMyTime.Text = WorldTime.GetTime(
                    m_objMySelectedCity.Offset, 
                    m_objMySelectedCity.HasDST, 
                    m_objMySelectedCity.DSTStart, 
                    m_objMySelectedCity.DSTEnd,
                    DateTime.Now, 
                    TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours.ToString(), 
                    m_strDateFormat, 
                    m_strTimeFormat);

                this.Text = "WorldTime - " + m_grbMyDateTime.Text.Substring(
                    0, m_grbMyDateTime.Text.IndexOf(',')) + ": " + 
                    m_txtMyTime.Text;
            }
            else
            {
                m_txtTime.Text = WorldTime.GetTime(
                    m_objSelectedCity.Offset,
                    m_objSelectedCity.HasDST, m_objSelectedCity.DSTStart, 
                    m_objSelectedCity.DSTEnd,
                    DateTime.Now, TimeZone.CurrentTimeZone.GetUtcOffset
                    (DateTime.Now).TotalHours.ToString(), m_strDateFormat, 
                    m_strTimeFormat);

                if (m_lstCities.SelectedItem != null)
                {
                    this.Text = "WorldTime - " +
                        m_lstCities.SelectedItem.ToString() + ": " +
                        m_txtTime.Text;
                }
            }
        }

        /// <summary>
        /// Calculate and display the time for each Destination City selected 
        /// in the Meeting Planner.
        /// </summary>
        private void GetTimeButton_Click(object sender, EventArgs e)
        {
            DateTime dtSCity;
            string strSourceOffset = "";
            int intSourceHasDST = 0;
            string strSourceDSTStart = "";
            string strSourceDSTEnd = "";

            m_lvMeetingPlanner.Items.Clear();

            try
            {
                foreach (int intSelectedIndex in m_lstDCities.SelectedIndices)
                {
                    string strSCity = "";

                    strSCity = m_cboSDay.SelectedItem.ToString() + "/" +
                        m_cboSMonth.SelectedItem.ToString() + "/" +
                        m_cboSYear.SelectedItem.ToString() + " " +
                        m_cboSourceCityHour.SelectedItem.ToString() + ":" +
                        m_cboSMin.SelectedItem.ToString();

                    dtSCity = DateTime.Parse(strSCity);

                    m_dbCommand = new SQLiteCommand(Queries.GetCityDetails
                        (m_intSCity[cboSCity.SelectedIndex]), m_dbConnection);

                    m_dr = m_dbCommand.ExecuteReader();

                    if (m_dr.HasRows)
                    {
                        while (m_dr.Read())
                        {
                            strSourceOffset = m_dr[1].ToString();
                            intSourceHasDST = int.Parse(m_dr[2].ToString());
                            strSourceDSTStart = m_dr[3].ToString();
                            strSourceDSTEnd = m_dr[4].ToString();
                        }
                    }

                    m_dr.Close();
                    m_dr.Dispose();

                    m_dbCommand.Dispose();

                    m_dbCommand = new SQLiteCommand(Queries.GetCityDetails
                        (m_intDCities[intSelectedIndex]), m_dbConnection);

                    m_dr = m_dbCommand.ExecuteReader();

                    if (m_dr.HasRows)
                    {
                        int intHasDST = 0;

                        while (m_dr.Read())
                        {
                            intHasDST = int.Parse(m_dr[2].ToString());

                            ListViewItem item = m_lvMeetingPlanner.Items.Add(
                                m_lstDCities.Items[intSelectedIndex].ToString());

                            item.SubItems.Add(
                                WorldTime.GetTime(m_dr[1].ToString(),
                                intHasDST, m_dr[3].ToString(), m_dr[4].ToString(),
                                dtSCity,
                                WorldTime.GetCurrentOffset(
                                strSourceOffset, intSourceHasDST, strSourceDSTStart, strSourceDSTEnd, dtSCity, false), 
                                m_strDateFormat, 
                                m_strTimeFormat));
                        }
                    }

                    m_dr.Close();
                    m_dr.Dispose();

                    m_dbCommand.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WorldTime", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        private void DestinationCities_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            if (m_lstDCities.SelectedIndices.Count > 0)
            {
                m_btnGetTime.Enabled = true;
            }
            else
            {
                m_btnGetTime.Enabled = false;
            }
        }

        private void WorldTimeWebsiteValue_LinkClicked(
            object sender, LinkLabelLinkClickedEventArgs e)
        {
            m_lblWorldTimeWebsiteValue.LinkVisited = true;
            System.Diagnostics.Process.Start(
                "http://www.codeplex.com/worldtime/");
        }

        private void PersonalWebsiteValue_LinkClicked(
            object sender, LinkLabelLinkClickedEventArgs e)
        {
            m_lblPersonalWebsiteValue.LinkVisited = true;
            System.Diagnostics.Process.Start(
                "http://karthikabiraman.brinkster.net/");
        }   

        #region Notify Icon

        private void NotifyIcon_MouseDoubleClick(
            object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            m_bIsBalloonTipBeingShown = false;
        }

        /// <summary>
        /// Display a balloon tip with the time for the 1st five 'My Cities'.
        /// </summary>
        private void NotifyIcon_MouseMove(
            object sender, MouseEventArgs e)
        {
            if (!m_bIsBalloonTipBeingShown)
            {
                string strBalloonText = "";
                int intCityCount = 0;

                foreach(ListViewItem item in m_lvMyCities.Items)
                {
                    strBalloonText += item.SubItems[0].Text.Substring(
                        0, item.SubItems[0].Text.IndexOf(',')) + ": " +
                        item.SubItems[1].Text + "\n";

                    intCityCount += 1;

                    if (intCityCount == 5)
                    {
                        break;
                    }
                }

                if (intCityCount > 0)
                {
                    m_notifyIcon.ShowBalloonTip(
                        20, "WorldTime",
                        strBalloonText,
                        ToolTipIcon.Info);
                    m_bIsBalloonTipBeingShown = true;
                }
            }
        }

        private void NotifyIcon_BalloonTipClosed(
            object sender, System.EventArgs e)
        {
            m_bIsBalloonTipBeingShown = false;
        }

        private void ShowMenuItem_Click(object sender, System.EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            m_bIsBalloonTipBeingShown = false;
        }

        private void ExitMenuItem_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Settings

        private void ShowSystemTrayIcon_CheckedChanged(
            object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowSystemTrayIcon = 
                m_chkMinimzeToTray.Enabled = m_chkShowSystemTrayIcon.Checked;
            Properties.Settings.Default.Save();
        }

        private void MinimzeToTray_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinimizeToSystemTray =
                m_chkMinimzeToTray.Checked;
            Properties.Settings.Default.Save();
        }

        private void StandardTimeFormat_CheckedChanged(
            object sender, EventArgs e)
        {
            if (m_rbStandardTimeFormat.Checked)
            {
                Properties.Settings.Default.UseStandardTimeFormat = true;
                Properties.Settings.Default.Save();
            }
        }

        private void MilitaryTimeFormat_CheckedChanged(
            object sender, EventArgs e)
        {
            if (m_rbMilitaryTimeFormat.Checked)
            {
                Properties.Settings.Default.UseStandardTimeFormat = false;
                Properties.Settings.Default.Save();
            }
        }

        private void WorldTime_CheckedChanged(object sender, EventArgs e)
        {
            if (m_rbWorldTime.Checked)
            {
                Properties.Settings.Default.DateFormat = "WorldTime";
                Properties.Settings.Default.Save();
            }
        }

        private void SystemCultureShort_CheckedChanged(
            object sender, EventArgs e)
        {
            if (m_rbSystemCultureShort.Checked)
            {
                Properties.Settings.Default.DateFormat = "SystemCultureShort";
                Properties.Settings.Default.Save();
            }
        }

        private void SystemCultureLong_CheckedChanged(
            object sender, EventArgs e)
        {
            if (m_rbSystemCultureLong.Checked)
            {
                Properties.Settings.Default.DateFormat = "SystemCultureLong";
                Properties.Settings.Default.Save();
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a selected city.
    /// </summary>
    struct SelectedCity
    {
        /// <summary>
        /// The City ID of the selected city.
        /// </summary>
        public int CityID
        {
            get { return m_intCityID; }
            set { m_intCityID = value; }
        }
        private int m_intCityID;

        /// <summary>
        /// Whether the selected city ever has DST.
        /// </summary>
        public int HasDST
        {
            get { return m_intHasDST; }
            set { m_intHasDST = value; }
        }
        private int m_intHasDST;

        /// <summary>
        /// The Offset of the selected City.
        /// </summary>
        public string Offset
        {
            get { return m_strOffset; }
            set { m_strOffset = value; }
        }
        private string m_strOffset;        

        /// <summary>
        /// The DST Start of the selected city.
        /// </summary>
        public string DSTStart
        {
            get { return m_strDSTStart; }
            set { m_strDSTStart = value; }
        }
        private string m_strDSTStart;       

        /// <summary>
        /// The DST End of the selected city.
        /// </summary>
        public string DSTEnd
        {
            get { return m_strDSTEnd; }
            set { m_strDSTEnd = value; }
        }
        private string m_strDSTEnd;        
    }
}