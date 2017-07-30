using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
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
using System.IO.Packaging;


namespace liga
{
	/// Logika interakcji dla klasy MainWindow.xaml
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			InitBinding();
		}
		//do wyboru czy dodajemy czy edytujemy gracza
		bool addEdit = true;
		private SQLiteDataAdapter m_oDataAdapter = null;
		private SQLiteDataAdapter m_oDataAdapter2 = null;
		private SQLiteDataAdapter m_oDataAdapter3 = null;
		private DataSet m_oDataSet = null;
		private DataSet m_oDataSet2 = null;
		private DataSet m_oDataSet3 = null;
		private DataTable m_oDataTable = null;
		private DataTable m_oDataTable2 = null;
		private DataTable m_oDataTable3 = null;

		private void InitBinding()
		{
			SQLiteConnection oSQLiteConnection = new SQLiteConnection("Data Source=liga.db");
			SQLiteCommand oCommand = oSQLiteConnection.CreateCommand();
			SQLiteCommand oCommand2 = oSQLiteConnection.CreateCommand();

			oCommand.CommandText = "SELECT * FROM Person";
			oCommand2.CommandText = "SELECT *, sum(Point + (CASE WHEN Transfers < '1' THEN '0' ELSE(Transfers - 1) * (-4) END)) AS \"LOL\" " +
				"FROM Points, Person WHERE Points.PersonID = Person.PersonID GROUP BY Name ORDER BY \"LOL\" DESC";
			
			m_oDataAdapter = new SQLiteDataAdapter(oCommand.CommandText, oSQLiteConnection);
			m_oDataAdapter2 = new SQLiteDataAdapter(oCommand2.CommandText, oSQLiteConnection);
			
			SQLiteCommandBuilder oCommandBuilder = new SQLiteCommandBuilder(m_oDataAdapter);
			SQLiteCommandBuilder oCommandBuilder2 = new SQLiteCommandBuilder(m_oDataAdapter2);
			
			m_oDataSet = new DataSet();
			m_oDataSet2 = new DataSet();
			
			m_oDataAdapter.Fill(m_oDataSet);
			m_oDataAdapter2.Fill(m_oDataSet2);
			
			m_oDataTable = m_oDataSet.Tables[0];
			m_oDataTable2 = m_oDataSet2.Tables[0];
			
			lstPlayer.DataContext = m_oDataTable.DefaultView;
			lstGeneral.DataContext = m_oDataTable2.DefaultView;
			
		}

		private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
		{
		}
		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			if (btnAdd.Content.ToString() == "Add")
			{
				Unlock();
				addEdit = true;
				btnDelete.IsEnabled = false;
				btnEdit.IsEnabled = false;
				string Cancel = "Cnc";
				btnAdd.Content = Cancel;
			}
			else
			{
				ClearTextBox();
				Lock();
				string Add = "Add";
				btnAdd.Content = Add;
				btnDelete.IsEnabled = true;
				btnEdit.IsEnabled = true;
			}
		}
		private void btnEdit_Click(object sender, RoutedEventArgs e)
		{
			if (0 == lstPlayer.SelectedItems.Count)
			{
				return;
			}
			if (btnEdit.Content.ToString() == "Edit")
			{
				Unlock();
				btnDelete.IsEnabled = false;
				btnAdd.IsEnabled = false;
				addEdit = false;
				DataRow oDataRow = ((DataRowView)lstPlayer.SelectedItem).Row;				
				Name.Text = oDataRow[1].ToString();
				Surname.Text = oDataRow[2].ToString();
				Team.Text = oDataRow[3].ToString();
				string Cancel = "Cnc";
				btnEdit.Content = Cancel;
			}
			else
			{
				ClearTextBox();
				Lock();
				string Edit = "Edit";
				btnEdit.Content = Edit;
				btnDelete.IsEnabled = true;
				btnAdd.IsEnabled = true;
			}
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (0 == lstPlayer.SelectedItems.Count)
			{
				return;
			}
			DataRow oDataRow = ((DataRowView)lstPlayer.SelectedItem).Row;
			oDataRow.Delete();
			m_oDataAdapter.Update(m_oDataSet);
		}

		private void btnAccept_Click(object sender, RoutedEventArgs e)
		{
			if (Name.Text.ToString() == "" || Surname.Text.ToString() == "" || Team.Text.ToString() == "")
			{
				MessageBox.Show("You have not completed all the data.");
				return;
			}
			else
			{
				if (addEdit)
				{
					DataRow oDataRow = m_oDataTable.NewRow();
					oDataRow[1] = Name.Text.ToString();
					oDataRow[2] = Surname.Text.ToString();
					oDataRow[3] = Team.Text.ToString();
					m_oDataTable.Rows.Add(oDataRow);
					m_oDataAdapter.Update(m_oDataSet);
					ClearTextBox();
					string Add = "Add";
					btnAdd.Content = Add;
					btnDelete.IsEnabled = true;
					btnEdit.IsEnabled = true;
					MessageBox.Show("You have created a new Player!");
				}
				else
				{
					DataRow oDataRow = ((DataRowView)lstPlayer.SelectedItem).Row;
					oDataRow.BeginEdit();
					oDataRow[1] = Name.Text.ToString();
					oDataRow[2] = Surname.Text.ToString();
					oDataRow[3] = Team.Text.ToString();
					oDataRow.EndEdit();
					m_oDataAdapter.Update(m_oDataSet);
					ClearTextBox();
					btnDelete.IsEnabled = false;
					btnAdd.IsEnabled = false;
					string Edit = "Edit";
					btnEdit.Content = Edit;
					btnDelete.IsEnabled = true;
					btnAdd.IsEnabled = true;
					MessageBox.Show("You changed the Player's data.");
				}
			}
			Lock();
			InitBinding();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (null != m_oDataAdapter)
			{
				m_oDataAdapter.Dispose();
				m_oDataAdapter2.Dispose();
				m_oDataAdapter = null;
				m_oDataAdapter2 = null;
				m_oDataAdapter3.Dispose();
				m_oDataAdapter3 = null;
			}
		}
		//czyści textboxy do edycji graczy
		private void ClearTextBox()
		{
			Name.Text = null;
			Surname.Text = null;
			Team.Text = null;
		}

		//odblokowuje boxy do edycji lisy graczy
		private void Unlock()
		{
			Name.IsEnabled = true;
			Surname.IsEnabled = true;
			Team.IsEnabled = true;
			btnAccept.IsEnabled = true;
			btnShow2Points.IsEnabled = false;
		}

		//blokuje boxy do edycji lisy graczy
		private void Lock()
		{
			Name.IsEnabled = false;
			Surname.IsEnabled = false;
			Team.IsEnabled = false;
			btnAccept.IsEnabled = false;
			btnShow2Points.IsEnabled = true;
		}

		private void lstPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}

		//////////////////////////////////////////////////////////////////////////////////////////////
		////Dodawanie punktów/////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////

		//Blokowanie zakładek poza points
		private void PointsOption()
		{
			Tab.SelectedItem = pointsT; //wybieranie zakladki
		
			general.IsEnabled = false;
			list.IsEnabled = false;
			pointsT.IsEnabled = true;
		}

		int ID;

		private void btnShowPoints_Click(object sender, RoutedEventArgs e)
		{
			if (0 == lstGeneral.SelectedItems.Count)
			{
				return;
			}
			DataRow oDataRow2 = ((DataRowView)lstGeneral.SelectedItem).Row;
			int id = Convert.ToInt32(oDataRow2[1]);
			string Name = Convert.ToString(oDataRow2[6]);
			string Surname = Convert.ToString(oDataRow2[7]);
			string Team = Convert.ToString(oDataRow2[8]);
			string SumPo = Convert.ToString(oDataRow2[9]);

			ID = id;
			PointsOption();
			InitPoints();
			NameP.Text = Name;
			SurnameP.Text = Surname;
			TeamP.Text = Team;
			SumP.Text = SumPo;
		}
		private void btnShowPoints2_Click(object sender, RoutedEventArgs e)
		{
			
			if (0 == lstPlayer.SelectedItems.Count)
			{
				return;
			}
			DataRow oDataRow = ((DataRowView)lstPlayer.SelectedItem).Row;
			int id = Convert.ToInt32(oDataRow[0]);
			string imie = Convert.ToString(oDataRow[1]);
			string nazwisko = Convert.ToString(oDataRow[2]);
			string druzyna = Convert.ToString(oDataRow[3]);

			ID = id;
			PointsOption();
			InitPoints();
			NameP.Text = imie;
			SurnameP.Text = nazwisko;
			TeamP.Text = druzyna;
		}
		private void btnBack_Click(object sender, RoutedEventArgs e)
		{
			Tab.SelectedItem = general; //wybieranie zakladki
			general.IsEnabled = true;
			list.IsEnabled = true;
			pointsT.IsEnabled = false;
			NameP.Text = "---";
			SurnameP.Text = "---";
			TeamP.Text = "---";
			InitBinding();
			LockP();
		}

		private void InitPoints()
		{
			SQLiteConnection oSQLiteConnection = new SQLiteConnection("Data Source=liga.db");
			SQLiteCommand oCommand3 = oSQLiteConnection.CreateCommand();
			oCommand3.CommandText = "SELECT *, CASE WHEN Transfers < '1' THEN '0' ELSE (Transfers-1)*(-4) END AS \"Deduct\", " +
			"(Point + (CASE WHEN Transfers < '1' THEN '0' ELSE (Transfers-1)*(-4) END)) AS \"SUM\" FROM Points WHERE PersonID = " + ID + " ORDER BY \"GameweekNr\" ASC";
			m_oDataAdapter3 = new SQLiteDataAdapter(oCommand3.CommandText, oSQLiteConnection);
			SQLiteCommandBuilder oCommandBuilder3 = new SQLiteCommandBuilder(m_oDataAdapter3);
			m_oDataSet3 = new DataSet();
			m_oDataAdapter3.Fill(m_oDataSet3);
			m_oDataTable3 = m_oDataSet3.Tables[0];
			lstPoints.DataContext = m_oDataTable3.DefaultView;
		}
	
		private void btnAddP_Click(object sender, RoutedEventArgs e)
		{
			if (btnAddP.Content.ToString() == "Add Points")
			{
				UnlockP();
				addEdit = true;
				btnDeleteP.IsEnabled = false;
				btnEditP.IsEnabled = false;
				string Cancel = "  Cancel  ";
				btnAddP.Content = Cancel;
			}
			else
			{
				ClearTextBoxP();
				LockP();
				string Add = "Add Points";
				btnAddP.Content = Add;
				btnDeleteP.IsEnabled = true;
				btnEditP.IsEnabled = true;
			}
		}
		private void btnEditP_Click(object sender, RoutedEventArgs e)
		{
			if (0 == lstPoints.SelectedItems.Count)
			{
				return;
			}
			if (btnEditP.Content.ToString() == "Edit Points")
			{
				UnlockP();
				btnDeleteP.IsEnabled = false;
				btnAddP.IsEnabled = false;
				addEdit = false;
				DataRow oDataRow = ((DataRowView)lstPoints.SelectedItem).Row;
				
				Gameweek.Text = oDataRow[2].ToString();
				Points.Text = oDataRow[3].ToString();
				Transfers.Text = oDataRow[4].ToString();

				string Cancel = "  Cancel   ";
				btnEditP.Content = Cancel;
			}
			else
			{
				ClearTextBoxP();
				LockP();
				string Edit = "Edit Points";
				btnEditP.Content = Edit;
				btnDeleteP.IsEnabled = true;
				btnAddP.IsEnabled = true;
			}
		}

		private void btnDeleteP_Click(object sender, RoutedEventArgs e)
		{
			if (0 == lstPoints.SelectedItems.Count)
			{
				return;
			}
			DataRow oDataRow = ((DataRowView)lstPoints.SelectedItem).Row;
			oDataRow.Delete();
			m_oDataAdapter3.Update(m_oDataSet3);
			MessageBox.Show("You removed the selected points.");
		}

		private void btnAcceptP_Click(object sender, RoutedEventArgs e)
		{

			if (addEdit)
			{
				DataRow oDataRow = m_oDataTable3.NewRow();
				try
				{
					int gameweek = int.Parse(Gameweek.Text.ToString());
					int points = int.Parse(Points.Text.ToString());
					int transfers = int.Parse(Transfers.Text.ToString());
				}
				catch (FormatException) { MessageBox.Show("Bad Format."); return; }
				catch (OverflowException) { MessageBox.Show("Overflow."); return; }

				if (int.Parse(Gameweek.Text.ToString()) < 0 || int.Parse(Gameweek.Text.ToString()) > 40 || int.Parse(Points.Text.ToString()) < 0 || int.Parse(Points.Text.ToString()) > 200 || int.Parse(Transfers.Text.ToString()) < 0 || int.Parse(Transfers.Text.ToString()) > 10)
				{
					MessageBox.Show("You have not completed all the data, or the data are not correct.");
					return;
				}
				else
				{
					oDataRow[1] = Convert.ToInt32(ID);
					oDataRow[2] = Convert.ToInt32(Gameweek.Text.ToString());
					oDataRow[3] = Convert.ToInt32(Points.Text.ToString());
					oDataRow[4] = Convert.ToInt32(Transfers.Text.ToString());

					m_oDataTable3.Rows.Add(oDataRow);
					m_oDataAdapter3.Update(m_oDataSet3);

					ClearTextBoxP();
					string Add = "Add Points";
					btnAddP.Content = Add;
					btnDeleteP.IsEnabled = true;
					btnEditP.IsEnabled = true;
					btnAcceptP.IsEnabled = false;
					InitPoints();
					MessageBox.Show("You added points.");
				}
			}
			else
			{
				DataRow oDataRow = ((DataRowView)lstPoints.SelectedItem).Row;
				try
				{
					int gameweek = int.Parse(Gameweek.Text.ToString());
					int points = int.Parse(Points.Text.ToString());
					int transfers = int.Parse(Transfers.Text.ToString());
				}
				catch (FormatException) { MessageBox.Show("Bad Format."); return; }
				catch (OverflowException) { MessageBox.Show("Overflow."); return; }

				if (int.Parse(Gameweek.Text.ToString()) < 0 || int.Parse(Gameweek.Text.ToString()) > 40 || int.Parse(Points.Text.ToString()) < 0 || int.Parse(Points.Text.ToString()) > 200 || int.Parse(Transfers.Text.ToString()) < 0 || int.Parse(Transfers.Text.ToString()) > 10)
				{
					MessageBox.Show("You have not completed all the data, or the data are not correct.");
					return;
				}
				else
				{
					oDataRow.BeginEdit();
					oDataRow[1] = Convert.ToInt32(ID);
					oDataRow[2] = Convert.ToInt32(Gameweek.Text.ToString());
					oDataRow[3] = Convert.ToInt32(Points.Text.ToString());
					oDataRow[4] = Convert.ToInt32(Transfers.Text.ToString());
					oDataRow.EndEdit();

					m_oDataAdapter3.Update(m_oDataSet3);

					ClearTextBoxP();
					btnDeleteP.IsEnabled = false;
					btnAddP.IsEnabled = false;
					string Edit = "Edit Points";
					btnEditP.Content = Edit;
					btnDeleteP.IsEnabled = true;
					btnAddP.IsEnabled = true;
					MessageBox.Show("You changed score.");
				}
			}
			LockP();
		}

		private void ClearTextBoxP()
		{
			Points.Text = null;
			Gameweek.Text = null;
			Transfers.Text = null;
		}

		//odblokowuje boxy do edycji listy punktów
		private void UnlockP()
		{
			Points.IsEnabled = true;
			Gameweek.IsEnabled = true;
			Transfers.IsEnabled = true;
			btnAcceptP.IsEnabled = true;
		}

		//blokuje boxy do edycji lisy punktów
		private void LockP()
		{
			Points.IsEnabled = false;
			Gameweek.IsEnabled = false;
			Transfers.IsEnabled = false;
			btnAcceptP.IsEnabled = false;
		}

		private void lstPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
		private void PrintBtn_Click(object sender, RoutedEventArgs e)
		{
			PrintDialog printDialog = new PrintDialog();
			if (printDialog.ShowDialog() == true)
			{
				printDialog.PrintVisual(lstGeneral, "General");
			}
		}
	}
}
