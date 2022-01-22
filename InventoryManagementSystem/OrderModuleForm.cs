using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class OrderModuleForm : Form
    {
        SqlConnection con = new SqlConnection(Connection.ConnectionString);
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        int qty = 0;
        int number = 1;
        ArrayList datagridValues = new ArrayList();
        ArrayList totalPriceOfCart = new ArrayList();
        public OrderModuleForm()
        {
            InitializeComponent();
            LoadCustomer();
            LoadProduct();
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
           
        }

        public void LoadCustomer()
        {
            int i = 0;
            dgvCustomer.Rows.Clear();
            cm = new SqlCommand("SELECT cid, cname FROM tbCustomer WHERE CONCAT(cid,cname) LIKE '%"+txtSearchCust.Text+"%'", con);
            con.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvCustomer.Rows.Add(i, dr[0].ToString(), dr[1].ToString());
            }
            dr.Close();
            con.Close();
        }

        public void LoadProduct()
        {
            int i = 0;
            dgvProduct.Rows.Clear();
            cm = new SqlCommand("SELECT * FROM tbProduct WHERE CONCAT(pid, pname, pprice, pdescription, pcategory) LIKE '%" + txtSearchProd.Text + "%'", con);
            con.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvProduct.Rows.Add(i, dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString(), dr[5].ToString());
            }
            dr.Close();
            con.Close();
        }

        private void txtSearchCust_TextChanged(object sender, EventArgs e)
        {
            LoadCustomer();
        }

        private void txtSearchProd_TextChanged(object sender, EventArgs e)
        {
            LoadProduct();
        }

        
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (UDQty.Value <= 0)
            {
                MessageBox.Show("Please Do greater than 0");
                UDQty.Value = 1;
            }
            else
            {
                GetQty();
                if (Convert.ToInt16(UDQty.Value) > qty)
                {
                    MessageBox.Show("Instock quantity is not enough!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UDQty.Value = UDQty.Value - 1;
                    return;
                }
                if (Convert.ToInt16(UDQty.Value) > 0)
                {
                    int total = Convert.ToInt16(txtPrice.Text) * Convert.ToInt16(UDQty.Value);
                    txtTotal.Text = total.ToString();
                }
            }
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtCId.Text = dgvCustomer.Rows[e.RowIndex].Cells[1].Value.ToString();
            txtCName.Text = dgvCustomer.Rows[e.RowIndex].Cells[2].Value.ToString();
        }

        private void dgvProduct_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtPid.Text = dgvProduct.Rows[e.RowIndex].Cells[1].Value.ToString();
            txtPName.Text = dgvProduct.Rows[e.RowIndex].Cells[2].Value.ToString();
            txtPrice.Text = dgvProduct.Rows[e.RowIndex].Cells[4].Value.ToString();
            int total = Convert.ToInt16(txtPrice.Text) * Convert.ToInt16(UDQty.Value);
            txtTotal.Text = total.ToString();
        }

      

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtCId.Text == "")
                {
                    MessageBox.Show("Please select customer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txtPid.Text == "")
                {
                    MessageBox.Show("Please select product!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show("Are you sure you want to insert this order?", "Saving Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    cm = new SqlCommand("INSERT INTO tbOrder(odate, pid, cid, qty, price, total)VALUES(@odate, @pid, @cid, @qty, @price, @total)", con);
                    cm.Parameters.AddWithValue("@odate", dtOrder.Value);
                    cm.Parameters.AddWithValue("@pid", Convert.ToInt32(txtPid.Text));
                    cm.Parameters.AddWithValue("@cid", Convert.ToInt32(txtCId.Text));
                    cm.Parameters.AddWithValue("@qty", Convert.ToInt32(UDQty.Value));
                    cm.Parameters.AddWithValue("@price", Convert.ToInt32(txtPrice.Text));
                    cm.Parameters.AddWithValue("@total", Convert.ToInt32(txtTotal.Text));
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("Order has been successfully inserted.");
                    

                    cm = new SqlCommand("UPDATE tbProduct SET pqty=(pqty-@pqty) WHERE pid LIKE '"+ txtPid.Text +"' ", con);                    
                    cm.Parameters.AddWithValue("@pqty", Convert.ToInt16(UDQty.Value));
                   
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                    Clear();
                    LoadProduct();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Clear()
        {
            txtCId.Clear();
            txtCName.Clear();

            txtPid.Clear();
            txtPName.Clear();

            txtPrice.Clear();
            UDQty.Value = 0;
            txtTotal.Clear();
            dtOrder.Value = DateTime.Now;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();                        
        }

        public void GetQty()
        {
            cm = new SqlCommand("SELECT pqty FROM tbProduct WHERE pid='"+ txtPid.Text +"'", con);
            con.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                qty = Convert.ToInt32(dr[0].ToString());
            }
            dr.Close();
            con.Close();
        }

        private void car_button_Click(object sender, EventArgs e)
        {
            number++;
            if (datagridValues.Contains(txtPName.Text))
            {
                MessageBox.Show("This Item Present in The List Please select another or delete to insert Again");
            }
            else
            {
                string serialColumn = number.ToString();
                string firstColumn = txtPName.Text;
                datagridValues.Add(firstColumn);
                string secondColumn = UDQty.Text;
                string thirdColumn = "";
                if (txtTotal.Text == "")
                {
                    thirdColumn = txtPrice.Text;
                }
                else
                {
                    thirdColumn = txtTotal.Text;
                }
                totalPriceOfCart.Add(Convert.ToInt32(thirdColumn));
                string[] rows = { serialColumn, firstColumn, secondColumn, thirdColumn };
                cart_DataGridView.Rows.Add(rows);
                totalCalculation();
            }
        }

        private void totalCalculation() {
            int cal = 0;
            foreach (int item in totalPriceOfCart)
            {
                cal = cal + item;
            }
            totalPriceOfCart_TextBox.Text =  cal.ToString();
        }
        private void clear_DataGrid_Click(object sender, EventArgs e)
        {
            cart_DataGridView.Rows.Clear();
            datagridValues.Clear();
            totalPriceOfCart.Clear(); 
        }

        private void cart_DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //If this is header row or new row, do nothing
            if (e.RowIndex < 0 || e.RowIndex == this.cart_DataGridView.NewRowIndex)
                return;

            //If formatting your desired column, set the value
            if (e.ColumnIndex == 4)
            {
                
                datagridValues.Remove(cart_DataGridView.Rows[e.RowIndex].Cells[1].Value.ToString());
                totalPriceOfCart.Remove(Convert.ToInt32(cart_DataGridView.Rows[e.RowIndex].Cells[3].Value));
                MessageBox.Show("The Value from Grid is : " + cart_DataGridView.Rows[e.RowIndex].Cells[3].Value.ToString());

                cart_DataGridView.Rows.RemoveAt(e.RowIndex);
                totalCalculation();
            }
        }
    }
}
