using System.Windows;
using SunnyNet.Wpf.Models;

namespace SunnyNet.Wpf.Windows;

public partial class InterceptRuleEditorWindow : Window
{
    public string Contains { get; private set; } = "包含";
    public string Target { get; private set; } = "";
    public string InterceptType { get; private set; } = RuleDataKinds.Default;
    public bool SuspendProcess { get; private set; }

    public InterceptRuleEditorWindow(string? contains = null, string? target = null, string? interceptType = null, bool suspendProcess = false)
    {
        InitializeComponent();
        ContainsComboBox.ItemsSource = new[] { "包含", "通配", "等于", "正则" };
        InterceptTypeComboBox.ItemsSource = RuleDataKinds.All;
        ContainsComboBox.SelectedItem = string.IsNullOrWhiteSpace(contains) ? "包含" : contains;
        InterceptTypeComboBox.SelectedItem = RuleDataKinds.Normalize(interceptType);
        TargetTextBox.Text = target ?? "";
        SuspendCheckBox.IsChecked = suspendProcess || interceptType == "请求断点";
        Title = string.IsNullOrWhiteSpace(target) ? "拦截规则设置" : "编辑拦截规则";
    }

    private void Accept_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        Target = TargetTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(Target))
        {
            MessageBox.Show(this, "请填写拦截目标。", "拦截规则", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Contains = ContainsComboBox.SelectedItem?.ToString() ?? "包含";
        InterceptType = RuleDataKinds.Normalize(InterceptTypeComboBox.SelectedItem?.ToString());
        SuspendProcess = SuspendCheckBox.IsChecked == true;

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        DialogResult = false;
        Close();
    }
}
