using Eyjo.DevCommander.Abstractions;
using Eyjo.DevCommander.Mvvm;
using Eyjo.Toolkit.Mvvm;
using Microsoft.Extensions.Logging;

namespace DevCommanderModuleNameApp.Modules.DevCommanderModuleName
{
	public class DevCommanderModuleNameViewModel : DevCommanderViewModelBase, IViewInteractionViewModel
	{
		private readonly ICommandProvider _commandProvider;
		private readonly ILogger<DevCommanderModuleNameViewModel> _logger;
		private readonly IViewContext _viewContext;
		private readonly DevCommanderModuleNameSettings _settings;

		public DevCommanderModuleNameViewModel(ILogger<DevCommanderModuleNameViewModel> logger, IViewContext viewContext, ICommandProvider commandProvider)
		{
			_logger = logger;
			_viewContext = viewContext;
			_commandProvider = commandProvider;

			Title = "DevCommanderModuleName";
			SaveCommand = _commandProvider.CreateDelegateCommand(SaveCommand_Execute, SaveCommand_CanExecute);

			PropertyChanged += (s, e) =>
			{
				// Whenever a property changes, we check if there are any validation errors and update the buttons accordingly.
				if (e.PropertyName == nameof(HasErrors)) UpdateViewButtons();
			};

			_settings = _viewContext.GetSettings<DevCommanderModuleNameSettings>();
			SampleString = _settings.SampleSetting1;
			SampleInteger = _settings.SampleSetting2;
		}

		public string SampleString
		{
			get;
			set 
			{
				SetField(ref field, value, changeAction: UpdateViewButtons); 
				_settings.SampleSetting1 = field;
			}
		}

		public int SampleInteger
		{
			get;
			set 
			{
				SetField(ref field, value, changeAction: UpdateViewButtons);
				_settings.SampleSetting2 = field;
			}
		}

		public IDelegateCommand SaveCommand { get; }

		/// <inheritdoc/>
		protected override string DoValidateProperties(string propertyName)
		{
			return propertyName switch
			{
				nameof(SampleString) => string.IsNullOrEmpty(SampleString) ? "Sample String cannot be empty." : null,
				_ => null
			};
		}

		private void SaveCommand_Execute(object obj)
		{
			_logger.LogInformation("Saving settings.");
			_viewContext.SetSettings(_settings);
			_viewContext.DisplayMessage("Settings saved!", Title);
		}

		private bool SaveCommand_CanExecute(object obj) => !HasErrors;

		private void UpdateViewButtons()
		{
			RaiseCanExecute(SaveCommand);
		}
	}
}
