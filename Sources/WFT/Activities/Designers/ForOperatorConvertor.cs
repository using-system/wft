using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WFT.Activities.Designers
{
    public class ForOperatorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is BaseForActivity.Operator))
                return value;

            BaseForActivity.Operator forOp = (BaseForActivity.Operator)value ;

            switch(forOp)
            {
                case BaseForActivity.Operator.IsGreaterThan:
                    return ">";
                case BaseForActivity.Operator.IsGreaterThanOrEqualTo:
                    return ">=";
                case BaseForActivity.Operator.IsLessThan:
                    return "<";
                case BaseForActivity.Operator.IsLessThanOrEqualTo:
                    return "<=";
                default:
                    return forOp;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
