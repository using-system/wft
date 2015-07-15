using System;
using System.Reflection;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using WFT.Activities.Net;

namespace WFT.Activities.Designers
{
    // Interaction logic for SendMailDesigner.xaml
    public partial class SendMailDesigner
    {
        public SendMailDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(SendMail);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(SendMailDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("To"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("From"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Subject"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Host"), BrowsableAttribute.No);
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
