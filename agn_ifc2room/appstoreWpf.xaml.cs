// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace agn.ifc2revitRooms
{

    public partial class appstoreWpf : Window
    {
        public string filename = "";
        public ViewFamilyType viewFam = null;
        private Dictionary<string, ViewFamilyType> viewFamTypeList;

        public appstoreWpf(Document doc)
        {
            InitializeComponent();

            logo.Source = Ribbon.convertFromBitmap(agn.ifc2revitRooms.Properties.Resources.wpf_logo);
            logo2.Source = Ribbon.convertFromBitmap(agn.ifc2revitRooms.Properties.Resources.wpf_provided);

            FilteredElementCollector viewPlanCollector = new FilteredElementCollector(doc);
            IList<Element> viewPlans = viewPlanCollector.OfClass(typeof(ViewFamilyType)).ToElements();

            viewFamTypeList = new Dictionary<string, ViewFamilyType>();

            foreach (Element ele in viewPlans)
            {
                viewFam = ele as ViewFamilyType;
                if (viewFam.ViewFamily == ViewFamily.FloorPlan)
                {
                    viewFamTypeList.Add(viewFam.Name, viewFam);
                }
            }

            comboViewFam.ItemsSource = viewFamTypeList.Keys;

            while (comboViewFam.SelectedItem != null & System.IO.File.Exists(agnWpfPath.Text))
            {
                
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboViewFam.SelectedItem != null & System.IO.File.Exists(agnWpfPath.Text) & System.IO.Path.GetExtension(agnWpfPath.Text) == ".ifc")
                {
                    viewFam = viewFamTypeList[comboViewFam.SelectedItem.ToString()];
                    filename = agnWpfPath.Text;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    FailureWindow fw = new FailureWindow();
                    fw.ShowDialog();
                }
            }
            catch (Exception exc)
            {
                FailureWindow fw = new FailureWindow();
                fw.ErrorMessage.Content = exc;
                fw.ShowDialog();
            }
        }

        private void agnWpfPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "IFC-Models (*.ifc)|*.ifc";
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
            }

            agnWpfPath.Text = filename;
        }

        private void ifcFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "IFC-Models (*.ifc)|*.ifc";
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
            }

            agnWpfPath.Text = filename;
        }
    }
}
