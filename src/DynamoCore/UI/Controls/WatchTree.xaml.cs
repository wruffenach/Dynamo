﻿using System.Windows;
using System.Windows.Controls;
using Dynamo.UI.Views;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    //http://blogs.msdn.com/b/chkoenig/archive/2008/05/24/hierarchical-databinding-in-wpf.aspx

    /// <summary>
    /// Interaction logic for WatchTree.xaml
    /// </summary>
    public partial class WatchTree : UserControl, ISpecificVersionComponent
    {
        public WatchTree()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //find the element which was clicked
            //and implement it's method for jumping to stuff
            var fe = sender as FrameworkElement;

            if (fe == null)
                return;

            var node = (WatchNode)fe.DataContext;

            if (node != null)
                node.Click();
        }

        public void LoadSpecificVersionComponent()
        {
            _contentLoaded = true;
            SpecificVersionLoader.LoadSpecificVersionUserControl(this);
        }
    }
}