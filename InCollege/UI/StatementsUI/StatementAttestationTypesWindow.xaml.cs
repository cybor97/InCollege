﻿using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class StatementAttestationTypesWindow : Window, IUpdatable
    {
        public Statement Statement { get; set; }
        public StatementAttestationTypesWindow(Statement statement)
        {
            InitializeComponent();
            Statement = statement;
        }

        public async Task UpdateData()
        {
            var attestationTypesData = await NetworkUtils.RequestData<AttestationType>(this);

            var statementAttestationTypesData = await NetworkUtils.RequestData<StatementAttestationType>(this, (nameof(StatementAttestationType.StatementID), Statement?.ID ?? -1));
            foreach (var current in statementAttestationTypesData)
                current.AttestationType = attestationTypesData.FirstOrDefault(c => c.ID == current.AttestationTypeID);
            StatementAttestationTypesLV.ItemsSource = statementAttestationTypesData;

            AttestationTypeCB.ItemsSource = attestationTypesData.Where(currentAttestationType => !(statementAttestationTypesData
                                                                .Any(c => c.AttestationTypeID == currentAttestationType.ID)));
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AttestationTypeCB.Items.IsEmpty)
            {
                StatementAttestationTypeDialog.DataContext = new StatementAttestationType { StatementID = Statement?.ID ?? -1 };
                StatementAttestationTypeDialog.IsOpen = true;
            }
        }

        void StatementAttestationTypeDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (StatementAttestationTypeDialog.IsOpen)
                if (e.Key == Key.Enter)
                {
                    StatementAttestationTypeDialog.Focus();
                    SaveButton_Click(null, null);
                }
                else if (e.Key == Key.Escape)
                    CancelButton_Click(null, null);
        }

        async void StatementAttestationTypesWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                case Key.OemPlus:
                    AddButton_Click(null, null);
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                case Key.Delete:
                    RemoveItem_Click(null, null);
                    break;
                case Key.F5:
                    await UpdateData();
                    break;
            }
        }

        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AttestationTypeCB.SelectedItem != null)
                await NetworkUtils.ExecuteDataAction<StatementAttestationType>(this, (DBRecord)StatementAttestationTypeDialog.DataContext, DataAction.Save);
            StatementAttestationTypeDialog.IsOpen = false;
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            StatementAttestationTypeDialog.IsOpen = false;
        }

        void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (StatementAttestationTypesLV.SelectedItem != null)
            {
                StatementAttestationTypeDialog.DataContext = StatementAttestationTypesLV.SelectedItem;
                StatementAttestationTypeDialog.IsOpen = true;
            }
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var current in StatementAttestationTypesLV.SelectedItems)
                await NetworkUtils.ExecuteDataAction<StatementAttestationType>(null, (DBRecord)current, DataAction.Remove);
            await UpdateData();
        }

        async void StatementAttestationTypesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }
    }
}
