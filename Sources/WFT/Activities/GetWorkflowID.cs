using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Get the Workflow Instance ID
    /// </summary>
    [Description("Get the Workflow Instance ID")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.GetWorkflowIDToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/GetWorkflowIDDesigner.bmp")]
    public sealed class GetWorkflowID : CodeActivity<Guid>
    {
        protected override Guid Execute(CodeActivityContext context)
        {
            return context.WorkflowInstanceId;
        }
    }
}
