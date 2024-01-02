using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.Dashboard;

public class DialogViewModel : ObservableObject
{
    public void CloseDialog()
    {
        SukiHost.CloseDialog();
    }
}