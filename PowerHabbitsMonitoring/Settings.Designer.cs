﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PowerHabbitsMonitoring {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.4.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\dno1694\\source\\repos\\PowerHabbitsMonitoring\\PowerHabbitsMonitoring\\bin\\D" +
            "ebug")]
        public string DLLDirectory {
            get {
                return ((string)(this["DLLDirectory"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\dno1694\\source\\repos\\PowerHabbitsMonitoring\\PowerHabbitsMonitoring\\bin\\D" +
            "ebug\\activity.txt")]
        public string InactiveTimeFile {
            get {
                return ((string)(this["InactiveTimeFile"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\dno1694\\source\\repos\\PowerHabbitsMonitoring\\PowerHabbitsMonitoring\\bin\\D" +
            "ebug\\StatusCache.txt")]
        public string StatusCache {
            get {
                return ((string)(this["StatusCache"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://ws000webdev1.tcad.telia.se/PowerConsumptionMonitor/api/PowerConsumption")]
        public string ApiURL {
            get {
                return ((string)(this["ApiURL"]));
            }
            set {
                this["ApiURL"] = value;
            }
        }
    }
}
