﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Nodes.PackageManager;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishUI.xaml
    /// </summary>
    public partial class PackageManagerPublishUI : UserControl
    {

        public PackageManagerPublishUI(PackageManagerPublishViewModel viewModel)
        {

            InitializeComponent();
            this.DataContext = viewModel;

        }

    }
}