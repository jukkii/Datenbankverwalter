using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using Mysqlx.Crud;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using MySqlX.XDevAPI.Relational;

namespace Datenbankverwalter
{
    public partial class Form1 : Form
    {
        public string errorMsg = "no Error";
        public string serverText = "";
        public string userText = "";
        public string passwordText = "";
        public string databaseText = "";
        public string createTableString = "";

        ToolStripMenuItem newTableToolStrip = new ToolStripMenuItem();
        private bool createNewTableModeEnabled = false;

        public bool isConnected = false;

        private string lastUsedDatabase = "";

        DataGridView gridView = null;
        private bool dataGridCreated = false;

        public MySqlConnection connection = null;

        public static event EventHandler? ApplicationExit;

        public Form1()
        {
            System.Windows.Forms.Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            RefreshErrorMsg();

        }

        private void AddContextMenu(object sender)
        {
            Interaction.MsgBox(e.);
            //newTableToolStrip.Text = "Add Attribute";
            //ContextMenuStrip strip = new ContextMenuStrip();
            //foreach (DataGridViewColumn column in gridView.Columns)
            //{

            //    column.ContextMenuStrip = strip;
            //    column.ContextMenuStrip.Items.Add(newTableToolStrip);
            //}
        }

        private DataGridViewCellEventArgs mouseLocation;

        private void newTableToolStrip_Click(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            Interaction.MsgBox("cell " + sender + " clicked " + e.ColumnIndex + ", " + e.RowIndex);
        }

        private void gridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridViewCell clickedCell = (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex];
                var relativeMousePosition = gridView.PointToClient(Cursor.Position);
                ContextMenuStrip strip = new ContextMenuStrip();
                strip.Show(gridView, relativeMousePosition);
            }
        }

        private void OnApplicationExit(object? sender, EventArgs e)
        {
            try
            {
                FileStream fs = new("saveFile.txt", FileMode.Create);
                StreamWriter sw = new(fs);
                sw.WriteLine($"{serverText};{userText};{passwordText};{databaseText}");
                sw.Close();
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                RefreshErrorMsg();
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                Form2 formConnectSQL = new Form2();
                formConnectSQL.ShowDialog();
            }
            else
            {
                if (connection != null)
                {
                    connection.Close();
                    TrvDatabaseView.Nodes.Clear();
                }
                isConnected = false;
                BtnConnect.Text = "Connect";
            }
        }

        public void RefreshErrorMsg()
        {
            LblErrorMessage.Text = errorMsg;
        }

        public void connectToDatabase()
        {
            try
            {
                FileStream fs = new("saveFile.txt", FileMode.Create);
                StreamWriter sw = new(fs);
                sw.Write($"{serverText};{userText};{passwordText};{databaseText}");
                sw.Close();
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                RefreshErrorMsg();
            }
            label1.Text = isConnected.ToString();
            string connectString = $"server={serverText};uid={userText};pwd={passwordText};database={databaseText};";
            errorMsg = connectString;

            try
            {
                connection = new MySqlConnection(connectString);
                connection.Open();
                errorMsg = "Connected";
                LblConnectedTo.Text = $"Connected to:\n{serverText} as {userText}";
                createServerTree();
                RefreshErrorMsg();
            }
            catch (Exception exc)
            {
                errorMsg += exc.ToString();
                RefreshErrorMsg();
            }
            label1.Text = isConnected.ToString();
        }

        public void createServerTree()
        {
            TrvDatabaseView.Nodes.Clear();
            string query = "SHOW DATABASES;";
            int nodeCounter = 0;
            int nodeCounter1 = 0;

            MySqlDataAdapter da = new(query, connection);
            DataSet ds = new();
            da.Fill(ds, "databases");
            DataTable dt = ds.Tables["databases"];

            RefreshErrorMsg();

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    Object test = row[col];
                    if (test.ToString() == "mysql") break;
                    if (test.ToString() == "information_schema") break;
                    if (test.ToString() == "performance_schema") break;
                    if (test.ToString() == "phpmyadmin") break;
                    if (test.ToString() == "es_extended") break;
                    if (test.ToString() == "essentialmode") break;

                    string query1 = $"USE {test};";
                    MySqlCommand command = new(query1, connection);
                    command.ExecuteNonQuery();

                    TrvDatabaseView.Nodes.Add(test.ToString());

                    string query2 = $"SHOW TABLES;";
                    MySqlDataAdapter da2 = new(query2, connection);
                    DataSet ds2 = new();
                    da2.Fill(ds2, test.ToString());
                    DataTable dt2 = ds2.Tables[test.ToString()];

                    foreach (DataRow row1 in dt2.Rows)
                    {
                        foreach (DataColumn col1 in dt2.Columns)
                        {
                            TrvDatabaseView.Nodes[nodeCounter].Nodes.Add(row1[col1].ToString());

                            string query3 = $"SELECT * FROM {row1[col1]};";
                            MySqlDataAdapter da3 = new(query3, connection);
                            DataSet ds3 = new();
                            da3.Fill(ds3, row1[col1].ToString());
                            DataTable dt3 = ds3.Tables[row1[col1].ToString()];

                            foreach (DataRow row2 in dt3.Rows)
                            {
                                foreach (DataColumn col2 in dt3.Columns)
                                {
                                    TrvDatabaseView.Nodes[nodeCounter].Nodes[nodeCounter1].Nodes.Add(col2.ToString());
                                }
                                break;
                            }
                            nodeCounter1++;
                        }
                    }
                    nodeCounter++;
                    nodeCounter1 = 0;
                }
            }
        }

        private void BtnCreateDatabase_Click(object sender, EventArgs e)
        {
            string databaseNameInput = Interaction.InputBox("Name of Database:", "Create Database", "Database_1");
            if (databaseNameInput == "") return;
            try
            {
                string query1 = $"CREATE DATABASE {databaseNameInput};";
                MySqlCommand command = new(query1, connection);
                command.ExecuteNonQuery();
                createServerTree();
            }
            catch (Exception exc)
            {
                errorMsg = databaseNameInput + "\n";
                errorMsg += exc.ToString();
                RefreshErrorMsg();
            }
        }

        private void BtnDeleteDatabase_Click(object sender, EventArgs e)
        {
            if (TrvDatabaseView.SelectedNode.Text == TrvDatabaseView.SelectedNode.FullPath)
            {
                DialogResult dr = MessageBox.Show("Your are about to delete a database. Are you sure?", "Delete Table", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        string query1 = $"DROP DATABASE {TrvDatabaseView.SelectedNode.Text};";
                        MySqlCommand command = new(query1, connection);
                        command.ExecuteNonQuery();

                        createServerTree();
                    }
                    catch (Exception exc)
                    {
                        errorMsg = exc.ToString();
                        RefreshErrorMsg();
                    }
                }
            }
            else if (TrvDatabaseView.SelectedNode.Text != TrvDatabaseView.SelectedNode.FullPath && TrvDatabaseView.SelectedNode.Nodes.Count == 0)
            {
                Interaction.MsgBox("no database selected");
            }
            else
            {
                Interaction.MsgBox("no database selected");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //label1.Hide();
            try
            {
                FileStream fs = new("saveFile.txt", FileMode.Open);
                StreamReader sr = new(fs);
                while (sr.Peek() != -1)
                {
                    string? row = sr.ReadLine();
                    if (row is not null)
                    {
                        string[] part = row.Split(";");
                        serverText = part[0];
                        userText = part[1];
                        passwordText = part[2];
                        databaseText = part[3];
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
                RefreshErrorMsg();
            }
        }

        private DataGridView createDataGridViewObject(string name)
        {
            DataGridView gridView = new();
            gridView.Parent = tabPage1;
            gridView.Location = tabPage1.Location;
            gridView.Name = name;
            gridView.Size = new Size(1033, 200);
            gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            return gridView;
        }

        private void TrvDatabaseView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            createNewTableModeEnabled = false;
            label1.Text = "count: " + e.Node.Nodes.Count.ToString();
            if (!dataGridCreated)
            {
                gridView = createDataGridViewObject("tableView");
                dataGridCreated = true;
            }
            else
            {
                gridView.DataSource = null;
                gridView.Rows.Clear();
            }
            if (lastUsedDatabase != e.Node.Text)
            {
                TreeNode lastParent = e.Node.Parent;
                if (lastParent == null)
                {
                    lastUsedDatabase = e.Node.Text;
                    string query1 = $"USE {lastUsedDatabase};";
                    MySqlCommand command = new(query1, connection);
                    command.ExecuteNonQuery();
                }
                while (lastParent != null)
                {
                    label1.Text += $"test";
                    if (lastParent.Parent == null)
                    {
                        if (e.Node.Text == e.Node.FullPath) lastUsedDatabase = e.Node.Text;
                        else lastUsedDatabase = lastParent.Text;
                        label1.Text += $", '{lastUsedDatabase}' ";
                        string query1 = $"USE {lastUsedDatabase};";
                        MySqlCommand command = new(query1, connection);
                        command.ExecuteNonQuery();
                    }
                    lastParent = lastParent.Parent;
                }
            }
            try
            {
                if (e.Node.Text == e.Node.FullPath && e.Node.Nodes.Count > 0)
                {
                    string query = $"SHOW TABLES;";
                    MySqlDataAdapter da = new(query, connection);
                    DataSet ds = new();
                    da.Fill(ds, e.Node.Text);
                    DataTable dt = ds.Tables[e.Node.Text];
                    gridView.DataSource = dt;

                }
                else if (e.Node.Text == e.Node.FullPath && e.Node.Nodes.Count == 0)
                {
                    string query = $"SHOW TABLES;";
                    MySqlDataAdapter da = new(query, connection);
                    DataSet ds = new();
                    da.Fill(ds, e.Node.Text);
                    DataTable dt = ds.Tables[e.Node.Text];
                    gridView.DataSource = dt;
                }
                else if (e.Node.Text != e.Node.FullPath && e.Node.Nodes.Count == 0)
                {
                    int counter = 0;
                    TreeNode lastParent = e.Node.Parent;
                    while (lastParent != null)
                    {
                        lastParent = lastParent.Parent;
                        counter++;
                    }
                    string query = "";
                    if (counter > 1)
                    {
                        query = $"SELECT {e.Node.Text} FROM {e.Node.Parent.Text};";
                    }
                    else
                    {
                        connection.Close();
                        connection.Open();
                        query = $"USE {e.Node.Parent.Text}; SELECT * FROM {e.Node.Text};";
                    }
                    label1.Text += query;
                    MySqlDataAdapter da = new(query, connection);
                    DataSet ds = new();
                    da.Fill(ds, e.Node.Text);
                    DataTable dt = ds.Tables[e.Node.Text];

                    gridView.DataSource = dt;
                }
                else
                {
                    string query = $"SELECT * FROM {e.Node.Text};";
                    MySqlDataAdapter da = new(query, connection);
                    DataSet ds = new();
                    da.Fill(ds, e.Node.Text);
                    DataTable dt = ds.Tables[e.Node.Text];

                    gridView.DataSource = dt;
                }
            }
            catch (Exception exc)
            {
                errorMsg = exc.ToString();
                RefreshErrorMsg();
            }
        }

        private void beendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Add(tabPage1);
        }

        private void BtnCreateTable_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                Form3 formCreateTable = new Form3();
                formCreateTable.ShowDialog();
            }
            else
            {
                Interaction.MsgBox("Not connected to a database");
            }
        }

        private void BtnCreateNewTable_Click(object sender, EventArgs e)
        {
            createNewTableModeEnabled = true;
            if (!dataGridCreated)
            {
                gridView = createDataGridViewObject("tableView");
                newTableToolStrip.Text = "Add Attribute";
                gridView.CellMouseClick += new DataGridViewCellMouseEventHandler(AddContextMenu);
                //newTableToolStrip.Click += GridViewMouseArgs;
                ContextMenuStrip strip = new ContextMenuStrip();
                dataGridCreated = true;
                DataTable dt = new DataTable();
                dt.Columns.Add("Add Column");
                dt.Rows.Add("Add Row");
                gridView.ContextMenuStrip = strip;
                gridView.ContextMenuStrip.Items.Add(newTableToolStrip);
                gridView.DataSource = dt;
            }
            else
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Add Column");
                dt.Rows.Add("Add Row");
                gridView.DataSource = dt;
            }
        }

        public void createTableInDatabase(string query)
        {
            try
            {
                MySqlCommand command = new(query, connection);
                command.ExecuteNonQuery();
                createServerTree();
            }
            catch (Exception exc)
            {
                errorMsg = exc.ToString();
                RefreshErrorMsg();
            }
        }

        private void BtnDeleteTable_Click(object sender, EventArgs e)
        {
            string parent = null;

            if (TrvDatabaseView.SelectedNode.Text == TrvDatabaseView.SelectedNode.FullPath && TrvDatabaseView.SelectedNode.Nodes.Count > 0)
            {
                Interaction.MsgBox("no table selected");
            }
            else if (TrvDatabaseView.SelectedNode.Text != TrvDatabaseView.SelectedNode.FullPath && TrvDatabaseView.SelectedNode.Nodes.Count == 0)
            {
                TreeNode lastParent = TrvDatabaseView.SelectedNode.Parent;
                int counter = 0;
                while (lastParent != null)
                {
                    lastParent = lastParent.Parent;
                    counter++;
                }
                if (counter > 1)
                {
                    Interaction.MsgBox("no table selected");
                }
                else
                {
                    DialogResult dr = MessageBox.Show("Your are about to delete a table. Are you sure?", "Delete Table", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.Yes)
                    {
                        try
                        {
                            string query1 = $"DROP TABLE {TrvDatabaseView.SelectedNode.Text};";
                            MySqlCommand command = new(query1, connection);
                            command.ExecuteNonQuery();

                            createServerTree();
                        }
                        catch (Exception exc)
                        {
                            errorMsg = exc.ToString();
                            RefreshErrorMsg();
                        }
                    }
                }
            }
            else
            {
                parent = TrvDatabaseView.SelectedNode.Parent.Text;

                DialogResult dr = MessageBox.Show("Your are about to delete a table. Are you sure?", "Delete Table", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        string query1 = $"USE {parent}; DROP TABLE {TrvDatabaseView.SelectedNode.Text};";
                        MySqlCommand command = new(query1, connection);
                        command.ExecuteNonQuery();

                        createServerTree();
                    }
                    catch (Exception exc)
                    {
                        errorMsg = exc.ToString();
                        RefreshErrorMsg();
                    }
                }
            }
        }
    }
}
