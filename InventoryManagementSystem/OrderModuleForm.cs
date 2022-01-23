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
        private string reciptNumber = "";
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
            //If this is header row or new row, do nothing
            if (e.RowIndex < 0 || e.RowIndex == this.cart_DataGridView.NewRowIndex)
                return;
            txtCId.Text = dgvCustomer.Rows[e.RowIndex].Cells[1].Value.ToString();
            txtCName.Text = dgvCustomer.Rows[e.RowIndex].Cells[2].Value.ToString();
        }

        private void dgvProduct_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //If this is header row or new row, do nothing
            if (e.RowIndex < 0 || e.RowIndex == this.cart_DataGridView.NewRowIndex)
                return;
            txtPid.Text = dgvProduct.Rows[e.RowIndex].Cells[1].Value.ToString();
            txtPName.Text = dgvProduct.Rows[e.RowIndex].Cells[2].Value.ToString();
            txtPrice.Text = dgvProduct.Rows[e.RowIndex].Cells[4].Value.ToString();
            int total = Convert.ToInt16(txtPrice.Text) * Convert.ToInt16(UDQty.Value);
            txtTotal.Text = total.ToString();
        }

        private readonly Random _random = new Random();

        public string RandomString(int size = 4, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }
            int num = _random.Next(9999);
            return lowerCase ? builder.ToString().ToLower() : builder.ToString() +""+ num.ToString();
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
                    reciptNumber = RandomString();
                    cm = new SqlCommand("INSERT INTO tbOrder(odate, pid, cid, qty, price, total, receiptID ) VALUES (@odate, @pid, @cid, @qty, @price, @total, @receiptID)", con);
                    
                    //
                    //
                    //
                    for (int i = 0; i < cart_DataGridView.Rows.Count; i++)
                    {
                        cm.Parameters.AddWithValue("@odate", dtOrder.Value);
                        //
                        cm.Parameters.AddWithValue("@cid", Convert.ToInt32(txtCId.Text));
                        cm.Parameters.AddWithValue("@pid", Convert.ToInt32(cart_DataGridView.Rows[i].Cells[5].Value));
                        cm.Parameters.AddWithValue("@qty", Convert.ToInt32(cart_DataGridView.Rows[i].Cells[2].Value));
                        cm.Parameters.AddWithValue("@price", Convert.ToInt32(cart_DataGridView.Rows[i].Cells[3].Value));
                        cm.Parameters.AddWithValue("@total", Convert.ToInt32(cart_DataGridView.Rows[i].Cells[4].Value));
                        //
                        cm.Parameters.AddWithValue("@receiptID", reciptNumber.ToString());
                        con.Open();
                        cm.ExecuteNonQuery();
                        con.Close();
                        cm.Parameters.Clear();
                    }
                    
                    MessageBox.Show("Order has been successfully inserted.");


                    cm = new SqlCommand("UPDATE tbProduct SET pqty=(pqty-@pqty) WHERE pid LIKE '" + txtPid.Text + "' ", con);
                    cm.Parameters.AddWithValue("@pqty", Convert.ToInt16(UDQty.Value));
                    printDocumentMethod();
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                    Clear();
                    LoadProduct();
                    cart_DataGridView.Rows.Clear();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void printDocumentMethod() {
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }
        public void Clear()
        {
            txtCId.Clear();
            txtCName.Clear();

            txtPid.Clear();
            txtPName.Clear();

            txtPrice.Clear();
            UDQty.Value = 1;
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
                string thirdColumn = txtPrice.Text;
                string fourthColumn = "";
                if (txtTotal.Text == "")
                {
                    fourthColumn = txtPrice.Text;
                }
                else
                {
                    fourthColumn = txtTotal.Text;
                }
                totalPriceOfCart.Add(Convert.ToInt32(fourthColumn));
                
                string fifthColumn = txtPid.Text;
                string[] rows = { serialColumn, firstColumn, secondColumn, thirdColumn,fourthColumn,fifthColumn };
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cart_DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //If this is header row or new row, do nothing
            if (e.RowIndex < 0 || e.RowIndex == this.cart_DataGridView.NewRowIndex)
                return;

            //If formatting your desired column, set the value
            if (e.ColumnIndex == 6)
            {
                
                datagridValues.Remove(cart_DataGridView.Rows[e.RowIndex].Cells[1].Value.ToString());
                totalPriceOfCart.Remove(Convert.ToInt32(cart_DataGridView.Rows[e.RowIndex].Cells[4].Value));
                //MessageBox.Show("The Value from Grid is : " + cart_DataGridView.Rows[e.RowIndex].Cells[3].Value.ToString());

                cart_DataGridView.Rows.RemoveAt(e.RowIndex);
                totalCalculation();
            }
        }
        StringFormat strFormat;
        ArrayList arrColumnLefts = new ArrayList();
        ArrayList arrColumnWidths = new ArrayList();
        int iCellHeight, iCount, iTotalWidth, iHeaderHeight, iRow;
        bool bFirstPage, bNewPage;
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            

            try
            {
                //Set the left margin
                int iLeftMargin = e.MarginBounds.Left;
                //Set the top margin
                int iTopMargin = e.MarginBounds.Top;
                //Whether more pages have to print or not
                bool bMorePagesToPrint = false;
                int iTmpWidth = 0;

                //For the first page to print set the cell width and header height
                if (bFirstPage)
                {
                    foreach (DataGridViewColumn GridCol in cart_DataGridView.Columns)
                    {
                        iTmpWidth = (int)(Math.Floor((double)((double)GridCol.Width /
                            (double)iTotalWidth * (double)iTotalWidth *
                            ((double)e.MarginBounds.Width / (double)iTotalWidth))));

                        iHeaderHeight = (int)(e.Graphics.MeasureString(GridCol.HeaderText,
                            GridCol.InheritedStyle.Font, iTmpWidth).Height) + 11;

                        // Save width and height of headers
                        arrColumnLefts.Add(iLeftMargin);
                        arrColumnWidths.Add(iTmpWidth);
                        iLeftMargin += iTmpWidth;
                    }
                }
                //Loop till all the grid rows not get printed
                while (iRow <= cart_DataGridView.Rows.Count - 1)
                {
                    DataGridViewRow GridRow = cart_DataGridView.Rows[iRow];
                    //Set the cell height
                    iCellHeight = GridRow.Height + 5;
                    int iCount = 0;
                    //Check whether the current page settings allows more rows to print
                    if (iTopMargin + iCellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                    {
                        bNewPage = true;
                        bFirstPage = false;
                        bMorePagesToPrint = true;
                        break;
                    }
                    else
                    {
                        if (bNewPage)
                        {
                            e.Graphics.DrawString("Gillani Book Shop", new Font("Arial", 22, FontStyle.Regular), Brushes.Black, new Point(280, 0));
                            //Draw Header
                            e.Graphics.DrawString("Customer : "+txtCName.Text,
                                new Font(cart_DataGridView.Font, FontStyle.Bold),
                                Brushes.Black, e.MarginBounds.Left,
                                e.MarginBounds.Top - e.Graphics.MeasureString("Gillani Book Shop",
                                new Font(cart_DataGridView.Font, FontStyle.Bold),
                                e.MarginBounds.Width).Height - 13);

                            String strDate = DateTime.Now.ToLongDateString() + " " +
                                DateTime.Now.ToShortTimeString();
                            //Draw Date
                            e.Graphics.DrawString(strDate,
                                new Font(cart_DataGridView.Font, FontStyle.Bold), 
                                Brushes.Black, e.MarginBounds.Left +
                                (e.MarginBounds.Width - e.Graphics.MeasureString(strDate,
                                new Font(cart_DataGridView.Font, FontStyle.Bold),
                                e.MarginBounds.Width).Width),
                                e.MarginBounds.Top - e.Graphics.MeasureString("Gillani Book Shop",
                                new Font(new Font(cart_DataGridView.Font, FontStyle.Bold),
                                FontStyle.Bold), e.MarginBounds.Width).Height - 13);
                            //Draw Receipt Number
                            e.Graphics.DrawString("Receipt Number : " + reciptNumber,
                                new Font(cart_DataGridView.Font, FontStyle.Bold),
                                Brushes.Black,300,
                                e.MarginBounds.Top - e.Graphics.MeasureString("Gillani Book Shop",
                                new Font(new Font(cart_DataGridView.Font, FontStyle.Bold),
                                FontStyle.Bold), e.MarginBounds.Width).Height - 13);

                            //Draw Columns                 
                            iTopMargin = e.MarginBounds.Top;
                            for (int i = 0; i < cart_DataGridView.Columns.Count - 2; i++)
                            {
                                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray),
                                    new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                    (int)arrColumnWidths[iCount], iHeaderHeight));

                                e.Graphics.DrawRectangle(Pens.Black,
                                    new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                    (int)arrColumnWidths[iCount], iHeaderHeight));

                                e.Graphics.DrawString(cart_DataGridView.Columns[i].HeaderText,
                                    cart_DataGridView.Columns[i].InheritedStyle.Font,
                                    new SolidBrush(cart_DataGridView.Columns[i].InheritedStyle.ForeColor),
                                    new RectangleF((int)arrColumnLefts[iCount], iTopMargin,
                                    (int)arrColumnWidths[iCount], iHeaderHeight), strFormat);
                                iCount++;
                            }
                            /// <summary>
                            /// If To Draw all Data Grid use foreach Loop to show instead of for loop
                            /// </summary>
                            /// <param name="sender"></param>
                            /// <param name="e"></param>
                            //foreach (DataGridViewColumn GridCol in cart_DataGridView.Columns)
                            //{
                            //    e.Graphics.FillRectangle(new SolidBrush(Color.LightGray),
                            //        new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                            //        (int)arrColumnWidths[iCount], iHeaderHeight));

                            //    e.Graphics.DrawRectangle(Pens.Black,
                            //        new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                            //        (int)arrColumnWidths[iCount], iHeaderHeight));

                            //    e.Graphics.DrawString(GridCol.HeaderText,
                            //        GridCol.InheritedStyle.Font,
                            //        new SolidBrush(GridCol.InheritedStyle.ForeColor),
                            //        new RectangleF((int)arrColumnLefts[iCount], iTopMargin,
                            //        (int)arrColumnWidths[iCount], iHeaderHeight), strFormat);
                            //    iCount++;
                            //}
                            bNewPage = false;
                            iTopMargin += iHeaderHeight;
                        }
                        iCount = 0;
                        //Draw Columns Contents
                        for (int i = 0; i < GridRow.Cells.Count - 2; i++)
                        {
                            if (GridRow.Cells[i].Value != null)
                            {
                                e.Graphics.DrawString(GridRow.Cells[i].Value.ToString(),
                                    GridRow.Cells[i].InheritedStyle.Font,
                                    new SolidBrush(GridRow.Cells[i].InheritedStyle.ForeColor),
                                    new RectangleF((int)arrColumnLefts[iCount],
                                    (float)iTopMargin,
                                    (int)arrColumnWidths[iCount], (float)iCellHeight),
                                    strFormat);
                            }
                            //Drawing Cells Borders 
                            e.Graphics.DrawRectangle(Pens.Black,
                                new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                (int)arrColumnWidths[iCount], iCellHeight));
                            iCount++;
                        }
                        /// <summary>
                        /// If To Draw all Data Grid use foreach Loop to show instead of for loop
                        /// </summary>
                        /// <param name="sender"></param>
                        /// <param name="e"></param>
                        //foreach (DataGridViewCell Cel in GridRow.Cells)
                        //{
                            //if (Cel.Value != null)
                            //{
                            //    e.Graphics.DrawString(Cel.Value.ToString(),
                            //        Cel.InheritedStyle.Font,
                            //        new SolidBrush(Cel.InheritedStyle.ForeColor),
                            //        new RectangleF((int)arrColumnLefts[iCount],
                            //        (float)iTopMargin,
                            //        (int)arrColumnWidths[iCount], (float)iCellHeight),
                            //        strFormat);
                            //}
                            ////Drawing Cells Borders 
                            //e.Graphics.DrawRectangle(Pens.Black,
                            //    new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                            //    (int)arrColumnWidths[iCount], iCellHeight));
                            //iCount++;
                        //}
                    }
                    iRow++;
                    iTopMargin += iCellHeight;
                }
                //If more lines exist, print another page.
                if (bMorePagesToPrint)
                    e.HasMorePages = true;
                else
                    e.HasMorePages = false;
                e.Graphics.DrawString("Total : " + totalPriceOfCart_TextBox.Text, new Font("Arial", 16, FontStyle.Regular), Brushes.Black, new Point(500, iTopMargin));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
            }

        }
       
        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Near;
                strFormat.LineAlignment = StringAlignment.Center;
                strFormat.Trimming = StringTrimming.EllipsisCharacter;

                arrColumnLefts.Clear();
                arrColumnWidths.Clear();
                iCellHeight = 0;
                iCount = 0;
                bFirstPage = true;
                bNewPage = true;

                // Calculating Total Widths
                iTotalWidth = 0;
                foreach (DataGridViewColumn dgvGridCol in cart_DataGridView.Columns)
                {
                    iTotalWidth += dgvGridCol.Width;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
