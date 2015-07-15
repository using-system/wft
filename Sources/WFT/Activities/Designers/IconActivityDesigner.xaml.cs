using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace WFT.Activities.Designers
{
    /// <summary>
    /// Activity Designer display a icon
    /// <remarks>
    /// DesignerIconAttribute must be specified in the activity associated to this designer
    /// </remarks>
    /// </summary>
    public partial class IconActivityDesigner
    {
        public IconActivityDesigner()
        {
            InitializeComponent();
            this.Collapsible = false;        
        }

        private void Designer_Loaded(object sender, RoutedEventArgs e)
        {
            Type activityType = ModelItem.ItemType;

            DesignerIconAttribute iconAttr = activityType.GetCustomAttribute<DesignerIconAttribute>();
            if(iconAttr != null)
                this.Icon = new DrawingBrush(this.GetType().GetImageDrawingFromResource(iconAttr.ResourceName));
        }
    }
}
