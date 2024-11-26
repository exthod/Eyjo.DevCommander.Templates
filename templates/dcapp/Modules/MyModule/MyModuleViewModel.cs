using Eyjo.DevCommander.Abstractions;
using Eyjo.DevCommander.Mvvm;
using Eyjo.Toolkit.Mvvm;
using Microsoft.Extensions.Logging;

namespace MyDevCommanderApp.Modules.MyModule
{
	public class MyModuleViewModel : DevCommanderViewModelBase, IViewInteractionViewModel
	{
		private readonly ICommandProvider _commandProvider;
		private readonly ILogger<MyModuleViewModel> _logger;
		private readonly IShell _shell;

		public MyModuleViewModel(ILogger<MyModuleViewModel> logger, IShell shell, ICommandProvider commandProvider)
		{
			_logger = logger;
			_shell = shell;
			_commandProvider = commandProvider;

			Title = "DevCommanderModuleName";
			InputActionCommand = _commandProvider.CreateDelegateCommand(InputActionCommand_Execute, InputActionCommand_CanExecute);
		}

		public string InputField
		{
			get { return _inputField; }
			set { SetField(ref _inputField, value, changeAction: UpdateViewButtons); }
		}
		private string _inputField;

		public IDelegateCommand InputActionCommand { get; }

		private void InputActionCommand_Execute(object obj)
		{
			_logger.LogInformation("Action was pressed.");
			_shell.DisplayMessage("Action pressed...");
		}

		private bool InputActionCommand_CanExecute(object obj)
		{
			return InputField?.Length > 0;
		}

		private void UpdateViewButtons()
		{
			RaiseCanExecute(InputActionCommand);
		}
	}
}
