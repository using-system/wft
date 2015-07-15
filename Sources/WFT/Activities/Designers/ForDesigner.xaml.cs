using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WFT.Activities.Designers
{
    // Interaction logic for ForDesigner.xaml
    public partial class ForDesigner
    {
        public ForDesigner()
        {
            InitializeComponent();
        }

        private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
        {
            Type activityType = ModelItem.ItemType;

            DesignerIconAttribute iconAttr = activityType.GetCustomAttribute<DesignerIconAttribute>();
            if (iconAttr != null)
                this.Icon = new DrawingBrush(this.GetType().GetImageDrawingFromResource(iconAttr.ResourceName));
        }
    }
}
