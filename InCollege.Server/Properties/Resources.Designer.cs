﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InCollege.Server.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("InCollege.Server.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;center&gt;Авторизация&lt;/center&gt;
        ///&lt;ol&gt;
        /// &lt;li&gt;SignIn&lt;/li&gt;
        /// Авторизирует пользователя. Возвращает токен для доступа к данным. Параметры:
        /// &lt;ul&gt;
        /// &lt;li&gt;UserName - имя пользователя&lt;/li&gt;
        /// &lt;li&gt;Password - пароль&lt;/li&gt;
        /// &lt;/ul&gt;
        ///
        /// &lt;li&gt;SignUp&lt;/li&gt;
        /// Регистрирует пользователя:
        /// &lt;ul&gt;
        /// &lt;li&gt;UserName - имя пользователя&lt;/li&gt;
        /// &lt;li&gt;Password - пароль&lt;/li&gt;
        /// &lt;li&gt;AccountType - тип аккаунта(в числовом представлении)&lt;/li&gt;
        /// &lt;li&gt;BirthDate - дата рождения&lt;/li&gt; 
        /// &lt;li&gt;FullName - ФИО&lt;/li&gt; 
        /// &lt;li&gt;ProfileImage - изображение профиля(фун [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AuthPage {
            get {
                return ResourceManager.GetString("AuthPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Все методы работают через POST, т.к. кэширование для API не является необходимым.
        ///
        ///Для доступа к любому методу нужен токен авторизации, полученный при &lt;a href=&quot;/?Auth=1&quot;&gt;авторизации&lt;/a&gt;.
        ///Передавать его следует параметром token=&quot;содержимое_токена&quot;.
        ///
        ///Существуют такие методы:
        ///
        ///&lt;ol&gt;
        /// &lt;li&gt;GetRange&lt;/li&gt;
        /// Получает диапазон данных. Параметры:
        /// &lt;ul&gt;
        /// &lt;li&gt;table - какие данные нужны&lt;/li&gt;
        /// &lt;li&gt;skipRecords - сколько записей необходимо пропустить&lt;/li&gt;
        /// &lt;li&gt;countRecords - сколько записей запрашивается с 0-й л [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DataPage {
            get {
                return ResourceManager.GetString("DataPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;center&gt;&lt;h1&gt;&lt;b&gt;Добро пожаловать&lt;/b&gt;&lt;/h1&gt;&lt;/center&gt;
        ///&lt;center&gt;&lt;h2&gt;&lt;b&gt;На сервер системы InCollege&lt;/b&gt;&lt;/h2&gt;&lt;/center&gt;
        ///&lt;center&gt;&lt;a href=&quot;/?Auth=1&quot;&gt;Авторизация&lt;/a&gt;&lt;/center&gt;
        ///&lt;center&gt;&lt;a href=&quot;/?Data=1&quot;&gt;Данные&lt;/a&gt;&lt;/center&gt;
        ///&lt;a style=&quot;position:fixed; bottom:0; height:auto; margin-top:40px; width: 100%; text-align:center&quot;&gt;Made by [CYBOR]&lt;/a&gt;
        ///&lt;a style=&quot;position:fixed; bottom:0; height:auto; margin-top:40px; width: 100%; text-align:right&quot;&gt;{Version}&lt;/a&gt;.
        /// </summary>
        internal static string HomePage {
            get {
                return ResourceManager.GetString("HomePage", resourceCulture);
            }
        }
    }
}
