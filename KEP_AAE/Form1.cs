using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KEP_AAE
{
    public partial class Form1 : Form
    {

        private readonly DateTime now = DateTime.Now;
        private String[] personInfo = new string[9];
        private int selectedRow = -1;
        static String connectionstring = "Data source=..\\kep_aae_local.db;Version=3";
        private SQLiteConnection conn = new SQLiteConnection(connectionstring);

        public Form1()
        {
            InitializeComponent();
            
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //init error and search labels
            this.label8.Text = " ";
            this.label9.Text = " ";
            this.label10.Text = " ";
            this.label11.Text = " ";
            this.label12.Text = " ";
            this.label13.Text = " ";
            //init button visibility
            this.button2.Visible = false;
            this.button3.Visible = false;
            this.button5.Visible= false;
            Read_Data(this.conn, dataGridView1, false);
        }

        private bool Fetched_Data()
        {
            bool check = this.field_Check();
            if (check)
            {
                this.personInfo[0] = maskedTextBox1.Text; //first
                this.personInfo[1] = maskedTextBox2.Text; //last
                this.personInfo[2] = maskedTextBox3.Text; //email name field
                this.personInfo[8] = textBox2.Text;
                string emailDomain;
                if (comboBox1.SelectedIndex != -1)
                {
                    object t = comboBox1.SelectedItem; //selected domain
                    emailDomain = Convert.ToString(t);
                    this.personInfo[3] = this.personInfo[2] + emailDomain; //so that we send a proper format to the database
                }
                else
                {
                    return false;
                }
                
                this.personInfo[4] = maskedTextBox4.Text; //phone number
                this.personInfo[5] = (maskedTextBox5.Text + " " + maskedTextBox7.Text); //home address
                this.label12.Text = " ";
                this.personInfo[6] = maskedTextBox6.Text + "/" + maskedTextBox8.Text + "/" + maskedTextBox9.Text; //date of birth
                this.personInfo[7] = Convert.ToString(now); //today :)
                
                return true;
            } else
            {
                return false;
            }
        }
        private void Insert_Data(SQLiteConnection connection, DataGridView grid)
        {
            bool done = this.Fetched_Data();
            if (done)
            {
                connection.Open();
                string insertQuery = "INSERT INTO myTable (firstName, lastName, emailAddress, phoneNumber, homeAddress, dateBirth, dateRequested, requestType) " +
                    "VALUES (@firstName, @lastName, @emailAddress, @phoneNumber, @homeAddress, @dateBirth, @dateRequested, @requestType)";
                

                using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, connection))
                {
                    cmd.Parameters.Add("@firstName", DbType.String).Value = this.personInfo[0];
                    cmd.Parameters.Add("@lastName", DbType.String).Value = this.personInfo[1];
                    cmd.Parameters.Add("@emailAddress", DbType.String).Value = this.personInfo[3];
                    cmd.Parameters.Add("@phoneNumber", DbType.String).Value = this.personInfo[4];
                    cmd.Parameters.Add("@homeAddress", DbType.String).Value = this.personInfo[5];
                    cmd.Parameters.Add("@dateBirth", DbType.String).Value = this.personInfo[6];
                    cmd.Parameters.Add("@dateRequested", DbType.String).Value = this.personInfo[7];
                    cmd.Parameters.Add("@requestType", DbType.String).Value = this.personInfo[8];
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                }

                connection.Close();

                //refresh grid
                if (textBox1.Text.Trim().Length == 0)
                {
                    Read_Data(connection, grid, false);
                }
                else
                {
                    Read_Data(connection, grid, true);
                }

            }
        }


        private bool field_Check()
        {
            //check all masks are complete/fullfilled , if not show error labels
            bool[] signal = new bool[10];
            //signal[9] is the general signal to decide what bool value the function will return
            //spaghetti code warning
            //init all bools
            for (int i = 0; i < signal.Length; i++) { signal[i] = true; }
            bool homeDummy = this.maskedTextBox5.MaskCompleted & this.maskedTextBox7.MaskCompleted;
            bool dateDummy = this.maskedTextBox6.MaskCompleted & this.maskedTextBox8.MaskCompleted & this.maskedTextBox9.MaskCompleted;
            if (!this.maskedTextBox1.MaskCompleted)
            {
                this.label8.Text = "Εισάγετε ένα όνομα που πλήρει τις απαιτήσεις";
                signal[0] = false;
                signal[9] = false;
            }
            if (signal[0]) { this.label8.Text = " "; }
            if (!this.maskedTextBox2.MaskCompleted)
            {
                this.label9.Text = "Εισάγετε ένα επώνυμο που πλήρει τις απαιτήσεις";
                signal[1] = false;
                signal[9] = false;
            }
            if (signal[1]) { this.label9.Text = " "; }
            if (!this.maskedTextBox3.MaskCompleted)
            {
                this.label10.Text = "Εισάγετε ένα e-mail που πλήρει τις απαιτήσεις";
                signal[2] = false;
                signal[9] = false;
            }
            if (comboBox1.SelectedIndex == -1)
            {
                this.label10.Text = "Εισάγετε ένα e-mail που πλήρει τις απαιτήσεις";
                signal[3] = false;
                signal[9] = false;
            }
            if (signal[2] & signal[3]) { this.label10.Text = " "; }
            if (!this.maskedTextBox4.MaskCompleted)
            {
                this.label11.Text = "Εισάγετε έναν αριθμό τηλ. που πλήρει τις απαιτήσεις";
                signal[4] = false;
                signal[9] = false;
            }
            if (signal[4]) { this.label11.Text = " "; }
            if (!dateDummy)
            {
                this.label12.Text = "Εισάγετε μια ημερομηνία με τον τρόπο που αναφέρεται πάνω";
                signal[5] = false;
                signal[9] = false;
            }
            if (signal[5]) { this.label12.Text = " "; }
            string day = maskedTextBox6.Text;
            string mo = maskedTextBox8.Text;
            string year = maskedTextBox9.Text;
            string input = day + "/" + mo + "/" + year;
            DateTime inputDate;
            CultureInfo politismos = new CultureInfo("el-GR");
            bool properDate = DateTime.TryParseExact(input, "d", politismos, DateTimeStyles.None, out inputDate);
            if (!properDate)
            {
                this.label12.Text = "Έχετε βάλει μη-ορθή ημερομηνία, δοκιμάστε ξανά";
                signal[6] = false;
                signal[9] = false;
            }
            if (signal[6]) { this.label12.Text = " "; }
            if (!homeDummy)
            {
                this.label13.Text = "Εισάγετε μια διεύθυνση κατοικίας με τον τρόπο που αναφέρεται πάνω";
                signal[7] = false;
                signal[9] = false;
            }
            if (signal[7]) { this.label13.Text = " "; }
            if (textBox2.Text.Trim().Length == 0)
            {
                this.label21.Text = "Σιγουρευτείτε πώς έχετε γράψει κάποιο αίτημα";
                signal[8] = false;
                signal[9] = false;
            }
            if (signal[8]) { this.label21.Text = " ";  } 
            if (signal[9]) { return true; }
            return false;
        }

        private void Read_Data(SQLiteConnection connection, DataGridView grid, bool search)
        {
            connection.Open();
            string query;
            if (search)
            {
                query = "SELECT * FROM myTable WHERE firstName LIKE @search OR lastName LIKE @search ORDER BY id DESC";

            } 
            else
            {
                query = "SELECT * FROM myTable ORDER BY id DESC";
            }

            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
            {

                if (search) { cmd.Parameters.Add("@search", DbType.String).Value = "%" + (this.textBox1.Text) + "%"; }
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }

            connection.Close();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            //update button
            bool check = this.field_Check();
            if (!check) { return; }
            string caption = "Επιβεβαίωση Ενημέρωσης Αιτήματος";
            string message = "Θέλετε σίγουρα να ενημερώσετε αυτό το αίτημα; Τα προηγούμενα δεδομένα θα χαθούν!";
            MessageBoxButtons koympia = MessageBoxButtons.OKCancel;
            MessageBoxIcon iconidio = MessageBoxIcon.Warning;
            DialogResult result = MessageBox.Show(message, caption, koympia, iconidio);
            if (result == DialogResult.OK)
            {
                Update_Data(this.conn, dataGridView1);
            }
        }

        private void Update_Data(SQLiteConnection connection, DataGridView grid)
        {
            this.selectedRow = grid.CurrentCell.RowIndex;
            if ( selectedRow== -1 ) { return; }
            int id = Convert.ToInt32(grid.Rows[this.selectedRow].Cells[8].Value);
            bool done = this.Fetched_Data();
            if (done) 
            {
                connection.Open();
                string updateQuery = "UPDATE myTable SET firstName=@firstName, lastName=@lastName, emailAddress=@emailAddress, phoneNumber=@phoneNumber, homeAddress=@homeAddress, dateBirth=@dateBirth, dateRequested=@dateRequested, requestType=@requestType WHERE id=@rowIndex";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, connection))
                {
                    cmd.Parameters.Add("@firstName", DbType.String).Value = this.personInfo[0];
                    cmd.Parameters.Add("@lastName", DbType.String).Value = this.personInfo[1];
                    cmd.Parameters.Add("@emailAddress", DbType.String).Value = this.personInfo[3];
                    cmd.Parameters.Add("@phoneNumber", DbType.String).Value = this.personInfo[4];
                    cmd.Parameters.Add("@homeAddress", DbType.String).Value = this.personInfo[5];
                    cmd.Parameters.Add("@dateBirth", DbType.String).Value = this.personInfo[6];
                    cmd.Parameters.Add("@dateRequested", DbType.String).Value = this.personInfo[7];
                    cmd.Parameters.Add("@requestType", DbType.String).Value = this.personInfo[8];
                    cmd.Parameters.Add("@rowIndex", DbType.Int32).Value = id;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                }

                connection.Close();

                //refresh grid
                if (textBox1.Text.Trim().Length == 0)
                {
                    Read_Data(connection, grid, false);
                } 
                else
                {
                    Read_Data(connection, grid, true);
                }
                    
            }
            
        }
        
        private void Import_Data(DataGridView grid)
        {
            string emailTemp = " ";
            maskedTextBox1.Text = Convert.ToString(dataGridView1.Rows[this.selectedRow].Cells[0].Value); //first name
            maskedTextBox2.Text = Convert.ToString(dataGridView1.Rows[this.selectedRow].Cells[1].Value); //last name
            emailTemp = Convert.ToString(dataGridView1.Rows[this.selectedRow].Cells[2].Value);
            int papakiIndex = emailTemp.LastIndexOf("@");
            if (!(papakiIndex == -1 )) 
            {
                maskedTextBox3.Text = emailTemp.Substring(0, papakiIndex);
                char dom = emailTemp[papakiIndex+ 1];
                switch (dom)
                {
                    case 'g':
                        comboBox1.SelectedIndex = 0;
                        break;

                    case 'h':
                        comboBox1.SelectedIndex = 1;
                        break;

                    case 'y':
                        comboBox1.SelectedIndex = 2;
                        break;

                    case 'a':
                        comboBox1.SelectedIndex = 3;
                        break;
                }
            }
            
            maskedTextBox4.Text = Convert.ToString(dataGridView1.Rows[this.selectedRow].Cells[3].Value);
            //home address fetch block
            string homeTemp = Convert.ToString(dataGridView1.Rows[this.selectedRow].Cells[4].Value);
            int splitIndex = homeTemp.LastIndexOf(' ');
            maskedTextBox5.Text = homeTemp.Substring(0, splitIndex + 1); // home address name 
            maskedTextBox7.Text = homeTemp.Substring(splitIndex+ 1); // home number
            //date of birth fetch block
            string dateTemp = Convert.ToString(dataGridView1.Rows[this.selectedRow].Cells[5].Value);
            string[] dateSplit = dateTemp.Split('/');
            maskedTextBox6.Text = dateSplit[0]; //day
            maskedTextBox8.Text = dateSplit[1]; //month
            maskedTextBox9.Text = dateSplit[2]; //year
        }
        private void Delete_Data(SQLiteConnection connection, DataGridView grid) 
        {
            connection.Open();
            string deleteQuery = "DELETE FROM mytable WHERE id=@rowIndex";
            int id = Convert.ToInt32(grid.Rows[this.selectedRow].Cells[8].Value);
            using (SQLiteCommand cmd = new SQLiteCommand(deleteQuery, connection))
            {
                cmd.Parameters.Add("@rowIndex", DbType.Int32).Value = id;
                cmd.ExecuteNonQuery();
            }

            connection.Close();
            this.selectedRow = -1;
            if (textBox1.Text.Trim().Length == 0)
            {
                Read_Data(connection, grid, false);
            }
            else
            {
                Read_Data(connection, grid, true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Insert_Data(this.conn, dataGridView1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string caption = "Επιβεβαίωση Διαγραφής Αιτήματος";
            string message = "Θέλετε σίγουρα να διαγράψετε αυτό το αίτημα; Τα δεδομένα θα χαθούν!";
            MessageBoxButtons koympia = MessageBoxButtons.OKCancel;
            MessageBoxIcon iconidio = MessageBoxIcon.Warning;
            DialogResult result = MessageBox.Show(message, caption, koympia, iconidio);
            if (result == DialogResult.OK)
            {
                Delete_Data(this.conn, dataGridView1);
            }
        }

        //when i click the on the mtbx cursor goes to the leftmost empty character

        private void maskedTextBox1_Click(object sender, EventArgs e)
        {
            maskedTextBox1.SelectionStart = maskedTextBox1.Text.Length;
        }

        private void maskedTextBox2_Click(object sender, EventArgs e)
        {
            maskedTextBox2.SelectionStart = maskedTextBox2.Text.Length;
        }

        private void maskedTextBox3_Click(object sender, EventArgs e)
        {
            maskedTextBox3.SelectionStart = maskedTextBox3.Text.Length;
        }

        private void maskedTextBox4_Click(object sender, EventArgs e)
        {
            maskedTextBox4.SelectionStart = maskedTextBox4.Text.Length;
        }

        private void maskedTextBox5_Click(object sender, EventArgs e)
        {
            maskedTextBox5.SelectionStart = maskedTextBox5.Text.Length;
        }

        private void maskedTextBox6_Click(object sender, EventArgs e)
        {
            maskedTextBox6.SelectionStart = maskedTextBox6.Text.Length;
        }

        private void maskedTextBox7_Click(object sender, EventArgs e)
        {
            maskedTextBox7.SelectionStart = maskedTextBox7.Text.Length;
        }

        private void maskedTextBox8_Click(object sender, EventArgs e)
        {
            maskedTextBox8.SelectionStart = maskedTextBox8.Text.Length;
        }

        private void maskedTextBox9_Click(object sender, EventArgs e)
        {
            maskedTextBox9.SelectionStart = maskedTextBox9.Text.Length;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 0)
            {
                Read_Data(conn, dataGridView1, false);
            }
            else
            {
                Read_Data(conn,dataGridView1, true);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;
            button2.Visible = false;
            button3.Visible = false;
            button5.Visible = false;
            if (!(selectedRow == -1))
            {
                button2.Visible = true;
                button3.Visible = true;
                button5.Visible = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string caption = "Επιβεβαίωση Αντιγραφής Δεδομένων";
            string message = "Θέλετε σίγουρα να αντιγράψετε τα δεδομένα στα πεδία; Τα προηγούμενα δεδομένα θα χαθούν!";
            MessageBoxButtons koympia = MessageBoxButtons.OKCancel;
            MessageBoxIcon iconidio = MessageBoxIcon.Warning;
            DialogResult result = MessageBox.Show(message, caption, koympia, iconidio);
            if (result == DialogResult.OK)
            {
                Import_Data(dataGridView1);
            }
        }
    }
}