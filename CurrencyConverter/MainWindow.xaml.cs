using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
namespace CurrencyConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

   
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;
        SqlCommand sqlCommand;
        SqlDataAdapter sqlDataAdapter;

        private int CurrencyId = 0;
        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
            GetData();
        }

        public void myConnection()
        {
            string connection = ConfigurationManager.ConnectionStrings["CurrencyConverter.Properties.Settings.tmamDBConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connection);
            sqlConnection.Open();
        }
        private void BindCurrency()
        {
            myConnection();
            string query = "select Id, CurrencyName from Currency_Master";
            sqlCommand=new SqlCommand(query,sqlConnection);
            sqlCommand.CommandType= CommandType.Text;
            sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            DataTable dtCurrency = new DataTable();
            sqlDataAdapter.Fill(dtCurrency);

            DataRow newRow = dtCurrency.NewRow();
            newRow["Id"] = 0;
            newRow["CurrencyName"] = "--SELECT--";
            dtCurrency.Rows.InsertAt(newRow,0);

            if (dtCurrency != null && dtCurrency.Rows.Count > 0) { 
                cmbFromCurrency.ItemsSource= dtCurrency.DefaultView;
                cmbToCurrency.ItemsSource= dtCurrency.DefaultView;
            }
            sqlConnection.Close();

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";  
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";    
            cmbToCurrency.SelectedIndex = 0;


        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue = 0;
            if (txtCurrency.Text==null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Pleaese Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                txtCurrency.Focus();
                return;
            }
            else if(cmbFromCurrency.SelectedValue==null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                cmbFromCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                cmbFromCurrency.Focus();
                return;
            }

            myConnection();
            string queryf = "Select Amount from Currency_Master where Id=@Id";
            sqlCommand =new SqlCommand(queryf,sqlConnection);
            sqlCommand.Parameters.AddWithValue("@id",int.Parse(cmbFromCurrency.SelectedValue.ToString()));
            object resultFrom = sqlCommand.ExecuteScalar();
            double amountFrom = 0;
            if (resultFrom != null && resultFrom != DBNull.Value)
            {
                amountFrom = (Double)resultFrom;
            }

            string queryt = "Select Amount from Currency_Master where Id=@Id";
            sqlCommand = new SqlCommand(queryf, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@id", int.Parse(cmbToCurrency.SelectedValue.ToString()));
            object resultTo = sqlCommand.ExecuteScalar();
            double amountTo = 0;
            if (resultTo != null && resultTo != DBNull.Value)
            {
                amountTo = (Double)resultTo;
            }

            sqlConnection.Close();

            if (cmbFromCurrency.SelectedValue==cmbToCurrency.SelectedValue)
            {
                ConvertedValue = double.Parse(txtCurrency.Text);

                lblCurrency.Content=cmbToCurrency.Text+" "+ConvertedValue.ToString("N3");
            }
            else
            {
               


                ConvertedValue = (double.Parse(txtCurrency.Text) * amountFrom) / amountTo;
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");


            }

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtCurrency.Text=String.Empty;
            if (cmbFromCurrency.Items.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;
            }
            if (cmbToCurrency.Items.Count > 0) { 
               cmbToCurrency.SelectedIndex = 0;
            }
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+");
            e.Handled = !regex.IsMatch(e.Text);


        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrency.Focus();
                    return;
                }

               

                if (CurrencyId>0)    //Upadte
                {
                    if (MessageBox.Show("Are you sure you want to update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        myConnection();
                        string query = "Update Currency_Master set Amount=@Amount ,CurrencyName=@CurrencyName " +
                            "where Id=@Id";
                        sqlCommand=new SqlCommand(query,sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@Id",CurrencyId);
                        sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                        sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                        sqlCommand.ExecuteNonQuery();

                        MessageBox.Show("Data Updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    }

                }
                else {                       //Save
                    if (MessageBox.Show("Are you sure you want to Save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        myConnection();
                        string query = "Insert into Currency_Master values(@Amount,@CurrencyName)";
                        sqlCommand = new SqlCommand(query, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                        sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                        sqlCommand.ExecuteNonQuery();

                        MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    ClearMaster();
                }
                

            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = String.Empty;
                txtCurrencyName.Text = String.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyId = 0;  ////
                BindCurrency();
                txtAmount.Focus();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void GetData()
        {
            try
            {
                myConnection();
                string query = "select * from Currency_Master";
                sqlCommand =new SqlCommand(query, sqlConnection);
                sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlDataAdapter.Fill(dt);

                if(dt!=null && dt.Rows.Count > 0)
                {
                    dgvCurrency.ItemsSource=dt.DefaultView;
                }
                else
                {
                   dgvCurrency.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           ClearMaster();
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid =(DataGrid) sender;
                DataRowView selectedRow = dataGrid.CurrentItem as DataRowView;
                if (selectedRow != null){
                    if (dataGrid.SelectedCells.Count > 0)
                    {
                        if (dataGrid.Items.Count > 0)
                        {
                            CurrencyId = int.Parse(selectedRow["Id"].ToString());
                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)  //edit 
                            {
                                txtAmount.Text = selectedRow["Amount"].ToString();
                                txtCurrencyName.Text = selectedRow["CurrencyName"].ToString();
                                btnSave.Content = "Update";
                               
                            }
                            else if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)  //save
                            {
                                if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == 
                                    MessageBoxResult.Yes)
                                    myConnection();
                                string query = "Delete from Currency_Master where  Id=@Id";
                                sqlCommand = new SqlCommand(query, sqlConnection);
                                sqlCommand.Parameters.AddWithValue("Id", CurrencyId);
                                sqlCommand.ExecuteNonQuery();

                                MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ClearMaster();
                            }
                        }
                    }
                }
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }

        }
    }
}
