using System.Security.Cryptography;
using System.Text;
using DevToys.Api;
using DevToys.Core.Settings;

namespace DevToys.Blazor.BuiltInTools.PasswordGenerator;

[Export(typeof(IGuiTool))]
[Name("Password Generator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF5AC', // Lock icon
    GroupName = PredefinedCommonToolGroupNames.Generators,
    ResourceManagerAssemblyIdentifier = nameof(DevToysBlazorResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Blazor.BuiltInTools.PasswordGenerator.PasswordGenerator",
    ShortDisplayTitleResourceName = nameof(PasswordGenerator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(PasswordGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(PasswordGenerator.Description),
    AccessibleNameResourceName = nameof(PasswordGenerator.AccessibleName),
    SearchKeywordsResourceName = nameof(PasswordGenerator.SearchKeywords))]
[Order(0)] // First in Generator group
internal sealed class PasswordGeneratorGuiTool : IGuiTool
{
    private enum GridRows
    {
        Configuration,
        Output
    }

    private readonly IUINumberInput _passwordLengthInput = NumberInput("password-length-input");
    private readonly IUISwitch _includeLowercaseSwitch = Switch("include-lowercase-switch");
    private readonly IUISwitch _includeUppercaseSwitch = Switch("include-uppercase-switch");
    private readonly IUISwitch _includeDigitsSwitch = Switch("include-digits-switch");
    private readonly IUISwitch _includeSpecialSwitch = Switch("include-special-switch");
    private readonly IUISingleLineTextInput _excludeCharactersInput = SingleLineTextInput("exclude-characters-input");
    private readonly IUINumberInput _passwordCountInput = NumberInput("password-count-input");
    private readonly IUIMultiLineTextInput _outputTextArea = MultiLineTextInput("output-text-area");

    private readonly IUIButton _generateButton = Button("generate-button");
    private readonly IUIButton _copyButton = Button("copy-button");

    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IClipboard _clipboard = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    public UIToolView View
        => new(
            Grid()
                .RowLargeSpacing()

                .Rows(
                    (GridRows.Configuration, Auto),
                    (GridRows.Output, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.Configuration,

                        Stack()
                            .Vertical()
                            .LargeSpacing()
                            .WithChildren(

                                // Configuration Section
                                Stack()
                                    .Vertical()
                                    .MediumSpacing()
                                    .WithChildren(

                                        Label()
                                            .Text(PasswordGenerator.ConfigurationSectionTitle),

                                        // Length Configuration
                                        Setting("password-length-setting")
                                            .Icon("FluentSystemIcons", '\uF4A5')
                                            .Title(PasswordGenerator.LengthTitle)
                                            .Description(PasswordGenerator.LengthDescription)
                                            .InteractiveElement(
                                                _passwordLengthInput
                                                    .Value(30)
                                                    .Minimum(1)
                                                    .Maximum(128)
                                                    .OnTextChanged(OnSettingChanged)),

                                        // Character Types Configuration
                                        SettingGroup("character-types-setting")
                                            .Icon("FluentSystemIcons", '\uF4B8')
                                            .Title(PasswordGenerator.CharacterTypesTitle)
                                            .Description(PasswordGenerator.CharacterTypesDescription)
                                            .WithSettings(

                                                Setting("include-lowercase-setting")
                                                    .Title(PasswordGenerator.IncludeLowercaseTitle)
                                                    .InteractiveElement(
                                                        _includeLowercaseSwitch
                                                            .On()
                                                            .OnToggle(OnSettingChanged)),

                                                Setting("include-uppercase-setting")
                                                    .Title(PasswordGenerator.IncludeUppercaseTitle)
                                                    .InteractiveElement(
                                                        _includeUppercaseSwitch
                                                            .On()
                                                            .OnToggle(OnSettingChanged)),

                                                Setting("include-digits-setting")
                                                    .Title(PasswordGenerator.IncludeDigitsTitle)
                                                    .InteractiveElement(
                                                        _includeDigitsSwitch
                                                            .On()
                                                            .OnToggle(OnSettingChanged)),

                                                Setting("include-special-setting")
                                                    .Title(PasswordGenerator.IncludeSpecialTitle)
                                                    .InteractiveElement(
                                                        _includeSpecialSwitch
                                                            .On()
                                                            .OnToggle(OnSettingChanged))),

                                        // Exclude Characters Configuration
                                        Setting("exclude-characters-setting")
                                            .Icon("FluentSystemIcons", '\uF36E')
                                            .Title(PasswordGenerator.ExcludeCharactersTitle)
                                            .Description(PasswordGenerator.ExcludeCharactersDescription)
                                            .InteractiveElement(
                                                _excludeCharactersInput
                                                    .Placeholder(PasswordGenerator.ExcludeCharactersPlaceholder)
                                                    .OnTextChanged(OnSettingChanged)),

                                        // Count Configuration
                                        Setting("password-count-setting")
                                            .Icon("FluentSystemIcons", '\uF4A3')
                                            .Title(PasswordGenerator.CountTitle)
                                            .Description(PasswordGenerator.CountDescription)
                                            .InteractiveElement(
                                                _passwordCountInput
                                                    .Value(1)
                                                    .Minimum(1)
                                                    .Maximum(100)
                                                    .OnTextChanged(OnSettingChanged)),

                                        // Generate Button
                                        Stack()
                                            .Horizontal()
                                            .SmallSpacing()
                                            .WithChildren(
                                                _generateButton
                                                    .Icon("FluentSystemIcons", '\uF5B2')
                                                    .Text(PasswordGenerator.GenerateButtonText)
                                                    .OnClick(OnGenerateButtonClickAsync),

                                                _copyButton
                                                    .Icon("FluentSystemIcons", '\uF32B')
                                                    .Text(PasswordGenerator.CopyButtonText)
                                                    .OnClick(OnCopyButtonClickAsync)))),

                    Cell(
                        GridRows.Output,

                        Stack()
                            .Vertical()
                            .MediumSpacing()
                            .WithChildren(

                                Label()
                                    .Text(PasswordGenerator.OutputSectionTitle),

                                _outputTextArea
                                    .ReadOnly()
                                    .Placeholder(PasswordGenerator.OutputPlaceholder)))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private void OnSettingChanged()
    {
        // Auto-generate when settings change
        OnGenerateButtonClickAsync().Forget();
    }

    private async ValueTask OnGenerateButtonClickAsync()
    {
        try
        {
            int length = (int)_passwordLengthInput.Value;
            bool includeLowercase = _includeLowercaseSwitch.IsOn;
            bool includeUppercase = _includeUppercaseSwitch.IsOn;
            bool includeDigits = _includeDigitsSwitch.IsOn;
            bool includeSpecial = _includeSpecialSwitch.IsOn;
            string excludeCharacters = _excludeCharactersInput.Text ?? string.Empty;
            int count = (int)_passwordCountInput.Value;

            // Build character set
            var characterSet = new StringBuilder();
            
            if (includeLowercase) characterSet.Append(LowercaseChars);
            if (includeUppercase) characterSet.Append(UppercaseChars);
            if (includeDigits) characterSet.Append(DigitChars);
            if (includeSpecial) characterSet.Append(SpecialChars);

            string availableChars = characterSet.ToString();

            // Remove excluded characters
            if (!string.IsNullOrEmpty(excludeCharacters))
            {
                foreach (char excludeChar in excludeCharacters)
                {
                    availableChars = availableChars.Replace(excludeChar.ToString(), string.Empty);
                }
            }

            if (string.IsNullOrEmpty(availableChars))
            {
                _outputTextArea.Text(PasswordGenerator.NoCharactersAvailableError);
                return;
            }

            // Generate passwords
            var passwords = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string password = GenerateSecurePassword(availableChars, length);
                passwords.Add(password);
            }

            _outputTextArea.Text(string.Join(Environment.NewLine, passwords));
        }
        catch (Exception ex)
        {
            _outputTextArea.Text($"{PasswordGenerator.GenerationErrorPrefix} {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async ValueTask OnCopyButtonClickAsync()
    {
        string output = _outputTextArea.Text ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(output))
        {
            await _clipboard.SetClipboardTextAsync(output);
        }
    }

    private static string GenerateSecurePassword(string availableChars, int length)
    {
        if (string.IsNullOrEmpty(availableChars))
            throw new ArgumentException("No characters available for password generation");

        if (length <= 0)
            throw new ArgumentException("Password length must be greater than 0");

        var password = new StringBuilder(length);
        
        using (var rng = RandomNumberGenerator.Create())
        {
            var randomBytes = new byte[length * 4]; // Get extra bytes to ensure sufficient randomness
            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                // Convert 4 bytes to int and use modulo to get index
                int randomInt = BitConverter.ToInt32(randomBytes, i * 4);
                int index = Math.Abs(randomInt) % availableChars.Length;
                password.Append(availableChars[index]);
            }
        }

        return password.ToString();
    }
}