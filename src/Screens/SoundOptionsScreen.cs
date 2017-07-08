using System.Globalization;
using Warcraft.App;
using Warcraft.Audio;
using Warcraft.Screens.Base;
using Warcraft.Screens.Components;

namespace Warcraft.Screens
{
	public class SoundOptionsScreen : OptionsScreen
	{
		private readonly TextField sfxVolumeTextField;
		private readonly TextField musicVolumeTextField;

		public SoundOptionsScreen()
		{
			Title = "Sound Options";

			var okButton = new Button("OK", true);
			okButton.OnClick += OkButton_OnClick;
			Buttons.Add(okButton);

			var cancelButton = new Button("Cancel");
			cancelButton.OnClick += CancelButton_OnClick;
			Buttons.Add(cancelButton);

			sfxVolumeTextField = new TextField("SFX Volume:", (Settings.General.SfxVolume * 100).ToString(CultureInfo.InvariantCulture));
			sfxVolumeTextField.Validator += VolumeLevelValidator;
			TextFields.Add(sfxVolumeTextField);

			musicVolumeTextField = new TextField("Music Volume:", (Settings.General.MusicVolume * 100).ToString(CultureInfo.InvariantCulture));
			musicVolumeTextField.Validator += VolumeLevelValidator;
			TextFields.Add(musicVolumeTextField);
		}

		private bool VolumeLevelValidator(string text)
		{
			int level;
			if (int.TryParse(text, out level) && level >= 0 && level <= 100)
				return level.ToString() == text;

			return false;
		}

		private void OkButton_OnClick()
		{
			Settings.General.SfxVolume = int.Parse(sfxVolumeTextField.Text) / 100f;
			Settings.General.MusicVolume = int.Parse(musicVolumeTextField.Text) / 100f;

			// Linux: The Linux version does not seem to update the SoundEventRenderer's
			//        volume level. That means it will always play at default volume.
			Data.SoundEventRenderer.Volume = Settings.General.SfxVolume;
			AudioManager.SetVolume(Settings.General.MusicVolume);

			IsExiting = true;
		}

		private void CancelButton_OnClick() { IsExiting = true; }
	}
}