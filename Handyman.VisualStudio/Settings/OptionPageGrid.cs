using System.ComponentModel;
using Handyman.Settings;
using Microsoft.VisualStudio.Shell;

namespace CommerceRuntimeHandyman.Settings
{
    public class OptionPageGrid : DialogPage, IGlobalSettings
    {
        internal const string Category = "CommerceRuntimeHandyman";
        internal const string Name = "Options";

        private const string InterfacesCategoryName = "Interface names";

        private Handyman.Settings.GlobalSettings settings = new Handyman.Settings.GlobalSettings();

        [Category(InterfacesCategoryName)]
        [DisplayName("Request interface name")]
        [Description("The full qualified name of the interface that represents a request message.")]
        public string RequestInterfaceFQN
        {
            get { return this.settings.RequestInterfaceFQN; }
            set { this.settings.RequestInterfaceFQN = value; }
        }

        [Category(InterfacesCategoryName)]
        [DisplayName("Response interface name")]
        [Description("The full qualified name of the interface that represents a response message.")]
        public string ResponseInterfaceFQN
        {
            get { return this.settings.ResponseInterfaceFQN; }
            set { this.settings.ResponseInterfaceFQN = value; }
        }

        [Category(InterfacesCategoryName)]
        [DisplayName("Request handler interface name")]
        [Description("The full qualified name of the interface that represents a request handler.")]
        public string HandlerInterfaceFQN
        {
            get { return this.settings.HandlerInterfaceFQN; }
            set { this.settings.HandlerInterfaceFQN = value; }
        }

        public void UpdateSettings()
        {
            Handyman.Settings.SettingsManager.UpdateSettings(this.settings);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                this.UpdateSettings();
            }
        }
    }
}