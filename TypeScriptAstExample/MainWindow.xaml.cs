using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Zu.TypeScript;
using Zu.TypeScript.Change;
using Zu.TypeScript.TsTypes;

namespace TypeScriptAstExample
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<NodeChangeItem> _nodeChangeItems =
            new ObservableCollection<NodeChangeItem>();

        private TypeScriptAST _currentAst;
        private ChangeAST _currentChangeAst;
        private string _currentSource;
        private string _currentSourceFileName;

        public MainWindow()
        {
            InitializeComponent();
            lbChanges.ItemsSource = _nodeChangeItems;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            _nodeChangeItems.Clear();
            _currentChangeAst = null;
            _currentAst = new TypeScriptAST();
            _currentSource = tbSource.Text;
            _currentSourceFileName = tbFileName.Text;
            if (string.IsNullOrWhiteSpace(_currentSourceFileName)) _currentAst.MakeAST(_currentSource);
            else _currentAst.MakeAST(_currentSource, _currentSourceFileName);

            tbTreeString.Text = _currentAst.GetTreeString();
            lbNodes.ItemsSource = _currentAst.GetDescendants();
        }

        private void lbNodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var node = lbNodes.SelectedItem as Node;
            if (node != null)
            {
                tbNodeText.Text = node.GetTextWithComments();
                tbTreeString.Text = node.GetTreeString();
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            tbNewSource.Text = _currentChangeAst.GetChangedSource(_currentAst.SourceStr);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var node = lbNodes.SelectedItem as Node;
            if (node != null)
                try
                {
                    if (_currentChangeAst == null) _currentChangeAst = new ChangeAST(_nodeChangeItems);
                    _currentChangeAst.ChangeNode(node, tbNodeText.Text);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var node = lbNodes.SelectedItem as Node;
            if (node != null)
                try
                {
                    if (_currentChangeAst == null) _currentChangeAst = new ChangeAST(_nodeChangeItems);
                    _currentChangeAst.InsertBefore(node, tbNodeText.Text);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var node = lbNodes.SelectedItem as Node;
            if (node != null)
                try
                {
                    if (_currentChangeAst == null) _currentChangeAst = new ChangeAST(_nodeChangeItems);
                    _currentChangeAst.InsertAfter(node, tbNodeText.Text);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var node = lbNodes.SelectedItem as Node;
            if (node != null)
                try
                {
                    if (_currentChangeAst == null) _currentChangeAst = new ChangeAST(_nodeChangeItems);
                    _currentChangeAst.Delete(node);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            var searchStr = tbSearchStr.Text.ToLower();
            var ind = lbNodes.SelectedIndex + 1;
            while (ind < lbNodes.Items.Count &&
                   (lbNodes.Items[ind] as Node)?.ToString().ToLower().Contains(searchStr) != true) ind++;
            if (ind < lbNodes.Items.Count) lbNodes.SelectedIndex = ind;
            if (lbNodes.SelectedItem != null)
                lbNodes.ScrollIntoView(lbNodes.SelectedItem);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _nodeChangeItems.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                tbSource.Text = File.ReadAllText(fileName);
                tbFileName.Text = Path.GetFileName(fileName);
            }
        }

        private void lbChanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ch = lbChanges.SelectedItem as NodeChangeItem;
            if (ch == null) return;
            for (var i = 0; i < lbNodes.Items.Count; i++)
                if (lbNodes.Items[i] as Node == ch.Node)
                {
                    lbNodes.SelectedIndex = i;
                    break;
                }
            if (ch.NewValue != null)
                tbNodeText.Text = ch.NewValue;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            var fileName = "parser.ts";
            if (!File.Exists(fileName))
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    fileName = openFileDialog.FileName;
                else
                    return;
            }

            var source = File.ReadAllText(fileName);

            var ast = new TypeScriptAST(source, fileName);

            var change = new ChangeAST();

            foreach (var module in ast.GetDescendants().OfType<ModuleDeclaration>())
            {
                var funcs = module.Body.Children.OfType<FunctionDeclaration>().ToList();
                var enums = module.Body.Children.OfType<EnumDeclaration>();
                var moduleInfoFunc = $@"
    export function getModuleInfo() {{
        return ""Module {module.IdentifierStr} contains {funcs.Count()} functions ({
                        funcs.Count(v => v.IdentifierStr.StartsWith("parse"))
                    } starts with parse), {enums.Count()} enums ..."";
    }}
";
                change.InsertBefore(module.Body.Children.First(), moduleInfoFunc);
            }
            var newSource = change.GetChangedSource(ast.SourceStr);
            tbExample2Res.Text = newSource;
            File.WriteAllText("parser2.ts", newSource);
        }
    }
}