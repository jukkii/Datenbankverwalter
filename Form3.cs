using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Datenbankverwalter
{
    public partial class Form3 : Form
    {

        public static event EventHandler? SelectedIndexChanged;
        public Form3()
        {

            InitializeComponent();
            CmBType.Items.Add("INT()");
            CmBType.Items.Add("VARCHAR()");
            CmBType.Items.Add("FLOAT()");
            CmbDefault.Items.Add("NULL");
            CmbDefault.Items.Add("NOT NULL");
            CmBType.SelectionChangeCommitted += new EventHandler(this.OnTypeBoxIndexChanged);
        } 

        private void BtnAddAttribute_Click(object sender, EventArgs e)
        {
            if (TxtName.Text != "")
            {
                if (CmBType.Text != "")
                {
                    if (LiBName.Items.Count > 0)
                    {
                        bool isUsed = false;
                        for (int i = 0; i < LiBName.Items.Count; i++)
                        {
                            if (LiBName.Items[i].ToString() == TxtName.Text)
                            {
                                isUsed = true;
                                break;
                            }
                            else
                            {
                                isUsed = false;
                            }
                        }

                        if (isUsed)
                        {
                            Interaction.MsgBox("Name already in use");
                        }
                        else
                        {
                            if (CmbDefault.Text == "NOT NULL" && TxtValue.Text == "")
                            {
                                Interaction.MsgBox("Value can't be empty when type=NOT NULL");
                            }
                            else
                            {
                                if (CmbDefault.Text != "")
                                {
                                    LiBDefault.Items.Add(CmbDefault.Text);
                                }
                                else LiBDefault.Items.Add("NULL");
                                LiBName.Items.Add(TxtName.Text);
                                LiBType.Items.Add(CmBType.Text);
                                LiBValue.Items.Add(TxtValue.Text);
                            }
                        }
                    }
                    else
                    {
                        if (CmbDefault.Text == "NOT NULL" && TxtValue.Text == "")
                        {
                            Interaction.MsgBox("Value can't be empty when type=NOT NULL");
                        }
                        else
                        {
                            if (CmbDefault.Text != "")
                            {
                                LiBDefault.Items.Add(CmbDefault.Text);
                            }
                            else LiBDefault.Items.Add("NULL");
                            LiBName.Items.Add(TxtName.Text);
                            LiBType.Items.Add(CmBType.Text);
                            LiBValue.Items.Add(TxtValue.Text);
                        }
                    }
                }
                else Interaction.MsgBox("Type can't be empty");
            }
            else Interaction.MsgBox("Name can't be empty");
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnCreateTable_Click(object sender, EventArgs e)
        {
            string query1 = "";
            int counter = 0;
            if (LiBName.Items.Count > 0)
            {
                query1 += $"CREATE TABLE {TxtTableName.Text} (";

                for (int i = 0; i < LiBName.Items.Count; i++)
                {
                    query1 += $"{LiBName.Items[i]} {LiBType.Items[i]} {LiBDefault.Items[i]}";
                    if (counter < LiBName.Items.Count - 1) query1 += ",";
                    counter++;
                }
                counter = 0;
                query1 += ");";
                if (LiBValue.Items.Count > 0)
                {
                    query1 += $"INSERT INTO {TxtTableName.Text}(";
                    for (int i = 0; i < LiBName.Items.Count; i++)
                    {
                        query1 += $"{LiBName.Items[i]}";
                        if (counter < LiBName.Items.Count - 1) query1 += ",";
                        counter++;
                    }
                    counter = 0;
                    query1 += ") VALUES(";
                    for (int i = 0; i < LiBValue.Items.Count; i++)
                    {
                        string outputString = LiBType.Items[i].ToString().Substring(0, LiBType.Items[i].ToString().IndexOf('('));
                        //Interaction.MsgBox(outputString);
                        if (LiBValue.Items[i].ToString() != "")
                            if (outputString == "VARCHAR")
                                query1 += $"'{LiBValue.Items[i]}'";
                            else
                                query1 += $"{LiBValue.Items[i]}";
                        else query1 += "NULL";
                        if (counter < LiBName.Items.Count - 1) query1 += ",";
                        counter++;
                    }
                    query1 += ");";
                }
            }
            else Interaction.MsgBox("cant create table with no attribute");
            var mainForm = Application.OpenForms.OfType<Form1>().Single();
            Close();
            mainForm.createTableInDatabase(query1);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LiBDefault.Items.Clear();
            LiBName.Items.Clear();
            LiBType.Items.Clear();
            LiBValue.Items.Clear();
        }

        private void BtnDeleteAttribute_Click(object sender, EventArgs e)
        {
            int selIndex = -1;
            if (LiBName.SelectedIndex != -1)
            {
                selIndex = LiBName.SelectedIndex;
            }
            else if (LiBType.SelectedIndex != -1)
            {
                selIndex = LiBType.SelectedIndex;
            }
            else if (LiBDefault.SelectedIndex != -1)
            {
                selIndex = LiBDefault.SelectedIndex;
            }
            else if (LiBValue.SelectedIndex != -1)
            {
                selIndex = LiBValue.SelectedIndex;
            }
            if (selIndex != -1)
            {
                LiBType.Items.RemoveAt(selIndex);
                LiBDefault.Items.RemoveAt(selIndex);
                LiBName.Items.RemoveAt(selIndex);
                LiBValue.Items.RemoveAt(selIndex);
            }
        }

        private void BtnChangeType_Click(object sender, EventArgs e)
        {
            if (LiBType.SelectedIndex != -1)
            {
                LiBType.Items[LiBType.SelectedIndex] = CmBType.Text;
            }
        }

        private void BtnChangeDefault_Click(object sender, EventArgs e)
        {
            if (LiBDefault.SelectedIndex != -1)
            {
                LiBDefault.Items[LiBDefault.SelectedIndex] = CmbDefault.Text;
            }
        }
        private void BtnChangeName_Click(object sender, EventArgs e)
        {
            if (LiBName.SelectedIndex != -1)
            {
                LiBName.Items[LiBName.SelectedIndex] = TxtName.Text;
            }
        }
        private void BtnChangeValue_Click(object sender, EventArgs e)
        {
            if (LiBValue.SelectedIndex != -1)
            {
                LiBValue.Items[LiBValue.SelectedIndex] = TxtValue.Text;
            }
        }

        private void OnTypeBoxIndexChanged(object? sender, EventArgs e)
        {
            Interaction.MsgBox("wejfhbw");
            CmBType.SelectionStart = 0;
            CmBType.SelectionLength = 0;
        }

        private void CmbDefault_SelectedIndexChanged(object sender, EventArgs e)
        {
            CmbDefault.Select(CmbDefault.Text.Length, 2);
        }
    }
}
