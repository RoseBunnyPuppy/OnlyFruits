using StardewModdingAPI;
using StardewModdingAPI.Integrations.GenericModConfigMenu;

namespace OnlyFruitsMod.Features.UIHelpers
{
    /// <summary>
    ///     A wrapper class to allow defining menu chunks in 
    ///   a more fluent manner (as well as using a standardized 
    ///   format for the i18n subvalues)
    /// </summary>
    public class ConfigMenuHelper
    {
        private readonly IModHelper modHelper;
        public readonly IGenericModConfigMenuApi configMenu;
        public IManifest ModManifest { get; }

        public ConfigMenuHelper(
            IManifest modManifest,
            IModHelper modHelper,
            IGenericModConfigMenuApi configMenu
        )
        {
            this.ModManifest = modManifest;
            this.modHelper = modHelper;
            this.configMenu = configMenu;
        }

        /// <summary>
        ///   Invoke some code on the underlying config menu api
        /// </summary>
        public ConfigMenuHelper FluentBlock(Action<IGenericModConfigMenuApi> action)
        {
            action(this.configMenu);
            return this;
        }

        /// <summary>
        ///   Add a paragraph at the current position in the form.
        /// </summary>
        /// <param name="text">The paragraph text to display.</param>
        public ConfigMenuHelper AddParagraph(Func<string> text)
        {
            this.configMenu.AddParagraph(
                mod: this.ModManifest,
                text
            );
            return this;
        }

        /// <summary>
        ///   Add a paragraph at the current position in the form.
        /// </summary>
        /// <param name="i18nKeyName">The name of the i18n key to display</param>
        public ConfigMenuHelper AddParagraph(string i18nKey)
        {
            this.configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => this.modHelper.Translation.Get(i18nKey)
            );
            return this;
        }

        /// <summary>
        ///   Add a paragraph at the current position in the form.
        /// </summary>
        /// <param name="i18nKeyName">The name of the i18n key to display</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        public ConfigMenuHelper AddParagraph(
            string i18nKey,
            Func<object>? tokens
        )
        {
            this.configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => this.modHelper.Translation.Get(i18nKey, tokens?.Invoke())
            );
            return this;
        }


        /// <summary>
        ///   Add a section title at the current position in the form.
        /// </summary>
        /// <param name="i18nKeyName">The base name of the i18n key.  .label and .tooltip will be used</param>
        public ConfigMenuHelper AddSectionTitle(string i18nKeyName)
        {
            this.configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.modHelper.Translation.Get($"{i18nKeyName}.label"),
                tooltip: () => this.modHelper.Translation.Get($"{i18nKeyName}.tooltip")
            );
            return this;
        }


        /// <summary>
        ///   Add a boolean option at the current position in the form.
        /// </summary>
        /// <param name="i18nKeyName">The base name of the i18n key.  .label and .tooltip will be used</param>
        public ConfigMenuHelper AddBoolOption(
            string i18nKeyName,
            Func<bool> getValue,
            Action<bool> setValue
        )
        {
            this.configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.modHelper.Translation.Get($"{i18nKeyName}.label"),
                tooltip: () => this.modHelper.Translation.Get($"{i18nKeyName}.tooltip"),
                getValue: getValue,
                setValue: setValue
            );
            return this;
        }

        /// <summary>
        ///   Add a string option at the current position in the form.
        /// </summary>
        /// <param name="i18nKeyName">
        ///     The base name of the i18n key.  
        ///     .label and .tooltip will be used for the option
        ///     .choices.{{allowed_values}} will be used for the formatting of the options.
        /// </param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="allowedValues">The values that can be selected, or <c>null</c> to allow any.</param>
        /// <returns></returns>
        public ConfigMenuHelper AddTextOption(
            string i18nKeyName,
            Func<string> getValue, 
            Action<string> setValue,
            string[] allowedValues
        )
        {
            this.configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => this.modHelper.Translation.Get($"{i18nKeyName}.label"),
                tooltip: () => this.modHelper.Translation.Get($"{i18nKeyName}.tooltip"),
                getValue: getValue,
                setValue: setValue,
                allowedValues: allowedValues,
                formatAllowedValue: choice => this.modHelper.Translation.Get($"{i18nKeyName}.choices.{choice}")
            );
            return this;
        }

    }
}
