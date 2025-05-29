using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Helpers
{
    public static class ShimmerHelper
    {
        public static Visibility ReverseVisibility(Visibility visibility)
        {
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

