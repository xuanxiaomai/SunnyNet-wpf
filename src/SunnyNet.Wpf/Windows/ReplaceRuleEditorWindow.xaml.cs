using System.Windows;
using Microsoft.Win32;
using SunnyNet.Wpf.Models;

namespace SunnyNet.Wpf.Windows;

public partial class ReplaceRuleEditorWindow : Window
{
    private readonly ReplaceRuleItem _item;
    private readonly bool _isNew;

    public event EventHandler? RuleAccepted;

    public ReplaceRuleEditorWindow(ReplaceRuleItem item, bool isNew)
    {
        InitializeComponent();
        _item = item;
        _isNew = isNew;
        Title = isNew ? "替换规则设置" : "编辑替换规则";
        DataKindComboBox.ItemsSource = RuleDataKinds.All;
        DataKindComboBox.SelectedItem = RuleDataKinds.Normalize(item.DataKind);
        bool allRequests = string.IsNullOrWhiteSpace(item.MatchTarget);
        AllRequestsRadio.IsChecked = allRequests;
        UrlHostRadio.IsChecked = !allRequests;
        MatchTargetTextBox.Text = item.MatchTarget;
        ReplaceAllRadio.IsChecked = item.ReplaceAll;
        ReplacePartRadio.IsChecked = !item.ReplaceAll;
        UseHexCheckBox.IsChecked = item.UseHex;
        SourceTextBox.Text = item.SourceContent;
        bool fromFile = item.UsesResponseFile;
        FileDataRadio.IsChecked = fromFile;
        DirectDataRadio.IsChecked = !fromFile;
        if (fromFile)
        {
            FilePathTextBox.Text = item.ReplacementContent;
        }
        else
        {
            ReplacementTextBox.Text = item.ReplacementContent;
        }

        UpdateMatchMode();
        UpdateReplacementMode();
    }

    private void MatchMode_Changed(object sender, RoutedEventArgs routedEventArgs)
    {
        UpdateMatchMode();
    }

    private void ReplacementMode_Changed(object sender, RoutedEventArgs routedEventArgs)
    {
        UpdateReplacementMode();
    }

    private void UpdateMatchMode()
    {
        if (MatchTargetTextBox is null || UrlHostRadio is null)
        {
            return;
        }

        MatchTargetTextBox.IsEnabled = UrlHostRadio.IsChecked == true;
    }

    private void UpdateReplacementMode()
    {
        if (FileDataRadio is null || FilePathTextBox is null || BrowseFileButton is null || FileHexCheckBox is null || ReplacementTextBox is null)
        {
            return;
        }

        bool fromFile = FileDataRadio.IsChecked == true;
        FilePathTextBox.IsEnabled = fromFile;
        BrowseFileButton.IsEnabled = fromFile;
        FileHexCheckBox.IsEnabled = fromFile;
        ReplacementTextBox.IsEnabled = !fromFile;
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        OpenFileDialog dialog = new()
        {
            Title = "选择替换文件"
        };
        if (dialog.ShowDialog(this) == true)
        {
            FilePathTextBox.Text = dialog.FileName;
        }
    }

    private void Accept_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        _item.MatchTarget = AllRequestsRadio.IsChecked == true ? "" : MatchTargetTextBox.Text.Trim();
        _item.DataKind = DataKindComboBox.SelectedItem?.ToString() ?? RuleDataKinds.Default;
        _item.ReplaceAll = ReplaceAllRadio.IsChecked == true;
        _item.UseHex = UseHexCheckBox.IsChecked == true;
        _item.SourceContent = SourceTextBox.Text ?? "";
        if (FileDataRadio.IsChecked == true)
        {
            _item.RuleType = "响应文件";
            _item.ReplacementContent = FilePathTextBox.Text.Trim();
        }
        else
        {
            _item.RuleType = UseHexCheckBox.IsChecked == true ? "HEX" : "String(UTF8)";
            _item.ReplacementContent = ReplacementTextBox.Text ?? "";
        }

        if (string.IsNullOrWhiteSpace(_item.SourceContent) && !_item.ReplaceAll)
        {
            MessageBox.Show(this, "请填写被替换内容。", "替换规则", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RuleAccepted?.Invoke(this, EventArgs.Empty);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        DialogResult = false;
        Close();
    }
}
