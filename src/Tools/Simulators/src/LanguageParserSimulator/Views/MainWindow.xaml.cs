using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Luna.Compilers.Simulators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Win32;

namespace Luna.Compilers.Tools;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog()
        {
            CheckFileExists = true,
            CheckPathExists = true,
            Filter = "Lua 文件 (*.lua)|*.lua|MoonScript 文件 (*.moon)|*.moon|所有文件(*.*)|*.*",
            FilterIndex = 3,
            Multiselect = false,
            ShowReadOnly = false,
            Title = "打开文件",
            ValidateNames = true
        };
        if (dialog.ShowDialog() == true)
        {
            if (Simulators.Simulator.TryGetLanguageParserSimulatorByFileExtension(Path.GetExtension(dialog.FileName), out var languageParserSimulators))
            {
                var simulator = languageParserSimulators[0];
                var sourceText = SourceText.From(dialog.OpenFile());
                var tree = simulator.ParseSyntaxTree(sourceText);
                this.FillinSyntaxTree(simulator, tree);
            }
        }
    }

    private void FillinSyntaxTree(ILanguageParserSimulator simulator, SyntaxTree tree)
    {
        this.treeView.Items.Clear();
        this.treeView.Items.Add(ProcessNodeOrToken(tree.GetRoot()));

        TreeViewItem ProcessNodeOrToken(SyntaxNodeOrToken nodeOrToken)
        {
            var item = new TreeViewItem
            {
                Header = $"{simulator.GetKindText(nodeOrToken.RawKind)} {nodeOrToken.FullSpan}",
                Foreground = nodeOrToken.IsNode ? Brushes.DarkBlue : Brushes.DarkGreen
            };

            if (nodeOrToken.IsToken)
            {
                var token = (SyntaxToken)nodeOrToken;
                foreach (var trivia in token.LeadingTrivia)
                    item.Items.Add(ProcessTrivia(trivia, trailing: false));
                foreach (var trivia in token.TrailingTrivia)
                    item.Items.Add(ProcessTrivia(trivia, trailing: true));
            }
            else if (nodeOrToken.IsNode)
            {
                foreach (var child in ((SyntaxNode)nodeOrToken)!.ChildNodesAndTokens())
                    item.Items.Add(ProcessNodeOrToken(child));
            }
            return item;
        }
        TreeViewItem ProcessTrivia(SyntaxTrivia trivia, bool trailing)
        {
            var item = new TreeViewItem
            {
                Header = $"{(trailing ? "Trail" : "Lead")}: {simulator.GetKindText(trivia.RawKind)} {trivia.FullSpan}",
                Foreground = Brushes.OrangeRed
            };
            return item;
        }
    }
}
