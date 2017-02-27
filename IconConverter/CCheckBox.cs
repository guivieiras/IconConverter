using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IconConverter {
    public class CCheckBox : CheckBox{
        int dimension;

        public int Dimension {
            get {
                return dimension;
            }

            set {
                dimension = value;
            }
        }
    }
}
