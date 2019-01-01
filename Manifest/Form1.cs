﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;
using System.Collections.ObjectModel;
using System.IO;

namespace Manifest
{
    public partial class Form1 : Form
    {
        public ImageList Imagelist = new ImageList();
        int searchIndex;

        public Form1()
        {
            InitializeComponent();
            searchIndex = 0;

            // Key event handler for left/right keys
            // Requested by Mike for scrolling left/right through loads
            this.KeyUp += new KeyEventHandler(this.Form1_KeyUp);

            textBoxSearchPeople.KeyDown += new KeyEventHandler(searchPeople_KeyDown);

            // show/hide UI components
            hideEditPersonUI();
            buttonSavePerson.Hide();
            buttonAddPerson.Hide();
            buttonCancel.Hide();
            labelEditDetails.Hide();

            hideEditAircraftUI();
            buttonAddAircraftSubmit.Hide();
            buttonSaveAircraft.Hide();
            buttonCancelAircraft.Hide();

            WindowState = FormWindowState.Maximized;
            tabControl.SelectedTab = tabPageLoads;
            comboBoxLoadAircraft.SelectedIndex = 0;
            numericUpDownMaxJumpers.Value = 1;
            loadPeople();
            loadAircraft();

            // Retrieve all image files for logos used to group tandems/AFF
            String[] ImageFiles = Directory.GetFiles(@"C:\test");
            foreach (var file in ImageFiles)
            {
                //Add images to Imagelist
                Imagelist.Images.Add(Image.FromFile(file));
            }
        }

        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                Point current = panelLoads.AutoScrollPosition;
                Point scrolled = new Point(current.X + 50, current.Y);
                panelLoads.AutoScrollPosition = scrolled;
            }
            if (e.KeyCode == Keys.Left)
            {
                Point current = panelLoads.AutoScrollPosition;
                Point scrolled = new Point(current.X - 50, current.Y);
                panelLoads.AutoScrollPosition = scrolled;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("West Tennessee Skydiving - Manifest software\nCopyright 2018\nAll rights reserved", "About");
        }

        public void searchPeople_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchForPeople();
            }
        }

        public void searchForPeople()
        {
            String searchText = textBoxSearchPeople.Text.ToLower();
            Boolean found = false;
            int selectedIndex = listBoxPeople.SelectedIndex;
            int numItems = listBoxPeople.Items.Count;

            // If the first item in the list is selected and matches, just increment search index and return
            if (selectedIndex == 0 && searchIndex == 0 && listBoxPeople.Items[0].ToString().ToLower().Contains(searchText))
            {
                searchIndex++;
                return;
            }


            if (selectedIndex + 1 == numItems)
            {
                searchIndex = 0;
                selectedIndex = 0;
                listBoxPeople.SelectedIndex = 0;
            }    
            else
                searchIndex = selectedIndex + 1;

            // Starting at the search index, look for the next instance of the search text
            for (int i = searchIndex; i < listBoxPeople.Items.Count; i++)
            {
                if (listBoxPeople.Items[i].ToString().ToLower().Contains(searchText))
                {
                    found = true;
                    listBoxPeople.SelectedIndex = i;
                    searchIndex = i;
                    break;
                }
            }

            if (!found)
            {
                MessageBox.Show("Reached the end of the list.");
                searchIndex = 0;
                listBoxPeople.SelectedIndex = 0;
            }
        }

        public void loadPeople()
        {
            ObservableCollection<String> people = new ObservableCollection<String>();
            List<PersonType> peopleFromDB = new List<PersonType>();
            string connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
            using (var conn = new SqlConnection(connString))
            {
                string sqlString = @"select manifestNumber, firstName, lastName, paid from people";
                using (var command = new SqlCommand(sqlString, conn))
                {
                    conn.Open();
                    var result = command.ExecuteScalar();
                    System.Diagnostics.Debug.WriteLine(result.ToString());
                    conn.Close();
                }
            }

            String m, f, l;
            double p;

            using (SqlConnection cn = new SqlConnection(connString))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandText = "select manifestNumber, firstName, lastName, paid from people";
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        m = dr.GetString(0);
                        f = dr.GetString(1);
                        l = dr.GetString(2);
                        if (dr["paid"] != DBNull.Value)
                            Double.TryParse(dr.GetString(3), out p);
                        else
                            p = 0;
                        PersonType per = new PersonType(m, f, l, p);
                        peopleFromDB.Add(per);
                    }
                }
            }
            peopleFromDB.Sort();

            foreach (PersonType pt in peopleFromDB)
            {
                people.Add(pt.getManifestNumber() + " - " + pt.getFirstName() + " " + pt.getLastName());
            }
            listBoxPeople.DataSource = people;
        }

        private void showEditPersonUI()
        {
            labelManifestNumber.Show();
            textBoxManifestNumber.Show();
            labelFirstName.Show();
            textBoxFirstName.Show();
            labelLastName.Show();
            textBoxLastName.Show();
            checkBoxTI.Show();
            checkBoxAFF.Show();
            checkBoxCoach.Show();
            checkBoxVideo.Show();
        }

        private void hideEditPersonUI()
        {
            labelManifestNumber.Hide();
            textBoxManifestNumber.Text = "";
            textBoxManifestNumber.Hide();
            labelFirstName.Hide();
            textBoxFirstName.Text = "";
            textBoxFirstName.Hide();
            labelLastName.Hide();
            textBoxLastName.Text = "";
            textBoxLastName.Hide();
            checkBoxTI.Checked = false;
            checkBoxTI.Hide();
            checkBoxAFF.Checked = false;
            checkBoxAFF.Hide();
            checkBoxCoach.Checked = false;
            checkBoxCoach.Hide();
            checkBoxVideo.Checked = false;
            checkBoxVideo.Hide();
        }

        private void buttonAddNewPerson_Click(object sender, EventArgs e)
        {
            // Display the edit UI components
            showEditPersonUI();
            buttonAddPerson.Show();
            buttonCancel.Show();
            buttonSavePerson.Hide();

            // Clear the edit UI components
            textBoxManifestNumber.Text = "";
            textBoxFirstName.Text = "";
            textBoxLastName.Text = "";
        }

        private void buttonAddTandem_Click(object sender, EventArgs e)
        {
            Form addTandemWindow = new FormAddPersonToLoad();
            addTandemWindow.ShowDialog();
        }

        private void buttonNewLoad_Click(object sender, EventArgs e)
        {
            ListView loadList = new ListView();

            loadList.LargeImageList = Imagelist;
            loadList.SmallImageList = Imagelist;

            loadList.View = View.Details;

            loadList.HeaderStyle = ColumnHeaderStyle.None;
            loadList.FullRowSelect = true;
            loadList.Columns.Add("", -2);
            String aircraft = comboBoxLoadAircraft.Text;
            loadList.Items.Add(aircraft);
            loadList.View = View.Details; // Enables Details view so you can see columns
            loadList.Items.Add(new ListViewItem { ImageIndex = 0, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 0, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 1, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 1, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 2, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 2, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 3, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 3, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 4, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 4, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 5, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 5, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 6, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 6, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 7, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 7, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 8, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 8, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 9, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 9, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 10, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 10, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 11, Text = "5368 - Jamie Minyard AFF1" });
            loadList.Items.Add(new ListViewItem { ImageIndex = 11, Text = "5368 - Jamie Minyard AFF1" });

            loadList.Width = 200;
            loadList.Height = 500;
            loadList.Columns[0].Width = Width - 50;

            panelLoads.Controls.Add(loadList);
        }

        private void buttonDeletePerson_Click(object sender, EventArgs e)
        {
            try
            {
                String item = listBoxPeople.GetItemText(listBoxPeople.SelectedItem);
                String[] splitString = item.Split('-');
                String manNum = splitString[0];
                String name = splitString[1];
                DialogResult dialogResult = MessageBox.Show("ARE YOU SURE you want to delete this person from the database?\n\nManifest Number: " + manNum + "\nName: " + name + "\n\n***THIS ACTION CANNOT BE UNDONE***", "Confirm delete", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    String connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
                    using (SqlConnection cn = new SqlConnection(connString))
                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = "delete from People where manifestNumber = '" + manNum + "'";
                        cn.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            // If delete was successful, reload the people in the UI list
                            loadPeople();

                            // Hide the edit UI components
                            hideEditPersonUI();
                            buttonSavePerson.Hide();
                        }
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Unable to delete person.");
            }
        }

        private void buttonEditPerson_Click(object sender, EventArgs e)
        {
            // Display the edit UI components
            showEditPersonUI();
            buttonSavePerson.Show();
            buttonCancel.Show();
            buttonAddPerson.Hide();
            try
            {
                String item = listBoxPeople.GetItemText(listBoxPeople.SelectedItem);
                String[] splitString = item.Split('-');
                String manNum = splitString[0].Trim();
                String name = splitString[1].Trim();
                String firstName = name.Split(' ')[0];
                String lastName = name.Split(' ')[1];
                textBoxManifestNumber.Text = manNum;
                textBoxFirstName.Text = firstName;
                textBoxLastName.Text = lastName;

                // Get their checkbox statuses from the database
                Boolean t = false;
                Boolean a = false;
                Boolean c = false;
                Boolean v = false;
                String connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
                using (SqlConnection cn = new SqlConnection(connString))
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "select TI, AFFI, coach, videographer from people where manifestNumber = '" + manNum + "'";
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            t = dr.IsDBNull(0) ? false : dr.GetBoolean(0);
                            a = dr.IsDBNull(1) ? false : dr.GetBoolean(1);
                            c = dr.IsDBNull(2) ? false : dr.GetBoolean(2);
                            v = dr.IsDBNull(3) ? false : dr.GetBoolean(3);
                        }
                    }
                }

                checkBoxTI.Checked = t;
                checkBoxAFF.Checked = a;
                checkBoxCoach.Checked = c;
                checkBoxVideo.Checked = v;

                labelEditDetails.Text = "Editing details for " + manNum.ToString() + " - " + firstName + " " + lastName;
                labelEditDetails.Show();
            }
            catch (Exception x)
            {
                MessageBox.Show("Unable to edit person.\n\nError: " + x.ToString());
            }
        }

        private void buttonSavePerson_Click(object sender, EventArgs e)
        {
            String manNum = textBoxManifestNumber.Text;
            manNum = manNum.Replace("'", "");
            String fName = textBoxFirstName.Text;
            fName = fName.Replace("'", "");
            String lName = textBoxLastName.Text;
            lName = lName.Replace("'", "");
            Boolean ti = checkBoxTI.Checked;
            Boolean affi = checkBoxAFF.Checked;
            Boolean coach = checkBoxCoach.Checked;
            Boolean video = checkBoxVideo.Checked;

            String t = "0";
            if (ti)
                t = "1";
            String a = "0";
            if (affi)
                a = "1";
            String c = "0";
            if (coach)
                c = "1";
            String v = "0";
            if (video)
                v = "1";

            String connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
            using (SqlConnection cn = new SqlConnection(connString))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandText = "update People set firstName = @param2, lastName = @param3, TI = " + t + ", AFFI = " + a + ", coach = " + c + ", videographer = " + v + " where manifestNumber = @param1";

                cmd.Parameters.Add("@param1", SqlDbType.VarChar, 8).Value = manNum;
                cmd.Parameters.Add("@param2", SqlDbType.NVarChar, 50).Value = fName;
                cmd.Parameters.Add("@param3", SqlDbType.NVarChar, 50).Value = lName;

                cn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    // If insert was successful, reload the people in the UI list
                    loadPeople();

                    // Hide the edit UI components
                    hideEditPersonUI();
                    buttonSavePerson.Hide();
                    buttonCancel.Hide();
                    labelEditDetails.Hide();
                }
            }
        }

        private void buttonAddPerson_Click(object sender, EventArgs e)
        {
            String manNum = textBoxManifestNumber.Text;
            manNum = manNum.Replace("'","");
            String fName = textBoxFirstName.Text;
            fName = fName.Replace("'", "");
            String lName = textBoxLastName.Text;
            lName = lName.Replace("'", "");
            Boolean ti = checkBoxTI.Checked;
            Boolean affi = checkBoxAFF.Checked;
            Boolean coach = checkBoxCoach.Checked;
            Boolean video = checkBoxVideo.Checked;

            String t = "0";
            if (ti)
                t = "1";
            String a = "0";
            if (affi)
                a = "1";
            String c = "0";
            if (coach)
                c = "1";
            String v = "0";
            if (video)
                v = "1";



            String connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
            using (SqlConnection cn = new SqlConnection(connString))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandText = "insert into People(manifestNumber, firstName, lastName, paid, TI, AFFI, coach, videographer)" +
                    "values(@param1, @param2, @param3,'" + 0 + "'," + t + "," + a + "," + c + "," + v + ")";

                cmd.Parameters.Add("@param1", SqlDbType.VarChar, 8).Value = manNum;
                cmd.Parameters.Add("@param2", SqlDbType.NVarChar, 50).Value = fName;
                cmd.Parameters.Add("@param3", SqlDbType.NVarChar, 50).Value = lName;

                cn.Open();

                if (cmd.ExecuteNonQuery() == 1)
                {
                    // If insert was successful, reload the people in the UI list
                    loadPeople();

                    // Hide the edit UI components
                    hideEditPersonUI();
                    buttonCancel.Hide();
                    buttonAddPerson.Hide();
                }
            }
        }

        private void buttonSearchPeople_Click(object sender, EventArgs e)
        {
            searchForPeople();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            hideEditPersonUI();
            buttonSavePerson.Hide();
            buttonAddPerson.Hide();
            buttonCancel.Hide();
            labelEditDetails.Hide();
        }

        private void showEditAircraftUI()
        {
            labelEditDetailsAircraft.Show();
            labelAircraftName.Show();
            textBoxAircraftName.Show();
            labelMaxJumpers.Show();
            numericUpDownMaxJumpers.Show();
        }

        private void hideEditAircraftUI()
        {
            labelEditDetailsAircraft.Hide();
            labelAircraftName.Hide();
            textBoxAircraftName.Text = "";
            textBoxAircraftName.Hide();
            labelMaxJumpers.Hide();
            numericUpDownMaxJumpers.Value = 0;
            numericUpDownMaxJumpers.Hide();
        }

        private void buttonCancelAircraft_Click(object sender, EventArgs e)
        {
            hideEditAircraftUI();
            buttonAddAircraftSubmit.Hide();
            buttonSaveAircraft.Hide();
            buttonCancelAircraft.Hide();
        }

        private void buttonAddAircraftSubmit_Click(object sender, EventArgs e)
        {
            String name = textBoxAircraftName.Text;
            int cap = (int)numericUpDownMaxJumpers.Value;

            if (String.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a name for this aircraft.");
                return;
            }

            if (cap < 1)
            {
                MessageBox.Show("Max jumpers cannot be less than 1.");
                return;
            }

            String connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
            using (SqlConnection cn = new SqlConnection(connString))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandText = "insert into Aircraft(aircraftName, capacity)" +
                    "values(@param1, @param2)";

                cmd.Parameters.Add("@param1", SqlDbType.NVarChar, 50).Value = name;
                cmd.Parameters.Add("@param2", SqlDbType.Int).Value = cap;

                cn.Open();

                if (cmd.ExecuteNonQuery() == 1)
                {
                    // If insert was successful, reload the people in the UI list
                    loadAircraft();

                    // Hide the edit UI components
                    hideEditAircraftUI();
                    buttonAddAircraftSubmit.Hide();
                    buttonSaveAircraft.Hide();
                    buttonCancelAircraft.Hide();
                }
            }
        }

        public void loadAircraft()
        {
            ObservableCollection<String> aircraft = new ObservableCollection<String>();
            List<AircraftType> aircraftFromDB = new List<AircraftType>();
            string connString = @"Data Source=(localdb)\MSSQLLocalDB; AttachDbFilename=C:\Users\jamie\source\repos\Manifest\Manifest\WTSDatabase.mdf; Integrated Security=True;";
            using (var conn = new SqlConnection(connString))
            {
                string sqlString = @"select aircraftName, capacity from Aircraft";
                using (var command = new SqlCommand(sqlString, conn))
                {
                    conn.Open();
                    var result = command.ExecuteScalar();
                    System.Diagnostics.Debug.WriteLine(result.ToString());
                    conn.Close();
                }
            }

            String an;
            int c;

            using (SqlConnection cn = new SqlConnection(connString))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandText = "select aircraftName, capacity from Aircraft";
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        an = dr.GetString(0);
                        c = dr.GetInt32(1);

                        AircraftType air = new AircraftType(an, c);
                        aircraftFromDB.Add(air);
                    }
                }
            }

            foreach (AircraftType plane in aircraftFromDB)
            {
                aircraft.Add(plane.getName() + " - Max jumpers " + plane.getCapacity());
            }
            listBoxAircraft.DataSource = aircraft;
        }

        private void buttonSaveAircraft_Click(object sender, EventArgs e)
        {
            String name = textBoxAircraftName.Text;
            int cap = (int)numericUpDownMaxJumpers.Value;

            if (String.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a name for this aircraft.");
                return;
            }

            if (cap < 1)
            {
                MessageBox.Show("Max jumpers cannot be less than 1.");
                return;
            }

            hideEditAircraftUI();
            buttonAddAircraftSubmit.Hide();
            buttonSaveAircraft.Hide();
            buttonCancelAircraft.Hide();
        }

        private void buttonAddAircraft_Click(object sender, EventArgs e)
        {
            showEditAircraftUI();
            buttonAddAircraftSubmit.Show();
            buttonSaveAircraft.Hide();
            buttonCancelAircraft.Show();
            labelEditDetailsAircraft.Hide();
        }

        private void buttonEditAircraft_Click(object sender, EventArgs e)
        {
            String item = listBoxAircraft.GetItemText(listBoxAircraft.SelectedItem);
            String[] splitString = item.Split(new string[] { " - Max jumpers " }, StringSplitOptions.None);
            String name = splitString[0].Trim();
            String cap = splitString[1].Trim();
            int capacity = 0;
            textBoxAircraftName.Text = name;
            Int32.TryParse(cap, out capacity);
            numericUpDownMaxJumpers.Value = capacity;

            labelEditDetailsAircraft.Text = "Editing details for " + name + " with max jumpers " + capacity + ".";

            showEditAircraftUI();
            buttonAddAircraftSubmit.Hide();
            buttonSaveAircraft.Show();
            buttonCancelAircraft.Show();
        }

        private void buttonDeleteAircraft_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("ARE YOU SURE you want to delete this aircrafit?\n\n***THIS ACTION CANNOT BE UNDONE***", "Confirm delete", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                MessageBox.Show("Delete");
            }
        }
    }
}
