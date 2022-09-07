using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
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
    private ILexerSimulator _lexerSimulator;
    private ILanguageParserSimulator _languageParserSimulator;

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
            var extension = Path.GetExtension(dialog.FileName);
            if (
                Simulator.TryGetLexerSimulatorByFileExtension(extension, out var lexerSimulators) &&
                Simulator.TryGetLanguageParserSimulatorByFileExtension(extension, out var languageParserSimulators)
            )
            {
                this._lexerSimulator = lexerSimulators[0];
                this._languageParserSimulator = languageParserSimulators[0];
                var sourceText = SourceText.From(dialog.OpenFile());
                var tree = this._languageParserSimulator.ParseSyntaxTree(sourceText);
                this.FillinSyntaxTree(tree);
            }
        }
    }

    private void FillinSyntaxTree(SyntaxTree tree)
    {
        this.treeView.Items.Clear();
        this.treeView.Items.Add(ProcessNodeOrToken(tree.GetRoot()));

        TreeViewItem ProcessNodeOrToken(SyntaxNodeOrToken nodeOrToken)
        {
            var item = new TreeViewItem
            {
                Header = $"{this._languageParserSimulator.GetKindText(nodeOrToken.RawKind)} {nodeOrToken.FullSpan}",
                Foreground = nodeOrToken.IsNode ? Brushes.DarkBlue : Brushes.DarkGreen
            };

            if (nodeOrToken.IsToken)
            {
                var token = (SyntaxToken)nodeOrToken;
                item.Tag = token;
                foreach (var trivia in token.LeadingTrivia)
                    item.Items.Add(ProcessTrivia(trivia, trailing: false));
                foreach (var trivia in token.TrailingTrivia)
                    item.Items.Add(ProcessTrivia(trivia, trailing: true));
            }
            else if (nodeOrToken.IsNode)
            {
                var node = (SyntaxNode)nodeOrToken!;
                item.Tag = node;
                foreach (var child in node.ChildNodesAndTokens())
                    item.Items.Add(ProcessNodeOrToken(child));
            }
            return item;
        }
        TreeViewItem ProcessTrivia(SyntaxTrivia trivia, bool trailing)
        {
            var item = new TreeViewItem
            {
                Header = $"{(trailing ? "Trail" : "Lead")}: {this._languageParserSimulator.GetKindText(trivia.RawKind)} {trivia.FullSpan}",
                Foreground = Brushes.OrangeRed,
                Tag = trivia
            };
            return item;
        }
    }

    private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var nodeOrTokenOrTrivia = (e.NewValue as TreeViewItem)?.Tag;
        Debug.Assert(nodeOrTokenOrTrivia is not null);
        var basicProperties = new Dictionary<string, string>();
        var properties = nodeOrTokenOrTrivia.GetType()
            .GetProperties()
            .Where(pi => pi.CanRead && !pi.IsSpecialName)
            .OrderBy(pi => pi.Name)
            .ToDictionary(
                pi => pi.Name,
                pi => pi.GetValue(nodeOrTokenOrTrivia)?.ToString()
            );
        if (nodeOrTokenOrTrivia is SyntaxNode)
        {
            var node = (SyntaxNode)nodeOrTokenOrTrivia;
            basicProperties.Add("类型", node.GetType().Name);
            basicProperties.Add("种类", this._languageParserSimulator.GetKindText(node.RawKind));
        }
        else if (nodeOrTokenOrTrivia is SyntaxToken)
        {
            var token = (SyntaxToken)nodeOrTokenOrTrivia;
            basicProperties.Add("类型", nameof(SyntaxToken));
            basicProperties.Add("种类", this._languageParserSimulator.GetKindText(token.RawKind));
        }
        else if (nodeOrTokenOrTrivia is SyntaxTrivia)
        {
            var trivia = (SyntaxTrivia)nodeOrTokenOrTrivia;
            basicProperties.Add("类型", nameof(SyntaxTrivia));
            basicProperties.Add("种类", this._languageParserSimulator.GetKindText(trivia.RawKind));
        }

        UpdateBasicProperties();
        UpdateProperties();

        void UpdateBasicProperties()
        {
            this.gridBasicProperties.RowDefinitions.Clear();
            this.gridBasicProperties.Children.Clear();
            var index = 0;
            foreach (var basicProperty in basicProperties)
            {
                var key = new TextBlock()
                {
                    Text = basicProperty.Key,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                this.gridBasicProperties.Children.Add(key);
                Grid.SetRow(key, index);
                Grid.SetColumn(key, 0);

                var value = new TextBlock() { Text = basicProperty.Value };
                this.gridBasicProperties.Children.Add(value);
                Grid.SetRow(value, index);
                Grid.SetColumn(value, 1);

                index++;
            }
            for (var i = 0; i < index; i++)
            {
                this.gridBasicProperties.RowDefinitions.Add(new() { Height = new(18D) });
            }
        }
        void UpdateProperties()
        {
            this.gridProperties.RowDefinitions.Clear();
            this.gridProperties.Children.Clear();
            var index = 0;
            foreach (var property in properties)
            {
                if (property.Value is null) continue;

                var key = new TextBlock()
                {
                    Text = property.Key,
                    Padding = new(2D, 1D, 20D, 1D)
                };
                this.gridProperties.Children.Add(key);
                Grid.SetRow(key, index);
                Grid.SetColumn(key, 0);

                var value = new TextBlock()
                {
                    Text = property.Value,
                    Padding = new(2D, 1D, 2D, 1D)
                };
                this.gridProperties.Children.Add(value);
                Grid.SetRow(value, index);
                Grid.SetColumn(value, 1);

                index++;
            }
            for (var i = 0; i < index; i++)
            {
                this.gridProperties.RowDefinitions.Add(new() { Height = new(18D) });
            }
        }
    }
}
