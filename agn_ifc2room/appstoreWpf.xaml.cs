// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using Autodesk.Revit.DB;
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
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewFam = viewFamTypeList[comboViewFam.SelectedItem.ToString()];
            this.DialogResult = true;
            this.Close();
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
    }
}
