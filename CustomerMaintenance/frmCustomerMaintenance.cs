using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CustomerMaintenance.Model.DataLayer;

namespace CustomerMaintenance
{
    public partial class frmCustomerMaintenance : Form
    {
        public frmCustomerMaintenance()
        {
            InitializeComponent();
        }

        private MMABooksContext context = new MMABooksContext();
        private Customers selectedCustomer;

        // private constants for the index values of the Modify and Delete button columns
        //private const int ModifyIndex = 6;
        //private const int DeleteIndex = 7;

        private void frmCustomerMaintenance_Load(object sender, EventArgs e)
        {
            DisplayCustomers();
        }

      
        private void DisplayCustomers()
        {
            //clearing the grid each time add or delete to redisplay list.
            dgvCustomers.Columns.Clear();  
            // get customers result of Query and bind grid
            var customers = context.Customers
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.CustomerId,
                    c.Name,
                    c.Address,
                    c.City,
                    c.State,
                    c.ZipCode
                })
                .ToList();
         
            dgvCustomers.DataSource = customers;
           
            //format Column grid 1-6. setting widthfor each indexed/property in the collection into columns. 
            dgvCustomers.Columns[0].Visible = false;
            dgvCustomers.Columns[0].HeaderText = "Customer ID";
            dgvCustomers.Columns[0].Width = 110;
            dgvCustomers.Columns[1].HeaderText = "NAME";
            dgvCustomers.Columns[1].Width = 200;
            dgvCustomers.Columns[2].HeaderText = "Address";
            dgvCustomers.Columns[2].Width = 275;
            dgvCustomers.Columns[3].HeaderText = "City";
            dgvCustomers.Columns[3].Width = 100;
            dgvCustomers.Columns[4].HeaderText = "State";
            dgvCustomers.Columns[4].Width = 70;
            dgvCustomers.Columns[5].HeaderText = "Zip Code";
            dgvCustomers.Columns[5].Width = 100;           

            //Adding modify button column (not on the form) 
            var modifycolumn = new DataGridViewButtonColumn()
            {
                UseColumnTextForButtonValue = true,
                HeaderText = "Modify Customer",
                Text = "Modify"
            };
            dgvCustomers.Columns.Add(modifycolumn);

            //Adding delete button column (not on the form) 
            var deletecolumn = new DataGridViewButtonColumn()
            {
                UseColumnTextForButtonValue = true,
                HeaderText = "Delete Customer",
                Text = "Delete"
            };
            dgvCustomers.Columns.Add(deletecolumn);

            //setting color for display and readability.
            dgvCustomers.EnableHeadersVisualStyles = false;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Arial", 9, FontStyle.Bold);
            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkSeaGreen;
            dgvCustomers.AlternatingRowsDefaultCellStyle.BackColor =
                Color.PaleGoldenrod;
            dgvCustomers.Columns[6].DefaultCellStyle.BackColor = Color.AntiqueWhite;
               
                    }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            const int ModifyIndex = 6;
            const int DeleteIndex = 7;

            if (e.ColumnIndex == ModifyIndex || e.ColumnIndex == DeleteIndex)
            {
                int CustomerId = Convert.ToInt32(dgvCustomers.Rows[e.RowIndex].Cells[0].Value.ToString().Trim());
                selectedCustomer = context.Customers.Find(CustomerId);
            }

            if (e.ColumnIndex == ModifyIndex)
                ModifyCustomer();
            if (e.ColumnIndex == DeleteIndex)
                DeleteCustomer();
        }

        private void ModifyCustomer()
        {
            var addModifyCustomerForm = new frmAddModifyCustomer()
            {
                AddCustomer = false,
                Customer = selectedCustomer,
                States = context.States.OrderBy(s => s.StateName).ToList()
            };

            DialogResult result = addModifyCustomerForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    selectedCustomer = addModifyCustomerForm.Customer;
                    context.SaveChanges();
                    DisplayCustomers();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    HandleConcurrencyError(ex);
                }
                catch (DbUpdateException ex)
                {
                    HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    HandleGeneralError(ex);
                }
            }
        }

        private void DeleteCustomer()
        {
            DialogResult result =
                MessageBox.Show($"Delete {selectedCustomer.Name}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    context.Customers.Remove(selectedCustomer);
                    context.SaveChanges();
                    DisplayCustomers();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    HandleConcurrencyError(ex);
                }
                catch (DbUpdateException ex)
                {
                    HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    HandleGeneralError(ex);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var addModifyCustomerForm = new frmAddModifyCustomer()
            {
                AddCustomer = true,
                States = context.States.OrderBy(s => s.StateName).ToList()
            };
            DialogResult result = addModifyCustomerForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    selectedCustomer = addModifyCustomerForm.Customer;
                    context.Customers.Add(selectedCustomer);
                    context.SaveChanges();
                    DisplayCustomers();
                }
                catch (DbUpdateException ex)
                {
                    HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    HandleGeneralError(ex);
                }
            }
        }

        private void HandleConcurrencyError(DbUpdateConcurrencyException ex)
        {
            ex.Entries.Single().Reload();

            var state = context.Entry(selectedCustomer).State;
            if (state == EntityState.Detached)
            {
                MessageBox.Show("Another user has deleted that product.",
                    "Concurrency Error");
            }
            else
            {
                string message = "Another user has updated that product.\n" +
                    "The current database values will be displayed.";
                MessageBox.Show(message, "Concurrency Error");
            }
            this.DisplayCustomers();
        }

        private void HandleDatabaseError(DbUpdateException ex)
        {
            string errorMessage = "";
            var sqlException = (SqlException)ex.InnerException;
            foreach (SqlError error in sqlException.Errors)
            {
                errorMessage += "ERROR CODE:  " + error.Number + " " +
                                error.Message + "\n";
            }
            MessageBox.Show(errorMessage);
        }

        private void HandleGeneralError(Exception ex)
        {
            MessageBox.Show(ex.Message, ex.GetType().ToString());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
