using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    ///  Run activities (sequential) repeatedly until a specified expression evaluates to false
    /// </summary>
    [Designer(typeof(ForDesigner))]
    [Description("Run activities (sequential) repeatedly until a specified expression evaluates to false")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.ForToolbox.bmp")]
    [DesignerIcon("Resources/ForDesigner.bmp")]
    public class For : BaseForActivity
    {
        private IEnumerator<int> _values;

        protected override void ScheduleBody(IEnumerator<int> values, NativeActivityContext context)
        {
            _values = values;
            
            if (!_values.MoveNext())
                return;

            ScheduleBody(_values.Current, context);
        }

        private void ScheduleBody(int value, NativeActivityContext context)
        {
            context.ScheduleAction<int>(Body, value, ScheduleBodyComplete);
        }

        private void ScheduleBodyComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (!_values.MoveNext())
                return;
            ScheduleBody(_values.Current, context);
        }
    }
}
