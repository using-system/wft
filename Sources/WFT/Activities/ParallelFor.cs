using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Run in parallel activities repeatedly until a specified expression evaluates to false
    /// </summary>
    [Designer(typeof(ForDesigner))]
    [Description("Run in parallel activities repeatedly until a specified expression evaluates to false")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.ForToolbox.bmp")]
    [DesignerIcon("Resources/ForDesigner.bmp")]
    public class ParallelFor : BaseForActivity
    {
        protected override void ScheduleBody(IEnumerator<int> values, NativeActivityContext context)
        {
            while (values.MoveNext())
                context.ScheduleAction<int>(Body, values.Current);
        }
    }
}
