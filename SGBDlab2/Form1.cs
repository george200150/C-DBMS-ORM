using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;

namespace SGBDlab2
{

    public partial class Form1 : Form
    {

        Table parent;
        Table child;

        SqlConnection connection = new SqlConnection(@"Data Source=DESKTOP-A1S24IB\SQLEXPRESS;Initial Catalog=movie;Integrated Security=True");
        //SqlConnection connection = new SqlConnection("Data Source=DESKTOP-A1S24IB\\SQLEXPRESS;Initial Catalog=...;Integrated Security=True");
        SqlDataAdapter adapter = new SqlDataAdapter();
        SqlDataAdapter adapter2 = new SqlDataAdapter();
        SqlDataAdapter adapter3 = new SqlDataAdapter();
        DataSet data = new DataSet();
        DataSet data2 = new DataSet();
        DataSet data3 = new DataSet();

        public Form1(Table parent, Table child)
        {
            InitializeComponent();
            this.parent = parent;
            this.child = child;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //numericUpDown2.Maximum = 2020;
            //numericUpDown2.Minimum = 1800;
            //numericUpDown2.Value = 2020;

            //TODO: create TextField-s and Label-s for each Table

        }

        private void button1_Click(object sender, EventArgs e) // - să se afişeze toate înregistrările tabelei părinte;
        {
            //adapter.SelectCommand = new SqlCommand("SELECT * FROM " + parent.Name, connection);

            string queryString = SQLQueryBuilder.SelectCommand(parent);
            adapter.SelectCommand = new SqlCommand(queryString, connection);
            data.Clear();
            adapter.Fill(data);
            dataGridView1.DataSource = data.Tables[0];//luam tabelul returnat de query
        }

        private void button2_Click(object sender, EventArgs e) // insert
        { // având selectată o înregistrare din părinte, se permite adăugarea unei noi înregistrări fiu
            try
            {
                //adapter.InsertCommand = new SqlCommand("INSERT INTO " + child.Name + " " + child.GetInsertParamsString() + " VALUES " + child.GetInsertAtParamsString(), connection); //TODO: variable number of insert fields
                string queryString = SQLQueryBuilder.InsertCommand(child);
                adapter.InsertCommand = new SqlCommand(queryString, connection);
                adapter.InsertCommand.Parameters.Add("@titlu", SqlDbType.VarChar).Value = textBox1.Text; //TODO: variable number of insert fields
                adapter.InsertCommand.Parameters.Add("@an_aparitie", SqlDbType.Int).Value = numericUpDown2.Text; //TODO: variable number of insert fields
                adapter.InsertCommand.Parameters.Add("@cod_director", SqlDbType.Int).Value = (int)dataGridView1.CurrentRow.Cells[0].Value; //TODO: variable number of insert fields
                connection.Open();
                adapter.InsertCommand.ExecuteNonQuery();
                //connection.Close();
                MessageBox.Show("ADAUGAT!");
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("SELECTATI O INREGISTRARE DIN TABELUL PARINTE!");
                //connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //connection.Close();
            }
            finally
            {
                connection.Close();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) // incarc toate inregistrarile compatibile din copil
        { // la selectarea unei înregistrări din părinte, se vor afişa toate înregistrările tabelei fiu
            try
            {
                //adapter2.SelectCommand = new SqlCommand("SELECT * FROM " + child.Name + " WHERE " + child.getFK()[0].Fname + " = @cod_director", connection);
                string queryString = SQLQueryBuilder.SelectWhereCommand(child);
                adapter2.SelectCommand = new SqlCommand(queryString, connection);

                int value = (int)dataGridView1.CurrentRow.Cells[0].Value;
                adapter2.SelectCommand.Parameters.Add("@cod_director", SqlDbType.Int).Value = value;
                data2.Clear();
                adapter2.Fill(data2);
                dataGridView2.DataSource = data2.Tables[0];//luam tabelul returnat de query
            }
            catch (Exception)
            {
                MessageBox.Show("YOU MUST SELECT A ROW FROM THE PARENT TABLE!");
            }
        }

        //TODO: this is not parametrised !!!
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) // incarc field-urile copilului
        { // la selectarea unei înregistrări din fiu, trebuie să se permită ştergerea sau actualizarea datelor acesteia
            DataGridViewRow row = dataGridView2.CurrentRow;
            if (row.Cells[1].Value is System.DBNull)
                return;
            string titlu = (string)row.Cells[1].Value;
            int an_aparitie = (int)row.Cells[2].Value;
            int cod_director = (int)row.Cells[3].Value;

            textBox2.Text = titlu;

            //numericUpDown1.Maximum = 2020;
            //numericUpDown1.Minimum = 1800;
            //numericUpDown1.Value = an_aparitie;

            List<int> directorList = new List<int>();

            foreach (DataGridViewRow oneRow in dataGridView1.Rows)
            {
                Object value = oneRow.Cells[0].Value;
                if (value != null)
                {
                    int cod = (int)value;
                    directorList.Add(cod);
                }
            }
            //comboBox1.DataSource = directorList;
            comboBox1.Text = cod_director.ToString();
        }

        private void button3_Click(object sender, EventArgs e) // sterge
        { // la selectarea unei înregistrări din fiu, trebuie să se permită ştergerea sau actualizarea datelor acesteia
            try
            {
                //adapter.DeleteCommand = new SqlCommand("DELETE FROM " + child.Name + " WHERE " + child.getPK()[0].Fname + " = @cod_film", connection); //TODO: what if multiple pk-s
                string queryString = SQLQueryBuilder.DeleteCommand(child);
                adapter.DeleteCommand = new SqlCommand(queryString, connection);
                List<Field> pks = child.getPK();
                foreach (Field pk in pks)
                {
                    adapter.DeleteCommand.Parameters.Add(pk.Fname, SqlDbType.Int).Value = (int)dataGridView2.CurrentRow.Cells[0].Value; // WWWWWWWWWW TODO WWWWWWWWWW : problema e ca nu stiu unde sunt pk-urile in tabel... .Cells[0] !!!
                }
                //adapter.DeleteCommand.Parameters.Add("@cod_film", SqlDbType.Int).Value = (int)dataGridView2.CurrentRow.Cells[0].Value;
                connection.Open();
                adapter.DeleteCommand.ExecuteNonQuery();
                //connection.Close();
                MessageBox.Show("STERS!");
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("SELECTATI O INREGISTRARE DIN TABELUL FIU!");
                //connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //connection.Close();
            }
            finally
            {
                connection.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e) // modifica
        { // la selectarea unei înregistrări din fiu, trebuie să se permită ştergerea sau actualizarea datelor acesteia
            try
            {
                //adapter.UpdateCommand = new SqlCommand("UPDATE " + child.Name + " SET titlu = @titlu ,an_aparitie = @an_aparitie, cod_director = @cod_director WHERE " + child.getPK()[0].Fname + " = @cod_film", connection);
                string queryString = SQLQueryBuilder.UpdateCommand(child);
                adapter.UpdateCommand = new SqlCommand(queryString, connection);
                List<Field> nonpks = child.getNonPK();
                foreach (Field f in nonpks)
                {
                    if (f.Type.Equals(DataTypeEnum.STRING))
                        adapter.UpdateCommand.Parameters.Add(f.Fname, SqlDbType.VarChar).Value = textBox2.Text; //TODO: maybe specify source input for each field in the beginning???
                    else if (f.Type.Equals(DataTypeEnum.INT))
                        adapter.UpdateCommand.Parameters.Add(f.Fname, SqlDbType.Int).Value = numericUpDown1.Text; //TODO: still hardcoded !!!   //= (int)dataGridView2.CurrentRow.Cells[0].Value;
                    else if (f.Type.Equals(DataTypeEnum.DATE))
                        adapter.UpdateCommand.Parameters.Add(f.Fname, SqlDbType.Date).Value = comboBox1.Text; //TODO: still hardcoded !!!
                }
                //adapter.UpdateCommand.Parameters.Add("@titlu", SqlDbType.VarChar).Value = textBox2.Text;
                //adapter.UpdateCommand.Parameters.Add("@an_aparitie", SqlDbType.Int).Value = numericUpDown1.Value;
                //adapter.UpdateCommand.Parameters.Add("@cod_director", SqlDbType.Int).Value = comboBox1.SelectedValue;
                //adapter.UpdateCommand.Parameters.Add("@cod_film", SqlDbType.Int).Value = (int)dataGridView2.CurrentRow.Cells[0].Value;
                connection.Open();
                adapter.UpdateCommand.ExecuteNonQuery();
                //connection.Close();
                MessageBox.Show("MODIFICAT!");
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("SELECTATI O INREGISTRARE DIN TABELUL FIU!");
                //connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //connection.Close();
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
