using MdXaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FreezerGUI.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy LicenceWindow.xaml
    /// </summary>
    public partial class LicenceWindow : Window
    {
        public LicenceWindow()
        {
            InitializeComponent();
            this.FontSize = BootStrapper.Settings.GlobalFontSize;
            Loaded += LicenceWindow_Loaded;
        }

        private void LicenceWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //Markdown engine = new Markdown();
            // //string markdownTxt = System.IO.File.ReadAllText("example.md");
            //FlowDocument document = engine.Transform(Licence);
            //this.LicenceDocument.Document = document;
            MarkdownView.Markdown = Licence;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private const string Licence = @"
# Freezer Program License

**Copyright (c)**

**2022 Jan Sawicki**

[https://github.com/JTSawicki](https://github.com/JTSawicki)

All Rights Reserved

Unauthorized use or distribution, via any medium prohibited.

Proprietaly and confidential.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Written by Jan Sawicki, May 2023

## **Resources Copyright Notice (c)**

2022.07.28 Icons created by Anggara - Flaticon([https://www.flaticon.com](https://www.flaticon.com)): info

2022.07.28 Icons created by Freepik - Flaticon([https://www.flaticon.com](https://www.flaticon.com)): error, check-mark

2022.07.28 Icons created by Gregor Cresnar - Flaticon([https://www.flaticon.com](https://www.flaticon.com)): warning

## **Libraries Copyright Notice (c)**

**.Net and WPF**

Copyright (c) .NET Foundation and Contributors

<https://github.com/dotnet>

**CommunityToolkit.Mvvm**

Copyright (c) .NET Foundation and Contributors

<https://github.com/CommunityToolkit/dotnet>

**Serilog**

<https://github.com/serilog/serilog>

**Serilog.Exceptions**

Copyright (c) 2015 Muhammad Rehan Saeed

<https://github.com/RehanSaeed/Serilog.Exceptions>

**Oxyplot**

Copyright (c) 2014 OxyPlot contributors

<https://github.com/oxyplot/oxyplot>

**MaterialDesignInXamlToolkit**

Copyright (c) James Willock,  Mulholland Software and Contributors

<https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit>

**AvalonEdit**

Copyright (c) AvalonEdit Contributors

<https://github.com/icsharpcode/AvalonEdit>

**HelixToolkit**

Copyright (c) 2022 Helix Toolkit contributors

<https://github.com/helix-toolkit/helix-toolkit>

**PixiEditor.ColorPicker**

Copyright (c) PixiEditor Organization

<https://github.com/PixiEditor/ColorPicker>

**MdXaml**

Copyright (c) 2020 Bevan Arps, Whistyun

<https://github.com/whistyun/MdXaml>
";
    }
}
