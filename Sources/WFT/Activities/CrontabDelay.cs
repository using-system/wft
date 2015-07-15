using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Delays execution for a specified crontab expression
    ///
    /// * * * * *
    /// - - - - -
    /// | | | | |
    /// | | | | +_____ day of week (0 - 6) (Sunday=0)
    /// | | | +_______ month (1 - 12)
    /// | | +_________ day of month (1 - 31)
    /// | +____________hour (0 - 23)
    /// +_____________ min (0 - 59)    
    ///  Exemple of Cron Expression : 
    ///  1) ==> "* 1-3 * * *"
    ///  2) ==> "* * * 1,3,5,7,9,11 *"
    ///  3) ==> "10,25,40 * * * *"
    ///  4) ==> "* * * 1,3,8 1-2,5"
    /// </summary>
    [Description("Delays execution for a specified crontab expression")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.CronDelayToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/CronDelayDesigner.bmp")]
    public class CrontabDelay : NativeActivity
    {
        public CrontabDelay()
        {
            CronExpression = "* * * * *";
            StartDate = new InArgument<DateTime>();
            MinimumDelay = new InArgument<TimeSpan>();

            _body = new Delay()
            {
                Duration = _delayDuration
            };
        }

        /// <summary>
        /// Specify the Crontab expression
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Specify the Crontab expression")]
        public InArgument<string> CronExpression { get; set; }

        /// <summary>
        /// Base time
        /// </summary>
        [Description("Base time")]
        public InArgument<DateTime> StartDate { get; set; }

        /// <summary>
        /// Minimum delay to wait
        /// </summary>
        [Description("Minimum delay to wait")]
        public InArgument<TimeSpan> MinimumDelay { get; set; }

        private Activity _body = null;

        private Variable<TimeSpan> _delayDuration = new Variable<TimeSpan>();



        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            metadata.AddImplementationVariable(_delayDuration);
            metadata.AddImplementationChild(_body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            DateTime startDate = StartDate.Get(context);
            if (startDate == null
                || startDate == DateTime.MinValue)
                startDate = DateTime.Now;

            DateTime nextOccurrence = CronExpression.Get(context).GetNextCronOccurrence(startDate).Value;
            TimeSpan delay = (TimeSpan)(nextOccurrence - startDate);

            TimeSpan minimumDelay = MinimumDelay.Get(context);

            if (minimumDelay != null
                && delay.TotalSeconds < minimumDelay.TotalSeconds)
                delay = minimumDelay;

            _delayDuration.Set(context, delay);
            context.ScheduleActivity(_body);
        }
    }
}
