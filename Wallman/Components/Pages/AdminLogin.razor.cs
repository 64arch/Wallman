using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Wallman.Components.Pages;

class AdminLoginModel {
    public string AdminEmail { get; set; }
    public string AdminPass { get; set; }
}

public partial class AdminLogin : ComponentBase {
    
    [Inject] private IOptions<AdminLoginModel> adminSettings { get; set; }
    [Parameter] public EventCallback<bool> OnLogin { get; set; }

    
    private AdminLoginModel adminModel = new();
    private bool isLoggedIn = false;
    private string _statusMessage;
    private bool _isError;
    
    private async Task HandleLogin()
    {
        if (adminModel.AdminEmail != adminSettings.Value.AdminEmail || adminModel.AdminPass != adminSettings.Value.AdminPass) {
            _statusMessage = "Ошибка: Неправильная почта или пароль.";
            _isError = true;
            return;
        }
        else {
            isLoggedIn = true;
            await OnLogin.InvokeAsync(isLoggedIn); 
        }
    }
}