using System;
using System.ComponentModel;
using System.Windows;

namespace JaaS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainViewModel Model;


    public MainWindow()
    {
        InitializeComponent();
    }
    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
        if (Model != null)
            Model.Dispose();
    }
    private void Activate_Click(object sender, RoutedEventArgs e)
    {
        Model.ActivateRecognition();
    }
}
