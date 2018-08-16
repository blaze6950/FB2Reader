using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Win32;

namespace FB2Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String _fileName;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                FileName = "",
                DefaultExt = ".txt",
                Filter = "Text documents (.txt, .fb2, .xml)|*.txt;*.fb2;*.xml"
            };

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                _fileName = dlg.FileName;
                TextBox.Text = _fileName;
                LoadDocument();
            }
        }

        private void LoadDocument()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_fileName);

            var book = doc.DocumentElement;

            if (book != null && book.HasChildNodes)
            {
                FlowDocument newDocument = new FlowDocument();


                Run run = new Run(book["Header"].InnerText);
                Paragraph header = new Paragraph();
                header.Inlines.Add(run);
                header.FontSize = 40;
                header.TextAlignment = TextAlignment.Center;
                newDocument.Blocks.Add(header);

                var hasChildNodes = book["Content"].HasChildNodes;
                if (hasChildNodes != null && (bool) hasChildNodes)
                {
                    run = new Run("Содержание");
                    header = new Paragraph();
                    header.Inlines.Add(run);
                    header.FontSize = 20;
                    header.TextAlignment = TextAlignment.Center;
                    newDocument.Blocks.Add(header);
                    var content = book["Content"].ChildNodes;
                    List contentList = new List();
                    contentList.MarkerStyle = TextMarkerStyle.Decimal;
                    contentList.MarkerOffset = 8;
                    ListItem contentListItem;
                    foreach (var contentItem in content)
                    {
                        run = new Run(((XmlNode)contentItem).InnerText);
                        header = new Paragraph();
                        header.Inlines.Add(run);
                        header.FontSize = 14;
                        contentListItem = new ListItem(header);
                        contentList.ListItems.Add(contentListItem);
                    }
                    newDocument.Blocks.Add(contentList);
                }

                var contentSourceBool = book["ContentSource"].HasChildNodes;
                if (contentSourceBool != null && (bool)contentSourceBool)
                {
                    var contentSource = book["ContentSource"].ChildNodes;
                    foreach (var contentItemSource in contentSource)
                    {
                        run = new Run(((XmlNode)contentItemSource)["HeaderContentItem"].InnerText);
                        header = new Paragraph();
                        header.Inlines.Add(run);
                        header.FontSize = 20;
                        header.TextAlignment = TextAlignment.Center;
                        newDocument.Blocks.Add(header);

                        var source = ((XmlNode) contentItemSource)["Source"].ChildNodes;
                        foreach (var sourceContent in source)
                        {
                            if (((XmlNode)sourceContent).Name == "p")
                            {
                                run = new Run(((XmlNode)sourceContent).InnerText);
                                header = new Paragraph();
                                header.Inlines.Add(run);
                                header.FontSize = 12;
                                header.TextAlignment = TextAlignment.Justify;
                                newDocument.Blocks.Add(header);
                            }
                            else if (((XmlNode)sourceContent).Name == "image")
                            {
                                BitmapImage bi = new BitmapImage();
                                bi.BeginInit();
                                bi.UriSource = new Uri(((XmlNode)sourceContent).InnerText, UriKind.Absolute);
                                bi.EndInit();
                                Image image = new Image();
                                image.Source = bi;
                                BlockUIContainer blockUiContainer = new BlockUIContainer(image);
                                Figure figure = new Figure(blockUiContainer);
                                figure.HorizontalAnchor = FigureHorizontalAnchor.PageCenter;
                                //figure.Height = new FigureLength(image.Height);
                                //figure.Width = new FigureLength(image.Width);
                                header = new Paragraph();
                                header.Inlines.Add(figure);
                                newDocument.Blocks.Add(header);
                            }
                        }
                    }
                }
                docViewer.Document = newDocument;
            }
        }
    }
}
